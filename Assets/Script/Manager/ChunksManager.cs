using Assets.Script.Common;
using Assets.Script.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Script.Manager
{
    public class ChunksManager : MonoBehaviour
    {
        private PlayerController targetPlayer;
        private Transform worldTrans;
        private Transform poolTrans;
        private Transform activeChunksTrans;
        
        private StackPool<ChunkController> pool;
        private Dictionary<Vector3Int, ChunkController> activeChunks;
        
        private Vector3Int lastStayChunkPos;
        private bool IsInit;
        private bool IsChunksLoading;
        private bool NeedInterruptAndRegenerate;

        protected override void OnAwake()
        {

        }

        public void Start()
        {
            worldTrans = transform.parent;

            var poolContainer = new GameObject("ChunkPool");
            poolContainer.transform.parent = worldTrans;
            poolTrans = poolContainer.transform;

            var chunkContainer = new GameObject("ActiveChunks");
            chunkContainer.transform.parent = worldTrans;
            activeChunksTrans = chunkContainer.transform;

            targetPlayer = GameManager.Instance.CurPlayerController;
            activeChunks = new Dictionary<Vector3Int, ChunkController>();
            IsInit = false;

            pool = new StackPool<ChunkController>(
                (int)Mathf.Pow((Consts.ViewDistanceInChunks + 1) * 2, 2),
                OnCreateChunk,
                OnGetChunkFromPool,
                OnReleaseChunkToPool,
                OnDestroyChunk);
        }

        #region pool event

        private ChunkController OnCreateChunk()
        {
            var handle = Addressables.InstantiateAsync("Common/Chunk");
            var chunkController = handle.WaitForCompletion().GetComponent<ChunkController>();
            OnGetChunkFromPool(chunkController);
            return chunkController;
        }

        private void OnGetChunkFromPool(ChunkController chunkController)
        {
            chunkController.gameObject.SetActive(true);
            chunkController.transform.SetParent(activeChunksTrans, false);
        }

        private void OnReleaseChunkToPool(ChunkController chunkController)
        {
            chunkController.gameObject.SetActive(false);
            chunkController.transform.parent = poolTrans;
        }

        private void OnDestroyChunk(ChunkController chunkController)
        {
            chunkController.gameObject.SetActive(false);
            Destroy(chunkController.gameObject);
        }

        #endregion

        IEnumerator LazyUpdateChunks()
        {
            const int radius = Consts.ViewDistanceInChunks * Consts.ChunkSize;
            // calculate visible inner rect 
            var centerChunkPos = lastStayChunkPos;
            int startX = centerChunkPos.x - radius;
            int startZ = centerChunkPos.z - radius;
            int endX = centerChunkPos.x + radius + 1;
            int endZ = centerChunkPos.z + radius + 1;

            // calculate outer rect for chunks removal
            const int extraSize = Consts.ChunkSize;
            Rect outerRect = Rect.MinMaxRect(startX - extraSize, startZ - extraSize, endX + extraSize, endZ + extraSize);

            // release the chunks to pool that are outside of the outer rect
            activeChunks = activeChunks
                .Where(kv =>
                {
                    var pos = kv.Key;
                    bool isOutOfRange = !outerRect.Contains(new Vector2(pos.x, pos.z));
                    if (isOutOfRange)
                        pool.Release(activeChunks[pos]);

                    return !isOutOfRange;
                })
                .ToDictionary(kv=>kv.Key, kv=>kv.Value);

            // get all the positions of the chunks to create
            var chunksToCreate = new List<Vector3Int>();
            for (int x = startX; x < endX; x += Consts.ChunkSize)
            {
                for (int z = startZ; z < endZ; z += Consts.ChunkSize)
                {
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (!activeChunks.ContainsKey(pos))
                        chunksToCreate.Add(pos);
                }
            }

            var center = centerChunkPos;
            chunksToCreate.Sort((a, b) => (Mathf.Abs(a.x - center.x) + Mathf.Abs(a.z - center.z)) 
                                         - (Mathf.Abs(b.x - center.x) + Mathf.Abs(b.z - center.z)));

            if (NeedInterruptAndRegenerate)
                yield break;

            yield return null;

            var loadingChunks = new List<ChunkController>();
            foreach (var pos in chunksToCreate)
            {
                var chunkController = pool.Get();
                activeChunks.Add(pos, chunkController);

                chunkController.GenerateChunkToPosition(pos);
                loadingChunks.Add(chunkController);

                while(loadingChunks.Count >= 8)
                {
                    yield return null;
                    loadingChunks = loadingChunks.Where(chunk => chunk.IsLoading).ToList();
                }

                // check if need interruption and regenerate
                if (NeedInterruptAndRegenerate)
                    break;
            }

            while (loadingChunks.Count > 0)
            {
                yield return null;
                loadingChunks = loadingChunks.Where(chunk => chunk.IsLoading).ToList();
            }
        }

        IEnumerator StartUpdateChunks()
        {
            do
            {
                IsChunksLoading = true;
                NeedInterruptAndRegenerate = false;
                yield return LazyUpdateChunks();

            } while (NeedInterruptAndRegenerate);

            IsChunksLoading = false;
        }

        public void Update()
        {
            var playerPos = targetPlayer.transform.position;
            var newStayChunkPos = playerPos.ToChunkPosInt();

            if (lastStayChunkPos != newStayChunkPos || !IsInit)
            {
                IsInit = true;
                lastStayChunkPos = newStayChunkPos;

                if (IsChunksLoading)
                    NeedInterruptAndRegenerate = true;
                else
                    StartCoroutine(StartUpdateChunks());
            }
        }
    }
}