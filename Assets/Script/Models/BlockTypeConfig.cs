using System;
using UnityEngine;

namespace Assets.Script.Models
{
    [Serializable]
    public class BlockTypeConfig
    {
        public EBlockType Type;
        public Vector2Int TileTop;
        public Vector2Int TileSides;
        public Vector2Int TileBottom;
        public bool IsSolid;
        public bool IsTransparent;
        public int Layer;
    }
}