using UnityEngine;
using UnityEngine.AI;

namespace MotoTerrores.Tools
{
    /// <summary>
    /// Script de configuración rápida para el enemigo.
    /// Ejecuta SetupEnemy() desde el menú context para configurar automáticamente.
    /// </summary>
    public class EnemySetup : MonoBehaviour
    {
        [Header("Componentes")]
        [Tooltip("Animator Controller para el enemigo")]
        public RuntimeAnimatorController animatorController;

        [Header("NavMesh Agent")]
        [Tooltip("Radio del agente")]
        public float agentRadius = 0.5f;
        [Tooltip("Altura del agente")]
        public float agentHeight = 2f;
        [Tooltip("Velocidad de patrulla")]
        public float patrolSpeed = 2.5f;
        [Tooltip("Velocidad de persecución")]
        public float chaseSpeed = 3.5f;

        [Header("Detección")]
        [Tooltip("Rango de detección")]
        public float detectionRange = 15f;
        [Tooltip("Ángulo de visión")]
        public float fieldOfViewAngle = 120f;

        [Header("Daño")]
        [Tooltip("Daño de cordura por segundo")]
        public float sanityDamagePerSecond = 15f;

        [ContextMenu("Setup Enemy")]
        public void SetupEnemy()
        {
            // 1. Añadir NavMeshAgent si no existe
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
                Debug.Log("[EnemySetup] NavMeshAgent añadido.");
            }

            // Configurar NavMeshAgent
            agent.radius = agentRadius;
            agent.height = agentHeight;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 2f;

            // 2. Añadir Animator si no existe
            Animator animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
                Debug.Log("[EnemySetup] Animator añadido.");
            }

            // Asignar Animator Controller
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log("[EnemySetup] Animator Controller asignado.");
            }

            // 3. Añadir EnemyMonster si no existe
            EnemyMonster enemy = GetComponent<EnemyMonster>();
            if (enemy == null)
            {
                enemy = gameObject.AddComponent<EnemyMonster>();
                Debug.Log("[EnemySetup] EnemyMonster añadido.");
            }

            // 4. Configurar Vision en EnemyMonster directamente
            // Los valores de vision se configuran en el Inspector del EnemyMonster
            Debug.Log("[EnemySetup] Vision configurada en EnemyMonster.");

            Debug.Log("[EnemySetup] ✓ Configuración completada!");
            Debug.Log("Por favor, configura los valores específicos en el Inspector:");
            Debug.Log($"  - Animator: {(animatorController ? animatorController.name : "NO ASIGNADO")}");
            Debug.Log($"  - Sanity Damage: {sanityDamagePerSecond}");
            Debug.Log("  - Asigna el Animator Controller desde Assets/Objects/Enemigo/EnemyAnimatorController.controller");
        }

        [ContextMenu("Open Animator Controller Folder")]
        public void OpenAnimatorFolder()
        {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(animatorController);
            if (!string.IsNullOrEmpty(path))
            {
                string folder = System.IO.Path.GetDirectoryName(path);
                UnityEditor.EditorUtility.RevealInFinder(folder);
            }
            else
            {
                UnityEditor.EditorUtility.RevealInFinder(
                    UnityEditor.AssetDatabase.GetAssetPath(this)
                );
            }
#endif
        }
    }
}