using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.Common
{
    /// <summary>
    /// Custom Object Pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StackPool<T>
    {
        private int curPoolSize;
        private T[] pool;
        private Func<T> CreateIntance;
        private Action<T> OnGetIntance;
        private Action<T> DestroyInstance;
        private Action<T> ReleaseInstance;
        private readonly bool logWarning;

        public StackPool(int maxPoolSize, Func<T> onCreate, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy, bool logWarning = false)
        {
            if (onCreate == null)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onCreate cannot be null");
            }
            if (onGet == null)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onGet cannot be null");
            }
            if (onRelease == null)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onRelease cannot be null");
            }
            if (onDestroy == null)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onDestroy cannot be null");
            }
            this.CreateIntance = onCreate;
            this.OnGetIntance = onGet;
            this.ReleaseInstance = onRelease;
            this.DestroyInstance = onDestroy;
            pool = new T[maxPoolSize];
            curPoolSize = 0;
            this.logWarning = logWarning;
        }

        private void Push(T obj)
        {
            pool[curPoolSize++] = obj;
        }

        private T Pop()
        {
            return pool[--curPoolSize];
        }

        private T TryCreate()
        {
            T obj = default(T);
            try
            {
                obj = CreateIntance(); Debug.Log("TryCreate");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] fail to create instance");
                Debug.LogError(e);
            }

            if (EqualityComparer<T>.Default.Equals(obj, default(T)))
            {
                throw new Exception("[ObjectPool<{typeof(T).Name}>] the created instance is null");
            }
            return obj;
        }

        private void TryDestroy(T obj)
        {
            try
            {
                DestroyInstance(obj); Debug.Log("TryDestroy");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] fail to destroy instance");
                Debug.LogError(e);
            }
        }

        public T Get()
        {
            if (curPoolSize == 0)
                return TryCreate();

            T obj = Pop();
            OnGetIntance(obj);
            return obj;
        }

        public void Release(T obj)
        {
            if (curPoolSize >= pool.Length)
            {
                if (logWarning)
                    Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] not enough space, try increase the pool size");
                TryDestroy(obj);
                return;
            }
            ReleaseInstance(obj);
            Push(obj);
        }

        public void Clear()
        {
            for (int i = 0; i < curPoolSize; ++i)
            {
                TryDestroy(pool[i]);
            }
        }
    }
}
