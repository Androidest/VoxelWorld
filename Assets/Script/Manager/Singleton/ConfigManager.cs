using Assets.Script.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Manager
{
    public class ConfigManager : MonoBehaviorSingletonBase<ConfigManager>
    {
        [SerializeField] BlockConfig BlockConfig;
        public VoxelConfig VoxelConfig;
        public int Seed = 1334;
        public float VertCamSpeed = 5f;
        public float HorzCamSpeed = 5f;

        protected override void OnAwake()
        {
            BlockConfig.Blocks_Dict = BlockConfig.Blocks.ToDictionary(item => item.Type, item => item);
            VoxelConfig = new VoxelConfig(BlockConfig);
        }
    }
}
