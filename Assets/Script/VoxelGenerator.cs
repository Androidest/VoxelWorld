using Assets.Script.Manager;
using Assets.Script.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelGenerator : MonoBehaviour
{
    private FastNoiseLite Noise = new FastNoiseLite();
    const int ChunkSize = 32;
    const int ChunkHeight = 24;
    const int FrameRate = 30;
    const float LazyLoadingPercentPerFrame = 0.2f;
    const float LazyLoadingTimePerFrame = 1f / FrameRate * LazyLoadingPercentPerFrame;
    private int[,] HeightMap;
    private int StartX;
    private int StartZ;

    void Start()
    {
        Noise.SetSeed(ConfigManager.Instance.Seed);
        StartX = (int)transform.position.x;
        StartZ = (int)transform.position.z;

        GenerateHeightMap();
        StartCoroutine(LazyGenerateChunk());
    }

    void GenerateHeightMap()
    {
        int size = ChunkSize + 2;
        HeightMap = new int[size, size];
        
        for (int x = 0; x < size; ++x)
        {
            for (int z = 0; z < size; ++z)
            {
                HeightMap[x, z] = Mathf.FloorToInt((Noise.GetNoise(x + StartX, z + StartZ) + 1f) * 0.5f * ChunkHeight);
            }
        }
    }

    bool IsSolid(int x, int y, int z)
    {
        return HeightMap[x + 1, z + 1] >= y;
    }

    EBlockType GetBlockType(int x, int y, int z)
    {
        if (!IsSolid(x, y, z))
            return EBlockType.None;

        if (y > 20)
            return EBlockType.Snow;
        else if (y > 8)
            return EBlockType.Grass;
        else
            return EBlockType.Sand;
    }

    IEnumerator LazyGenerateChunk()
    {
        float nextInterruptTime = Time.realtimeSinceStartup + LazyLoadingTimePerFrame;
        int vertexCount = 0;
        int triangleCount = 0;
        var meshDataList = new List<(Vector3, MeshData)>();
        var voxelConfig = ConfigManager.Instance.VoxelConfig;

        for (int x = 0; x < ChunkSize; ++x)
        {
            for (int z = 0; z < ChunkSize; ++z)
            {
                for (int y = 0; y < ChunkHeight; ++y)
                {
                    var blockType = GetBlockType(x, y, z);
                    if (blockType == EBlockType.None)
                        continue;

                    int meshType = 0;
                    if (IsSolid(x, y, z + 1)) meshType |= FaceBitMask.Front;
                    if (IsSolid(x, y, z - 1)) meshType |= FaceBitMask.Back;
                    if (IsSolid(x - 1, y, z)) meshType |= FaceBitMask.Left;
                    if (IsSolid(x + 1, y, z)) meshType |= FaceBitMask.Right;
                    if (IsSolid(x, y + 1, z)) meshType |= FaceBitMask.Top;
                    if (IsSolid(x, y - 1, z)) meshType |= FaceBitMask.Bottom;

                    if (meshType == VoxelConfig.EMPTY_MESH_TYPE)
                        continue;

                    var meshData = voxelConfig.GetMeshData(blockType, meshType);
                    meshDataList.Add((new Vector3(x, y, z), meshData));
                    vertexCount += meshData.vertices.Length;
                    triangleCount += meshData.triangles.Length;

                    if (Time.realtimeSinceStartup > nextInterruptTime)
                    {
                        nextInterruptTime = Time.realtimeSinceStartup + LazyLoadingTimePerFrame;
                        yield return null;
                    }
                }
            }
        }

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uv = new Vector2[vertexCount];

        int vOffset = 0;
        int tOffset = 0;
        foreach (var (pos, meshData) in meshDataList)
        {
            var mVertices = meshData.vertices;
            var mTriangles = meshData.triangles;
            var muv = meshData.uv;

            for (int i = 0; i < mVertices.Length; ++i)
            {
                int index = i + vOffset;
                vertices[index] = mVertices[i] + pos;
                uv[index] = muv[i];
            }

            for (int i = 0; i < mTriangles.Length; ++i)
            {
                triangles[i + tOffset] = mTriangles[i] + vOffset;
            }

            vOffset += mVertices.Length;
            tOffset += mTriangles.Length;

            if (Time.realtimeSinceStartup > nextInterruptTime)
            {
                nextInterruptTime = Time.realtimeSinceStartup + LazyLoadingTimePerFrame;
                yield return null;
            }
        }

        // mesh
        var mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        // collier mesh
        var collider = GetComponent<MeshCollider>();
        Mesh colliderMesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
        colliderMesh.RecalculateNormals();
        collider.sharedMesh = colliderMesh;
    }

    void Update()
    {
        
    }
}
