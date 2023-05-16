using Assets.Script.Helpers;
using Assets.Script.Manager;
using Assets.Script.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    private FastNoiseLite noise = new FastNoiseLite();
    private int[,] heightMap;
    private int startX;
    private int startZ;
    private MeshFilter normalMeshFilter;
    private MeshFilter transparentMeshFilter;
    private MeshCollider meshCollider;
    private VoxelConfig voxelConfig;
    public bool IsLoading;

    private MeshData normalMesh;
    private MeshData transparentMesh;
    private MeshData colliderMesh;

    void Awake()
    {
        normalMeshFilter = transform.Find("NormalMesh").GetComponent<MeshFilter>();
        transparentMeshFilter = transform.Find("TransparentMesh").GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        voxelConfig = ConfigManager.Instance.VoxelConfig;

        noise.SetSeed(ConfigManager.Instance.Seed);
        noise.SetFractalOctaves(5);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFrequency(0.02f);
    }

    void InitHeightMap()
    {
        startX = (int)transform.position.x;
        startZ = (int)transform.position.z;

        int size = Consts.ChunkSize + 2;
        heightMap = new int[size, size];
        
        for (int x = 0; x < size; ++x)
        {
            for (int z = 0; z < size; ++z)
            {
                heightMap[x, z] = Mathf.FloorToInt((noise.GetNoise(x + startX, z + startZ) + 1f) * 0.5f * Consts.ChunkHeight);
            }
        }
    }

    EBlockType GetBlockType(int x, int y, int z)
    {
        if (heightMap[x + 1, z + 1] < y)
        {
            if (y < 10)
                return EBlockType.Water;
            return EBlockType.Air;
        }

        if (y > 30)
            return EBlockType.Snow;
        else if (y > 15)
            return EBlockType.Grass;
        else
        {
            return EBlockType.Sand;
        }
    }

    // ================= publics =============================
    public void Disable()
    {
        normalMeshFilter.gameObject.SetActive(false);
        transparentMeshFilter.gameObject.SetActive(false);
        meshCollider.enabled = false;
    }

    public void GenerateChunkToPosition(Vector3 pos)
    {
        if (IsLoading)
        {
            Debug.LogError($"[ChunkController] chunk {gameObject.name} is still loading");
            return;
        }

        transform.position = pos;
        gameObject.name = $"{transform.position}";
        Disable();
        InitHeightMap();
        StartCoroutine(GenerateChunkCoroutine());
    }

    public void UpdateChunk()
    {
        if (IsLoading)
        {
            Debug.LogError($"[ChunkController] chunk {gameObject.name} is still loading");
            return;
        }

        StartCoroutine(GenerateChunkCoroutine());
    }

    // ================== privates ==============================
    private BlockTypeConfig GetBlockData(int x, int y, int z)
    {
        var blockType = GetBlockType(x, y, z);
        return voxelConfig.GetBlockTypeConfig(blockType);
    }

    private IEnumerator GenerateChunkCoroutine()
    {
        IsLoading = true;

        var updateChunkTask = Task.Run(GenerateChunkMeshData);
        while (!updateChunkTask.IsCompletedSuccessfully)
            yield return null;

        if (normalMesh != null)
        {
            normalMeshFilter.mesh = normalMesh.ToMesh();
            normalMeshFilter.gameObject.SetActive(true);
        }

        if (transparentMesh != null)
        {
            transparentMeshFilter.mesh = transparentMesh.ToMesh();
            transparentMeshFilter.gameObject.SetActive(true);
        }

        if (colliderMesh != null)
        {
            meshCollider.sharedMesh = colliderMesh.ToMesh();
            meshCollider.enabled = true;
        }

        IsLoading = false;
    }

    private void GenerateChunkMeshData()
    {
        var meshDataList = new List<MeshData>();
        var meshDataList_trans = new List<MeshData>();
        var meshDataList_col = new List<MeshData>();

        for (int x = 0; x < Consts.ChunkSize; ++x)
        {
            for (int z = 0; z < Consts.ChunkSize; ++z)
            {
                for (int y = 0; y < Consts.ChunkHeight; ++y)
                {
                    BlockTypeConfig blockData = GetBlockData(x, y, z);
                    if (blockData.Type == EBlockType.Air)
                        continue;

                    var layer = blockData.Layer;

                    int meshType = 0;
                    if (GetBlockData(x, y, z + 1).Layer == layer) meshType |= FaceBitMask.Front;
                    if (GetBlockData(x, y, z - 1).Layer == layer) meshType |= FaceBitMask.Back;
                    if (GetBlockData(x - 1, y, z).Layer == layer) meshType |= FaceBitMask.Left;
                    if (GetBlockData(x + 1, y, z).Layer == layer) meshType |= FaceBitMask.Right;
                    if (GetBlockData(x, y + 1, z).Layer == layer) meshType |= FaceBitMask.Top;
                    if (GetBlockData(x, y - 1, z).Layer == layer) meshType |= FaceBitMask.Bottom;
                    if (meshType == VoxelConfig.EMPTY_MESH_TYPE)
                        continue;

                    var configMeshData = voxelConfig.GetMeshData(blockData.Type, meshType);
                    var meshData = configMeshData.CopyToPosition(new Vector3(x, y, z));

                    if (blockData.IsTransparent)
                        meshDataList_trans.Add(meshData);
                    else
                        meshDataList.Add(meshData);

                    if(blockData.IsSolid)
                        meshDataList_col.Add(meshData);
                }
            }
        }

        // mesh
        normalMesh = (meshDataList.Count > 0) ? MergeMeshData(meshDataList) : null;
        // collider mesh
        transparentMesh = (meshDataList_trans.Count > 0) ? MergeMeshData(meshDataList_trans) : null;
        // transparent mesh
        colliderMesh = (meshDataList_col.Count > 0) ? MergeMeshData(meshDataList_col, false) : null;
    }

    private MeshData MergeMeshData(List<MeshData> meshDataList, bool hasUV = true)
    {
        int vertexCount = meshDataList.Sum(m => m.Vertices.Length);
        var vertices = new Vector3[vertexCount];
        var triangles = new int[vertexCount / Consts.VoxelFaceVertexCount * Consts.VoxelFaceTriangleCount];
        var uv = hasUV? new Vector2[vertexCount] : null;

        int lastVertexIndex = 0;
        int tIndex = 0;

        foreach (var meshData in meshDataList)
        {
            Array.Copy(meshData.Vertices, 0, vertices, lastVertexIndex, meshData.Vertices.Length);

            if (hasUV)
                Array.Copy(meshData.UV, 0, uv, lastVertexIndex, meshData.UV.Length);

            for (int i = 0; i < meshData.Triangles.Length; ++i)
            {
                triangles[tIndex] = meshData.Triangles[i] + lastVertexIndex;
                ++tIndex;
            }

            lastVertexIndex += meshData.Vertices.Length;
        }

        return new MeshData()
        {
            Vertices = vertices,
            Triangles = triangles,
            UV = uv
        };
    }

}
