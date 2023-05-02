using Assets.Script.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Manager
{
    public class ConfigManager : MonoBehaviour
    {
        private static ConfigManager _Instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    Debug.LogError($"ConfigManager not initialized");
                }
                return _Instance;
            }
        }

        [SerializeField] BlockConfig BlockConfig;
        public VoxelConfig VoxelConfig;
        public int Seed = 1336;

        private void Awake()
        {
            BlockConfig.Blocks_Dict = BlockConfig.Blocks.ToDictionary(item => item.Type, item => item);
            VoxelConfig = new VoxelConfig(BlockConfig);
            _Instance = this;
        }
    }
}
