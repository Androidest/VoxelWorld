﻿using UnityEngine;

static class Consts
{
    public const float GravityMultiplier = 4f;
    public const float Gravity = -9.81f * GravityMultiplier;
    public const float PlayerMoveSpeed = 3f;
    public const float PlayerJumpHeight = 1.2f;
    public const float CameraAngleLimitX = 80f;
    public const float MaxCamDistance = 6f;
    public const float ToRadianMultiplier = Mathf.PI / 180;
    public const int ChunkSize = 32;
    public const int ChunkHeight = 24;
}
