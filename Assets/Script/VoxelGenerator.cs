using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelGenerator : MonoBehaviour
{
    private static readonly VoxelConfig VoxelConfig = new VoxelConfig(voxelSize: 1, texTiles: 10);
    private static readonly FastNoiseLite Noise = new FastNoiseLite(1337);

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

    IEnumerator LazyGenerateChunk()
    {
        float nextInterruptTime = Time.realtimeSinceStartup + LazyLoadingTimePerFrame;
        int vertexCount = 0;
        int triangleCount = 0;
        var meshDataList = new List<(Vector3, MeshData)>();

        for (int x = 0; x < ChunkSize; ++x)
        {
            for (int z = 0; z < ChunkSize; ++z)
            {
                for (int y = 0; y < ChunkHeight; ++y)
                {
                    if (!IsSolid(x, y, z))
                        continue;

                    int voxelType = 0;
                    if (IsSolid(x, y, z + 1)) voxelType |= FaceBitMask.Front;
                    if (IsSolid(x, y, z - 1)) voxelType |= FaceBitMask.Back;
                    if (IsSolid(x - 1, y, z)) voxelType |= FaceBitMask.Left;
                    if (IsSolid(x + 1, y, z)) voxelType |= FaceBitMask.Right;
                    if (IsSolid(x, y + 1, z)) voxelType |= FaceBitMask.Top;
                    if (IsSolid(x, y - 1, z)) voxelType |= FaceBitMask.Bottom;

                    if (voxelType == VoxelConfig.EMPTY_MESH_TYPE)
                        continue;

                    var meshData = VoxelConfig.GetMeshData(voxelType);
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

        var mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    void Update()
    {
        
    }
}
