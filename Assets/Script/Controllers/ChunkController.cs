using Assets.Script.Manager;
using Assets.Script.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkController : MonoBehaviour
{
    private const int frameRate = 30;
    private const float lazyLoadingPercentPerFrame = 0.5f;
    private const float lazyLoadingTimePerFrame = 1f / frameRate * lazyLoadingPercentPerFrame;

    private FastNoiseLite noise = new FastNoiseLite();
    private int[,] heightMap;
    private int startX;
    private int startZ;

    void Awake()
    {
        noise.SetSeed(ConfigManager.Instance.Seed);
        //StartCoroutine(LazyGenerateChunk());
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

    bool IsSolid(int x, int y, int z)
    {
        return heightMap[x + 1, z + 1] >= y;
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

    public IEnumerator LazyGenerateChunk()
    {
        InitHeightMap();
        yield return LazyUpdateChunk();
    }

    public IEnumerator LazyUpdateChunk()
    {
        float nextInterruptTime = Time.realtimeSinceStartup + lazyLoadingTimePerFrame;
        int vertexCount = 0;
        int triangleCount = 0;
        var meshDataList = new List<(Vector3, MeshData)>();
        var voxelConfig = ConfigManager.Instance.VoxelConfig;

        for (int x = 0; x < Consts.ChunkSize; ++x)
        {
            for (int z = 0; z < Consts.ChunkSize; ++z)
            {
                for (int y = 0; y < Consts.ChunkHeight; ++y)
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
                    vertexCount += meshData.Vertices.Length;
                    triangleCount += meshData.Triangles.Length;

                    //if (Time.realtimeSinceStartup > nextInterruptTime)
                    //{
                    //    nextInterruptTime = Time.realtimeSinceStartup + lazyLoadingTimePerFrame;
                    //    yield return null;
                    //}
                }
            }
        }

        yield return null;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uv = new Vector2[vertexCount];

        int vOffset = 0;
        int tOffset = 0;
        foreach (var (pos, meshData) in meshDataList)
        {
            var mVertices = meshData.Vertices;
            var mTriangles = meshData.Triangles;
            var muv = meshData.UV;

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

            //if (Time.realtimeSinceStartup > nextInterruptTime)
            //{
            //    nextInterruptTime = Time.realtimeSinceStartup + lazyLoadingTimePerFrame;
            //    yield return null;
            //}
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
