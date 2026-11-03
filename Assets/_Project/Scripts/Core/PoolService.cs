using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorFactory.Core
{
    public sealed class PoolService : MonoBehaviour
    {
        private sealed class Pool
        {
            public string Key;
            public GameObject Template;
            public readonly Stack<GameObject> Inactive = new();
            public readonly HashSet<GameObject> Active = new();
            public readonly HashSet<GameObject> All = new();
        }

        private readonly Dictionary<string, Pool> pools = new();
        private readonly Dictionary<GameObject, Pool> instanceOwners = new();

        private bool shuttingDown;

        public bool IsShuttingDown => shuttingDown;

        /// <summary>
        /// Số instance đang được dùng (active) trên mọi pool; dùng cho diagnostic và memory report.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                int count = 0;

                foreach (Pool pool in pools.Values)
                    count += pool.Active.Count;

                return count;
            }
        }

        /// <summary>
        /// Số instance đang nằm trong kho chờ (inactive) trên mọi pool; dùng cho diagnostic và memory report.
        /// </summary>
        public int InactiveCount
        {
            get
            {
                int count = 0;

                foreach (Pool pool in pools.Values)
                    count += pool.Inactive.Count;

                return count;
            }
        }

        public void CreatePool(string key, GameObject template, int initialSize)
        {
            if (shuttingDown)
                throw new InvalidOperationException("Pool service is shutting down.");

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Pool key is required.", nameof(key));

            if (template == null)
                throw new ArgumentNullException(nameof(template));

            if (pools.ContainsKey(key))
                throw new InvalidOperationException($"Pool '{key}' already exists.");

            var pool = new Pool
            {
                Key = key,
                Template = template
            };

            pools.Add(key, pool);

            for (int index = 0; index < Mathf.Max(0, initialSize); index++)
                pool.Inactive.Push(CreateInstance(pool));
        }

        public GameObject Get(string key)
        {
            if (shuttingDown)
                throw new InvalidOperationException("Pool service is shutting down.");

            if (!pools.TryGetValue(key, out Pool pool))
                throw new KeyNotFoundException($"Pool '{key}' does not exist.");

            while (pool.Inactive.Count > 0)
            {
                GameObject candidate = pool.Inactive.Pop();

                if (candidate != null)
                {
                    pool.Active.Add(candidate);
                    candidate.transform.SetParent(null, false);
                    candidate.SetActive(true);
                    return candidate;
                }
            }

            GameObject instance = CreateInstance(pool);
            pool.Active.Add(instance);
            instance.transform.SetParent(null, false);
            instance.SetActive(true);
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
                return;

            if (!instanceOwners.TryGetValue(instance, out Pool pool))
            {
                Debug.LogWarning(
                    $"'{instance.name}' was returned to a pool that does not own it.",
                    instance);
                return;
            }

            if (!pool.Active.Remove(instance))
            {
                Debug.LogWarning(
                    $"'{instance.name}' was returned twice to pool '{pool.Key}'.",
                    instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);

            if (!shuttingDown)
                pool.Inactive.Push(instance);
            else
                Destroy(instance);
        }

        public void BeginShutdown()
        {
            shuttingDown = true;
        }

        public void ClearAll()
        {
            foreach (Pool pool in pools.Values)
            {
                foreach (GameObject instance in pool.All)
                {
                    if (instance != null)
                        Destroy(instance);
                }
            }

            pools.Clear();
            instanceOwners.Clear();
        }

        private GameObject CreateInstance(Pool pool)
        {
            GameObject instance = Instantiate(pool.Template, transform);
            instance.name = pool.Template.name;
            instance.SetActive(false);

            pool.All.Add(instance);
            instanceOwners.Add(instance, pool);

            return instance;
        }
    }
}