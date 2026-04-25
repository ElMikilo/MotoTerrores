using UnityEngine;

/// <summary>
/// Estrategia de deteccion por sonido para El Mikilo.
/// El Mikilo escucha al jugador si hace ruido (correr,轰突, etc.).
/// Hereda de DetectionStrategyData para compartir logica con VisionDetectionStrategy.
/// </summary>
public class SoundDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [Header("Configuracion de Sonido")]
    [Tooltip("Nivel de ruido para que El Mikilo se active")]
    [SerializeField] private float noiseThresholdOn = 0.15f;

    [Tooltip("Nivel de ruido para que El Mikilo se desactive")]
    [SerializeField] private float noiseThresholdOff = 0.10f;

    // HERENCIA: Usa la clase base para datos compartidos
    private DetectionStrategyData _strategyData = new DetectionStrategyData();

    private Collider _volume;
    private float _currentNoise01;
    private bool _playerInRange;
    private Transform _cachedTarget;

    /// <summary>
    /// Inicializa la estrategia. Crea un collider como zona de deteccion de sonido.
    /// </summary>
    public void Initialize(EnemyMonster owner)
    {
        // Busca o crea un collider para detectar si el jugador esta cerca
        _volume = GetComponent<Collider>();
        if (_volume == null)
        {
            _volume = gameObject.AddComponent<BoxCollider>();
        }
        _volume.isTrigger = true;
        enabled = true;
    }

    // Se suscribe al evento de ruido cuando se activa
    void OnEnable()
    {
        GameEvents.OnNoiseChanged += OnNoiseChanged;
    }

    // Se desuscribe cuando se desactiva
    void OnDisable()
    {
        GameEvents.OnNoiseChanged -= OnNoiseChanged;
    }

    /// <summary>
    /// Callback cuando cambia el nivel de ruido del jugador.
    /// </summary>
    void OnNoiseChanged(float noiseLevel)
    {
        _currentNoise01 = noiseLevel;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = false;
    }

    /// <summary>
    /// Detecta si el jugador esta haciendo suficiente ruido para ser escuchado.
    /// </summary>
    public bool Detect(Transform target, out Vector3 targetPos)
    {
        if (_cachedTarget == null)
            _cachedTarget = target;

        // Verifica si el jugador esta en rango
        bool inRange = _playerInRange || IsInsideVolume(target);

        // Verifica si el ruido supera el umbral
        bool loud;
        if (_currentNoise01 >= noiseThresholdOn)
        {
            loud = true;
        }
        else if (_currentNoise01 <= noiseThresholdOff)
        {
            loud = false;
        }
        else
        {
            // Esta en zona intermedia, usa la memoria
            loud = _strategyData.IsInMemoryWindow();
        }

        // Detecto al jugador?
        bool detected = inRange && loud;

        if (detected && _cachedTarget != null)
        {
            // Actualiza la memoria con la posicion
            _strategyData.UpdateMemory(_cachedTarget.position);
            targetPos = _strategyData.GetLastKnownPosition();
            return true;
        }

        // Si esta en memoria, devuelve la ultima posicion conocida
        if (_strategyData.IsInMemoryWindow())
        {
            targetPos = _strategyData.GetLastKnownPosition();
            return false;
        }

        targetPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Verifica si el objetivo esta dentro del volumen de deteccion.
    /// </summary>
    bool IsInsideVolume(Transform targetTransform)
    {
        if (_volume == null || targetTransform == null)
            return false;

        Vector3 playerPos = targetTransform.position;
        Vector3 closestPoint = _volume.ClosestPoint(playerPos);
        return (closestPoint - playerPos).sqrMagnitude <= 1e-6f;
    }

    // Para usar en el inspector
    public float MemorySeconds
    {
        get => _strategyData.memorySeconds;
        set => _strategyData.memorySeconds = value;
    }
}
