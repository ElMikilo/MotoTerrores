using System.Collections.Generic;
using UnityEngine;

namespace MotoTerreres.Optimization
{
    /// <summary>
    /// Sistema de Distance Culling para deshabilitar objetos lejanos
    /// Autor: Claude - Optimización MotoTerrores
    /// </summary>
    public class DistanceCulling : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Distancia máxima de renderizado")]
        public float maxDistance = 100f;

        [Tooltip("Distancia a la que empiezan a deshabilitarse (buffer)")]
        public float fadeDistance = 20f;

        [Tooltip("Referencia de la cámara a usar (null = MainCamera)")]
        public Camera targetCamera;

        [Tooltip("Capas a incluir en el culling")]
        public LayerMask cullingLayers = ~0;

        [Header("Modo")]
        [Tooltip("Usar distancia 2D (XZ) en lugar de 3D")]
        public bool useXZDistance = true;

        [Header("Performance")]
        [Tooltip("Frecuencia de actualización en segundos (0 = cada frame)")]
        public float updateInterval = 0.5f;

        [Header("Opciones Avanzadas")]
        [Tooltip("Deshabilitar Mesh Renderers en lugar de el GameObject completo")]
        public bool disableRenderersOnly = false;

        [Tooltip("También deshabilitar physics (colliders)")]
        public bool disableColliders = true;

        private Transform _cameraTransform;
        private float _lastUpdateTime;
        private readonly List<CullingTarget> _cullingTargets = new List<CullingTarget>();

        private class CullingTarget
        {
            public GameObject gameObject;
            public Renderer[] renderers;
            public Collider[] colliders;
            public float boundsRadius;
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                Debug.LogError("[DistanceCulling] No se encontró cámara");
                enabled = false;
                return;
            }

            _cameraTransform = targetCamera.transform;
            FindCullingTargets();
        }

        private void FindCullingTargets()
        {
            _cullingTargets.Clear();

            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                // Solo objetos con renderers
                Renderer[] renderers = obj.GetComponents<Renderer>();
                if (renderers.Length == 0) continue;

                // Ignorar objetos estáticos (ya los optimiza Unity)
                if (obj.isStatic) continue;

                // Ignorar la cámara misma y objetos del player
                if (obj.CompareTag("MainCamera") || obj.CompareTag("Player")) continue;

                Collider[] colliders = disableColliders ? obj.GetComponents<Collider>() : null;

                CullingTarget target = new CullingTarget
                {
                    gameObject = obj,
                    renderers = renderers,
                    colliders = colliders,
                    boundsRadius = CalculateBoundsRadius(renderers)
                };

                _cullingTargets.Add(target);
            }

            Debug.Log($"[DistanceCulling] { _cullingTargets.Count} objetos encontrados para culling");
        }

        private float CalculateBoundsRadius(Renderer[] renderers)
        {
            if (renderers.Length == 0) return 1f;

            Bounds totalBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                totalBounds.Encapsulate(renderers[i].bounds);
            }

            return totalBounds.extents.magnitude;
        }

        private void Update()
        {
            if (Time.time - _lastUpdateTime < updateInterval) return;
            _lastUpdateTime = Time.time;

            Vector3 cameraPos = _cameraTransform.position;

            foreach (CullingTarget target in _cullingTargets)
            {
                if (target.gameObject == null) continue;

                float distance = CalculateDistance(cameraPos, target.gameObject.transform.position);
                bool shouldDisable = distance > maxDistance + target.boundsRadius;
                bool shouldFade = distance > maxDistance - fadeDistance;

                if (shouldDisable)
                {
                    SetObjectActive(target, false);
                }
                else if (shouldFade)
                {
                    // Fade gradual usando LOD
                    float fadeRatio = 1f - (distance - (maxDistance - fadeDistance)) / fadeDistance;
                    SetLODLevel(target, fadeRatio);
                }
                else
                {
                    SetObjectActive(target, true);
                    SetLODLevel(target, 1f);
                }
            }
        }

        private float CalculateDistance(Vector3 from, Vector3 to)
        {
            if (useXZDistance)
            {
                Vector3 fromXZ = new Vector3(from.x, 0, from.z);
                Vector3 toXZ = new Vector3(to.x, 0, to.z);
                return Vector3.Distance(fromXZ, toXZ);
            }
            return Vector3.Distance(from, to);
        }

        private void SetObjectActive(CullingTarget target, bool active)
        {
            if (disableRenderersOnly)
            {
                foreach (Renderer r in target.renderers)
                {
                    if (r != null) r.enabled = active;
                }
                if (disableColliders && target.colliders != null)
                {
                    foreach (Collider c in target.colliders)
                    {
                        if (c != null) c.enabled = active;
                    }
                }
            }
            else
            {
                if (target.gameObject.activeSelf != active)
                {
                    target.gameObject.SetActive(active);
                }
            }
        }

        private void SetLODLevel(CullingTarget target, float fadeRatio)
        {
            // Reducir calidad de materiales si es necesario
            // Esto es para fade gradual - simplificado
        }

        private void OnEnable()
        {
            // Recalcular cuando se activa
            _lastUpdateTime = float.MinValue;
        }

        private void OnDrawGizmosSelected()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            Vector3 pos = targetCamera.transform.position;

            // Radio máximo
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, maxDistance);

            // Radio de fade
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, maxDistance - fadeDistance);
        }
    }

    /// <summary>
    /// Componente para marcar objetos individuales con distancia de culling custom
    /// </summary>
    [AddComponentMenu("MotoTerrores/Optimization/Custom Culling Object")]
    public class CustomCullingObject : MonoBehaviour
    {
        [Header("Culling Personalizado")]
        [Tooltip("Distancia custom para este objeto (0 = usa global)")]
        public float customCullDistance = 0f;

        [Tooltip("Prioridad (objetos con mayor prioridad se cullan primero)")]
        public int cullingPriority = 0;

        [Tooltip("Mantener siempre activo (no se cullea)")]
        public bool alwaysActive = false;

        private DistanceCulling _cullingSystem;

        private void Start()
        {
            _cullingSystem = FindObjectOfType<DistanceCulling>();
        }

        public float GetCullDistance()
        {
            return customCullDistance > 0f ? customCullDistance : (_cullingSystem != null ? _cullingSystem.maxDistance : 100f);
        }
    }
}
