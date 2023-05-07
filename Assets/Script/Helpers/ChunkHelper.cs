using System;
using UnityEngine;

namespace Assets.Script.Helpers
{
    public static class ChunkHelper
    {
        private const int LOW_BIT_MASK = Consts.ChunkSize - 1;
        private const int HIGH_BIT_MASK = ~LOW_BIT_MASK;

        public static int GetLowBits(int number)
        {
            return number & LOW_BIT_MASK;
        }

        public static int GetHighBits(int number)
        {
            return number & HIGH_BIT_MASK;
        }

        public static  Vector3Int ToChunkPosInt(this Vector3 pos)
        {
            return new Vector3Int(GetHighBits((int)pos.x), 0, GetHighBits((int)pos.z));
        }

        public static Vector3 ToChunkPos(this Vector3 pos)
        {
            return new Vector3(GetHighBits((int)pos.x), 0, GetHighBits((int)pos.z));
        }

        public static  Vector3Int ToChunkLocalPos(this Vector3 pos)
        {
            return new Vector3Int(GetLowBits((int)pos.x), (int)MathF.Floor(pos.y), GetLowBits((int)pos.z));
        }
    }
}
