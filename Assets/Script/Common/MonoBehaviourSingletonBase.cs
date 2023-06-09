﻿using UnityEngine;


public class MonoBehaviourSingletonBase<T> : MonoBehaviour where T : MonoBehaviourSingletonBase<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError($"[MonoBehaviorSingletonBase] {typeof(T).Name} not initialized");
            }
            return instance;
        }
    }

    protected virtual void OnAwake()
    {
        Debug.LogError($"[MonoBehaviorSingletonBase] {typeof(T).Name} OnAwake not overrided");
    }

    private void Awake()
    {
        instance = gameObject.GetComponent<T>();
        instance.OnAwake();
    }
}
