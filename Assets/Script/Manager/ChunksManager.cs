using Assets.Script.Common;
using Assets.Script.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace Assets.Script.Manager
{
    public class ChunksManager : MonoBehaviorSingletonBase<ChunksManager>
    {
        private PlayerController targetPlayer;
        
        private StackPool<ChunkController> pool;
        private Dictionary<Vector3Int, ChunkController> activeChunks;
        private Transform world;
        private Vector3Int lastStayChunkPos;
        private Coroutine UpdateTerrainCoroutine;
        private bool IsInit;

        protected override void OnAwake()
        {

        }

        public void Start()
        {
            world = transform.parent;
            targetPlayer = GameManager.Instance.CurPlayerController;
            activeChunks = new Dictionary<Vector3Int, ChunkController>();
            IsInit = false;

            pool = new StackPool<ChunkController>(20,
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
            chunkController.transform.SetParent(world, false);
        }

        private void OnReleaseChunkToPool(ChunkController chunkController)
        {
            chunkController.gameObject.SetActive(false);
            chunkController.transform.parent = null;
        }

        private void OnDestroyChunk(ChunkController chunkController)
        {
            chunkController.gameObject.SetActive(false);
            Destroy(chunkController);
        }

        #endregion

        IEnumerator UpdateTerrain(Vector3Int newCenterChunkPos)
        {
            const int radius = Consts.ViewDistanceInChunks * Consts.ChunkSize;
            // calculate visible inner rect 
            int startX = newCenterChunkPos.x - radius;
            int startZ = newCenterChunkPos.z - radius;
            int endX = newCenterChunkPos.x + radius + 1;
            int endZ = newCenterChunkPos.z + radius + 1;

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

            var center = newCenterChunkPos;
            chunksToCreate.Sort((a, b) => (Mathf.Abs(a.x - center.x) + Mathf.Abs(a.z - center.x)) 
                                         - (Mathf.Abs(b.x - center.x) + Mathf.Abs(b.z - center.x)));
            foreach (var pos in chunksToCreate)
            {
                var chunkController = pool.Get();
                activeChunks.Add(pos, chunkController);

                chunkController.gameObject.name = $"{pos}";
                chunkController.transform.position = pos;
                yield return chunkController.LazyGenerateChunk();
            }
        }

        public void Update()
        {
            var playerPos = targetPlayer.transform.position;
            var newStayChunkPos = playerPos.ToChunkPosInt();

            if (lastStayChunkPos != newStayChunkPos || !IsInit)
            {
                IsInit = true;
                lastStayChunkPos = newStayChunkPos;

                if (UpdateTerrainCoroutine != null)
                    StopCoroutine(UpdateTerrainCoroutine);

                UpdateTerrainCoroutine = StartCoroutine(UpdateTerrain(newStayChunkPos));
            }
        }
    }
}