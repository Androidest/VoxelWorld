using Assets.Script.Manager;
using Assets.Script.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    private const int frameRate = 30;
    private const float lazyLoadingPercentPerFrame = 0.5f;
    private const float lazyLoadingTimePerFrame = 1f / frameRate * lazyLoadingPercentPerFrame;

    private FastNoiseLite noise = new FastNoiseLite();
    private int[,] heightMap;
    private int startX;
    private int startZ;
    private float nextInterruptTime;
    private MeshFilter normalMeshFilter;
    private MeshFilter transparentMeshFilter;
    private MeshCollider meshCollider;
    private VoxelConfig voxelConfig;

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

    public void Disable()
    {
        normalMeshFilter.gameObject.SetActive(false);
        transparentMeshFilter.gameObject.SetActive(false);
        meshCollider.gameObject.SetActive(false);
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

    BlockTypeConfig GetBlockData(int x, int y, int z)
    {
        var blockType = GetBlockType(x, y, z);
        return voxelConfig.GetBlockTypeConfig(blockType);
    }

    EBlockType GetBlockType(int x, int y, int z)
    {
        if (heightMap[x + 1, z + 1] < y)
        {
            if (y < 10)
                return EBlockType.Water;
            return EBlockType.None;
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

    public IEnumerator LazyGenerateChunk()
    {
        Disable();
        InitHeightMap();
        yield return LazyUpdateChunk();
    }

    public IEnumerator LazyUpdateChunk()
    {
        nextInterruptTime = Time.realtimeSinceStartup + lazyLoadingTimePerFrame;
        var meshDataList = new List<(Vector3, MeshData)>();
        var meshDataList_trans = new List<(Vector3, MeshData)>();
        var meshDataList_col = new List<(Vector3, MeshData)>();

        for (int x = 0; x < Consts.ChunkSize; ++x)
        {
            for (int z = 0; z < Consts.ChunkSize; ++z)
            {
                for (int y = 0; y < Consts.ChunkHeight; ++y)
                {
                    BlockTypeConfig blockData = GetBlockData(x, y, z);
                    if (blockData.Type == EBlockType.None)
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

                    var meshData = voxelConfig.GetMeshData(blockData.Type, meshType);

                    var pos = new Vector3(x, y, z);
                    if (blockData.IsTransparent)
                        meshDataList_trans.Add((pos, meshData));
                    else
                        meshDataList.Add((pos, meshData));

                    if(blockData.IsSolid)
                        meshDataList_col.Add((pos, meshData));

                    //yield return S();
                }
            }
        }

        // mesh
        if (meshDataList.Count > 0)
        {
            Mesh mesh = new Mesh();
            yield return MergeMeshData(meshDataList, mesh);
            normalMeshFilter.mesh = mesh;
            normalMeshFilter.gameObject.SetActive(true);
        }

        // transparent mesh
        if (meshDataList_trans.Count > 0)
        {
            Mesh mesh = new Mesh();
            yield return MergeMeshData(meshDataList_trans, mesh);
            transparentMeshFilter.mesh = mesh;
            transparentMeshFilter.gameObject.SetActive(true);
        }

        // collider mesh
        if (meshDataList_col.Count > 0)
        {
            Mesh mesh = new Mesh();
            yield return MergeMeshData(meshDataList_col, mesh, false);
            meshCollider.sharedMesh = mesh;
            meshCollider.gameObject.SetActive(true);
        }
    }

    IEnumerator MergeMeshData(List<(Vector3, MeshData)> meshDataList, Mesh mesh, bool hasUV = true)
    {
        int vertexCount = meshDataList.Sum(m => m.Item2.Vertices.Length);
        var vertices = new Vector3[vertexCount];
        var triangles = new int[vertexCount / Consts.VoxelFaceVertexCount * Consts.VoxelFaceTriangleCount];
        var uv = new Vector2[vertexCount];

        int vIndex = 0;
        int tIndex = 0;

        foreach (var (pos, meshData) in meshDataList)
        {
            var lastVoxelVertexIndex = vIndex;

            for (int i = 0; i < meshData.Vertices.Length; ++i)
            {
                vertices[vIndex] = meshData.Vertices[i] + pos;
                ++vIndex;
            }

            if (hasUV)
                Array.Copy(meshData.UV, 0, uv, lastVoxelVertexIndex, meshData.UV.Length);

            for (int i = 0; i < meshData.Triangles.Length; ++i)
            {
                triangles[tIndex] = meshData.Triangles[i] + lastVoxelVertexIndex;
                ++tIndex;
            }

        }
        yield return null;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        if (hasUV)
            mesh.uv = uv;
        mesh.RecalculateNormals();
    }

    IEnumerator S()
    {
        if (Time.realtimeSinceStartup > nextInterruptTime)
        {
            nextInterruptTime = Time.realtimeSinceStartup + lazyLoadingTimePerFrame;
            yield return null;
        }
    }

    void Update()
    {
        
    }
}
