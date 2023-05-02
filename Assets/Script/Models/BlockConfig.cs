using System.Collections.Generic;
using System;
using UnityEngine;

namespace Assets.Script.Models
{
    [CreateAssetMenu(fileName = "BlockConfig", menuName = "Config/BlockConfig")]
    public class BlockConfig : ScriptableObject
    {
        public float TileSizeX;
        public float TileSizeY;
        public List<BlockTypeConfig> Blocks;
        [NonSerialized] public Dictionary<EBlockType, BlockTypeConfig> Blocks_Dict;
    }
}