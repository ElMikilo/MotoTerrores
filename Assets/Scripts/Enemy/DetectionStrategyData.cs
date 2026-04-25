using UnityEngine;

/// <summary>
/// Clase base para las estrategias de deteccion del Mikilo.
/// Guarda datos y metodos compartidos entre VisionDetectionStrategy y SoundDetectionStrategy.
/// NO hereda de MonoBehaviour porque es solo una clase de datos.
/// </summary>
[System.Serializable]
public class DetectionStrategyData
{
    [Header("Configuracion Comun")]
    [Tooltip("Tiempo que el Mikilo recuerda la ultima posicion del jugador")]
    public float memorySeconds = 1.5f;

    // Datos protegidos que usan las estrategias
    protected float _lastSeenTime;
    protected Vector3 _lastSeenPos;

    /// <summary>
    /// Verifica si el objetivo esta dentro de la ventana de memoria del Mikilo.
    /// Cuando el jugador se esconde, El Mikilo sigue buscando por un tiempo.
    /// </summary>
    public bool IsInMemoryWindow()
    {
        return (Time.time - _lastSeenTime) <= memorySeconds;
    }

    /// <summary>
    /// Obtiene la ultima posicion conocida del jugador.
    /// </summary>
    public Vector3 GetLastKnownPosition()
    {
        return _lastSeenPos;
    }

    /// <summary>
    /// Actualiza la memoria con la posicion actual del jugador.
    /// </summary>
    public void UpdateMemory(Vector3 playerPosition)
    {
        _lastSeenPos = playerPosition;
        _lastSeenTime = Time.time;
    }

    /// <summary>
    /// Calcula cuanto tiempo paso desde la ultima vez que vio al jugador.
    /// </summary>
    public float GetTimeSinceLastSeen()
    {
        return Time.time - _lastSeenTime;
    }

    /// <summary>
    /// Resetea la memoria (para cuando El Mikilo vuelve a patrulla).
    /// </summary>
    public void ResetMemory()
    {
        _lastSeenTime = 0f;
        _lastSeenPos = Vector3.zero;
    }
}
