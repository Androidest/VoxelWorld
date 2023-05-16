using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Manager
{
    public class GameManager : MonoBehaviourSingletonBase<GameManager>
    {
        [SerializeField] public CameraController CameraController;
        [SerializeField] public PlayerController CurPlayerController;

        private Dictionary<Type, MonoBehaviour> managersDict;

        protected override void OnAwake()
        {
            CameraController.SetTarget(CurPlayerController.gameObject.transform);
        }

        private void Start()
        {
            var scripts = gameObject.GetComponents<MonoBehaviour>();
            var list = new List<MonoBehaviour>(scripts);
            managersDict = list.ToDictionary(m => m.GetType(), m => m);
        }

        private void Update()
        {
            
        }

        public T GetManager<T>() where T : MonoBehaviour
        {
            if (managersDict.TryGetValue(typeof(T), out var manager))
                return (T)manager;

            Debug.LogError($"[GameManager.GetManager] manager '{typeof(T).Name}' not found!");
            return default; 
        }
    }
}
