using UnityEngine;

/// <summary>
/// Estrategia de deteccion por vision para El Mikilo.
/// El Mikilo ve al jugador si esta dentro del rango y angulo de vision.
/// Hereda de DetectionStrategyData para compartir logica con SoundDetectionStrategy.
/// </summary>
public class VisionDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [Header("Vision")]
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    public float DetectionRange { get => detectionRange; set => detectionRange = value; }
    
    [SerializeField, Range(0f, 360f)] private float fieldOfViewAngle = 120f;
    public float FieldOfViewAngle { get => fieldOfViewAngle; set => fieldOfViewAngle = value; }
    
    [SerializeField] private float eyeHeight = 1.6f;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask occludersMask = ~0;
    [SerializeField] private bool requireLineOfSight = true;

    // HERENCIA: Usa la clase base para datos compartidos
    private DetectionStrategyData _strategyData = new DetectionStrategyData();

    private EnemyMonster _owner;

    /// <summary>
    /// Inicializa la estrategia guardando referencia al Mikilo.
    /// </summary>
    public void Initialize(EnemyMonster owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// Detecta si el jugador esta en el campo de vision de El Mikilo.
    /// </summary>
    public bool Detect(Transform target, out Vector3 targetPos)
    {
        targetPos = Vector3.zero;

        if (_owner == null || target == null)
            return false;

        Vector3 from = _owner.transform.position + Vector3.up * eyeHeight;
        Vector3 to = target.position + Vector3.up * eyeHeight;

        float dist = Vector3.Distance(from, to);

        // Verificar distancia
        if (dist > detectionRange)
        {
            // Si esta fuera de rango pero dentro de memoria, memoriza la ultima posicion
            if (_strategyData.IsInMemoryWindow())
            {
                targetPos = _strategyData.GetLastKnownPosition();
            }
            return false;
        }

        // Verificar angulo de vision
        Vector3 dir = (to - from).normalized;
        float angle = Vector3.Angle(_owner.transform.forward, dir);

        if (angle > fieldOfViewAngle * 0.5f)
        {
            // Esta fuera del angulo de vision
            if (_strategyData.IsInMemoryWindow())
            {
                targetPos = _strategyData.GetLastKnownPosition();
            }
            return false;
        }

        // Verificar linea de vista (obstaculos)
        if (requireLineOfSight)
        {
            if (Physics.Linecast(from, to, out RaycastHit hit, occludersMask, QueryTriggerInteraction.Ignore))
            {
                // Hay algo bloqueando la vista
                if (!hit.transform.IsChildOf(target))
                {
                    if (_strategyData.IsInMemoryWindow())
                    {
                        targetPos = _strategyData.GetLastKnownPosition();
                    }
                    return false;
                }
            }
        }

        // VIO AL JUGADOR! Actualiza la memoria
        _strategyData.UpdateMemory(target.position);
        targetPos = _strategyData.GetLastKnownPosition();
        return true;
    }

    // Para usar en el inspector
    public float MemorySeconds
    {
        get => _strategyData.memorySeconds;
        set => _strategyData.memorySeconds = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Dibuja el cono de vision
        if (fieldOfViewAngle < 360f)
        {
            Vector3 forward = transform.forward;
            Quaternion left = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0);
            Quaternion right = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, left * forward * detectionRange);
            Gizmos.DrawRay(transform.position, right * forward * detectionRange);

            // Arcos del cono
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
