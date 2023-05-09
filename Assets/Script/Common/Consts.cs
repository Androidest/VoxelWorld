using UnityEngine;

static class Consts
{
    public const int VoxelFaceCount = 6;
    public const int VoxelFaceTriangleCount = 6;
    public const int VoxelFaceVertexCount = 4;

    public const float GravityMultiplier = 4f;
    public const float Gravity = -9.81f * GravityMultiplier;
    public const float PlayerMoveSpeed = 12f;
    public const float PlayerJumpHeight = 1.6f;
    public const float CameraAngleLimitX = 80f;
    public const float MaxCamDistance = 6f;
    public const float ToRadianMultiplier = Mathf.PI / 180;
    public const int ViewDistanceInChunks = 8;
    public const int ChunkSize = 16;
    public const int ChunkHeight = 46;
    public const int SubMeshCounts = 2;
}
