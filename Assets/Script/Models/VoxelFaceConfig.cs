using UnityEngine;

namespace Assets.Script.Models
{
    public class VoxelFaceConfig
    {
        private readonly int[] triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        private readonly Vector2[] uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        private Vector3[][] faceVerticesMap;

        public VoxelFaceConfig()
        {
            const float voxelSize = 1;
            float front, right, top, back, left, bottom;
            front = right = top = voxelSize / 2f;
            back = left = bottom = -voxelSize / 2f;

            // 8 Vertices of a cube
            var cubeVertices = new Vector3[]
            {
                new Vector3(right, top, front),
                new Vector3(left, top, front),
                new Vector3(right, bottom, front),
                new Vector3(left, bottom, front),

                new Vector3(right, top, back),
                new Vector3(left, top, back),
                new Vector3(right, bottom, back),
                new Vector3(left, bottom, back),
            };

            // 6 face, 4 vertices each
            var faceVerticesIndex = new int[6][];
            faceVerticesIndex[FaceType.Front] = new int[] { 0, 1, 3, 2 }; // front
            faceVerticesIndex[FaceType.Back] = new int[] { 5, 4, 6, 7 }; // back
            faceVerticesIndex[FaceType.Left] = new int[] { 1, 5, 7, 3 }; // left
            faceVerticesIndex[FaceType.Right] = new int[] { 4, 0, 2, 6 }; // right
            faceVerticesIndex[FaceType.Top] = new int[] { 1, 0, 4, 5 }; // top
            faceVerticesIndex[FaceType.Bottom] = new int[] { 7, 6, 2, 3 }; // bottom

            faceVerticesMap = new Vector3[6][];
            for (int i = 0; i < 6; ++i)
            {
                faceVerticesMap[i] = new Vector3[4];
                for (int j = 0; j < 4; ++j)
                {
                    var index = faceVerticesIndex[i][j];
                    faceVerticesMap[i][j] = cubeVertices[index];
                }
            }
        }

        public MeshData GetFaceMeshData(int faceType)
        {
            if (faceVerticesMap == null)
            {
                Debug.LogError($"VoxelFaceHelper not initialized");
                return null;
            }

            return new MeshData
            {
                Vertices = faceVerticesMap[faceType],
                Triangles = triangles,
                UV = uv
            };
        }   

        
    }
}
