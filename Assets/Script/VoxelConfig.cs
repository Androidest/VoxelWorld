using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    public class VoxelConfig
    {
        private MeshData[] MeshTypeMap;
        public const int MAX_FACE_COUNT = 6;
        public static readonly int EMPTY_MESH_TYPE = (int)Mathf.Pow(2, MAX_FACE_COUNT);
        public static readonly int TYPE_COUNT = EMPTY_MESH_TYPE;

        public VoxelConfig(int voxelSize, float texTiles)
        {
            int faceCount = 6;
            float tileSize = 1 / texTiles;
            var faceConfig = new VoxelFaceConfig(voxelSize);
            int typeCount = TYPE_COUNT;
            MeshTypeMap = new MeshData[typeCount];

            // voxelType is a bit mask, uses the first 6 bits to represent the 6 faces of a voxel
            // if a bit is set, it means this face is hidden
            for (int voxelType = 0; voxelType < typeCount; ++voxelType)
            {
                var vertexList = new List<Vector3>();
                var triangleList = new List<int>();
                var uvList = new List<Vector2>();
                var indexOffset = 0;
                for (int faceType = 0; faceType < faceCount; ++faceType)
                {
                    // if this face shuold be hidden then skip this face
                    if (IsBitSet(voxelType, faceType))
                        continue;
                    var faceMesh = faceConfig.GetFaceMeshData(faceType);

                    foreach (var vert in faceMesh.vertices)
                        vertexList.Add(vert);

                    foreach (var vIndex in faceMesh.triangles)
                        triangleList.Add(vIndex + indexOffset);

                    foreach (var uv in faceMesh.uv)
                        uvList.Add(uv * tileSize);

                    indexOffset += faceMesh.vertices.Length; 
                }

                // generate mesh data for every type of voxel mesh
                // voxelType equals 0 means this type of voxel mesh has no hidden faces (has complete 6 faces) 
                MeshTypeMap[voxelType] = new MeshData
                { 
                    vertices = vertexList.ToArray(),
                    triangles = triangleList.ToArray(),
                    uv = uvList.ToArray(),
                };
            }
        }

        static bool IsBitSet(int bitMask, int bitIndex)
        {
            return (bitMask & (1 << bitIndex)) > 0;
        }

        public MeshData GetMeshData(int voxelType)
        {
            return MeshTypeMap[voxelType];
        }
    }
}
