using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.Models
{
    public class VoxelConfig
    {
        private Dictionary<EBlockType, MeshData[]> meshTypeMap;
        public const int MAX_FACE_COUNT = 6;
        public static readonly int TYPE_COUNT = (int)Mathf.Pow(2, MAX_FACE_COUNT);
        public static readonly int EMPTY_MESH_TYPE = TYPE_COUNT - 1;

        public VoxelConfig(BlockConfig config)
        {
            int faceCount = 6;
            var tileSize = new Vector2(config.TileSizeX, config.TileSizeY);
            var faceConfig = new VoxelFaceConfig();
            int typeCount = TYPE_COUNT;
            meshTypeMap = new Dictionary<EBlockType, MeshData[]>();

            foreach (var block in config.Blocks)
                meshTypeMap.Add(block.Type, new MeshData[typeCount]);

            // meshType is a bit mask, uses the first 6 bits to represent the 6 faces of a voxel
            // if a bit is set, it means this face is hidden
            for (int meshType = 0; meshType < typeCount; ++meshType)
            {
                var vertexList = new List<Vector3>();
                var triangleList = new List<int>();
                var uvDictList = new Dictionary<EBlockType, List<Vector2>>();
                var indexOffset = 0;
                for (int faceType = 0; faceType < faceCount; ++faceType)
                {
                    // if this face shuold be hidden then skip this face
                    if (IsBitSet(meshType, faceType))
                        continue;
                    var faceMesh = faceConfig.GetFaceMeshData(faceType);

                    // compute vertices
                    foreach (var vert in faceMesh.Vertices)
                        vertexList.Add(vert);

                    // compute triangles
                    foreach (var vIndex in faceMesh.Triangles)
                        triangleList.Add(vIndex + indexOffset);

                    indexOffset += faceMesh.Vertices.Length;

                    // compute uvs for different blockType and faces
                    foreach (var uv in faceMesh.UV)
                    {
                        foreach (var block in config.Blocks)
                        {
                            if (!uvDictList.ContainsKey(block.Type))
                                uvDictList.Add(block.Type, new List<Vector2>());

                            if (faceType == FaceType.Top)
                                uvDictList[block.Type].Add((uv + config.Blocks_Dict[block.Type].TileTop) * tileSize);
                            else if (faceType == FaceType.Bottom)
                                uvDictList[block.Type].Add((uv + config.Blocks_Dict[block.Type].TileBottom) * tileSize);
                            else
                                uvDictList[block.Type].Add((uv + config.Blocks_Dict[block.Type].TileSides) * tileSize);
                        }
                    }
                }

                if (vertexList.Count == 0)
                    continue;

                var vertices = vertexList.ToArray();
                var triangles = triangleList.ToArray();

                foreach (var block in config.Blocks)
                {
                    // generate mesh data for every type of voxel mesh
                    // meshType equals 0 means this type of voxel mesh has no hidden faces (has complete 6 faces) 
                    meshTypeMap[block.Type][meshType] = new MeshData
                    {
                        Vertices = vertices,
                        Triangles = triangles,
                        UV = uvDictList[block.Type].ToArray(),
                    };
                }
            }
        }

        static bool IsBitSet(int bitMask, int bitIndex)
        {
            return (bitMask & (1 << bitIndex)) > 0;
        }

        public MeshData GetMeshData(EBlockType blockType, int meshType)
        {
            return meshTypeMap[blockType][meshType];
        }
    }
}
