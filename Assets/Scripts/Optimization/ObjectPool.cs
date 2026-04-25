using System;
using System.Collections.Generic;
using UnityEngine;

namespace MotoTerrores.Optimization
{
    /// <summary>
    /// Object Pooling System para reutilizar objetos y evitar Instantiate/Destroy
    /// Autor: Claude - Optimización MotoTerrores
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [Serializable]
        public class PoolConfig
        {
            public string poolId;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
            public bool expandable = true;
        }

        [Header("Configuración de Pool")]
        [Tooltip("Lista de pools configurados")]
        public List<PoolConfig> pools = new List<PoolConfig>();

        private static ObjectPool _instance;
        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindAnyObjectByType<ObjectPool>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[ObjectPool]");
                        _instance = go.AddComponent<ObjectPool>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, Queue<GameObject>> _pooledObjects = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<GameObject, string> _objectToPoolId = new Dictionary<GameObject, string>();
        private Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var config in pools)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning($"[ObjectPool] Prefab no asignado para pool: {config.poolId}");
                    continue;
                }

                Queue<GameObject> objectQueue = new Queue<GameObject>();

                for (int i = 0; i < config.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(config);
                    obj.SetActive(false);
                    objectQueue.Enqueue(obj);
                }

                _pooledObjects[config.poolId] = objectQueue;
                _prefabLookup[config.poolId] = config.prefab;
            }
        }

        private GameObject CreateNewObject(PoolConfig config)
        {
            GameObject obj = Instantiate(config.prefab, transform);
            _objectToPoolId[obj] = config.poolId;
            return obj;
        }

        /// <summary>
        /// Obtiene un objeto del pool (o crea uno nuevo si es expandible)
        /// </summary>
        public GameObject GetFromPool(string poolId, Vector3 position, Quaternion rotation)
        {
            if (!_pooledObjects.ContainsKey(poolId))
            {
                Debug.LogError($"[ObjectPool] Pool no encontrado: {poolId}");
                return null;
            }

            Queue<GameObject> pool = _pooledObjects[poolId];
            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                // Buscar config para ver si es expandible
                var config = pools.Find(p => p.poolId == poolId);
                if (config != null && config.expandable)
                {
                    if (pool.Count < config.maxSize)
                    {
                        obj = CreateNewObject(config);
                    }
                    else
                    {
                        Debug.LogWarning($"[ObjectPool] Pool '{poolId}' alcanzó máximo ({config.maxSize})");
                        return null;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{poolId}' vacío y no expandible");
                    return null;
                }
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Versión simplificada sin posición/rotación
        /// </summary>
        public GameObject GetFromPool(string poolId)
        {
            return GetFromPool(poolId, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Devuelve un objeto al pool
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            if (!_objectToPoolId.TryGetValue(obj, out string poolId))
            {
                Debug.LogWarning($"[ObjectPool] Objeto no pertenece a ningún pool: {obj.name}");
                Destroy(obj);
                return;
            }

            if (!_pooledObjects.ContainsKey(poolId))
            {
                Debug.LogError($"[ObjectPool] Pool no encontrado al devolver: {poolId}");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            _pooledObjects[poolId].Enqueue(obj);
        }

        /// <summary>
        /// Precalentar un pool (crear objetos adicionales)
        /// </summary>
        public void Prewarm(string poolId, int count)
        {
            if (!_pooledObjects.ContainsKey(poolId)) return;

            var config = pools.Find(p => p.poolId == poolId);
            if (config == null) return;

            Queue<GameObject> pool = _pooledObjects[poolId];

            for (int i = 0; i < count && pool.Count < config.maxSize; i++)
            {
                GameObject obj = CreateNewObject(config);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Limpiar un pool específico
        /// </summary>
        /// <summary>
        /// Limpiar un pool específico
        /// </summary>
        public void ClearPool(string poolId)
        {
            if (!_pooledObjects.ContainsKey(poolId)) return;


            Queue<GameObject> pool = _pooledObjects[poolId];
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(obj);
                    else
                        Destroy(obj);
                }
            }
        }

        /// <summary>
        /// Obtener estadísticas del pool
        /// </summary>
        public Dictionary<string, int> GetPoolStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            foreach (var kvp in _pooledObjects)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Componente para marcar objetos como poolables automáticamente
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("ID del pool al que pertenece este objeto")]
        public string poolId = "default";

        [Tooltip("Tiempo antes de volver al pool (0 = inmediato)")]
        public float autoReturnDelay = 0f;

        private float _returnTime;

        private void OnDisable()
        {
            if (autoReturnDelay > 0)
            {
                _returnTime = Time.time + autoReturnDelay;
            }
        }

        private void Update()
        {
            if (autoReturnDelay > 0 && !gameObject.activeSelf && _returnTime > 0 && Time.time >= _returnTime)
            {
                _returnTime = 0f;
                ReturnToPool();
            }
        }

        public void ReturnToPool()
        {
            ObjectPool.Instance?.ReturnToPool(gameObject);
        }
    }
}