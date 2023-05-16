using Assets.Script.Models;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Helpers
{
    public static class MeshDataHelper
    {
        public static Mesh ToMesh(this MeshData meshData)
        {
            var mesh = new Mesh()
            {
                vertices = meshData.Vertices,
                triangles = meshData.Triangles,
                uv = meshData.UV
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        public static MeshData CopyToPosition(this MeshData meshData, Vector3 pos)
        {
            var newMeshData = new MeshData()
            {
                Vertices = meshData.Vertices.Select(v => v + pos).ToArray(),
                Triangles = meshData.Triangles,
                UV = meshData.UV
            };
            return newMeshData;
        }
    }
}