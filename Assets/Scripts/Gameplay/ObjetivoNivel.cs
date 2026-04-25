using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger que marca el objetivo del nivel.
/// Cuando el jugador llega a este punto (estacion de servicio, casa, etc.), gana el juego.
/// </summary>
public class ObjetivoNivel : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Nombre de la escena que se carga cuando el jugador gana")]
    [SerializeField] private string nombreEscenaVictoria = "Victoria";

    [Tooltip("Mensaje que aparece cuando llegas al objetivo")]
    [SerializeField] private string mensajeVictoria = "ESCAPASTE DE EL MIKILO!";

    [Header("Efectos")]
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private bool mostrarMensajeEnConsola = true;

    private bool _yaActivo = false;

    /// <summary>
    /// Cuando el jugador entra en el trigger, gana el juego.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Evita que se active multiples veces
        if (_yaActivo)
            return;

        // Verifica que sea el jugador
        if (other.CompareTag("Player"))
        {
            _yaActivo = true;
            ActivarVictoria();
        }
    }

    /// <summary>
    /// Activa la victoria: reproduce sonido, avisa al juego y carga la escena.
    /// </summary>
    private void ActivarVictoria()
    {
        // Reproduce el sonido de victoria
        if (sonidoVictoria != null)
        {
            AudioSource.PlayClipAtPoint(sonidoVictoria, transform.position);
        }

        // Muestra mensaje en consola
        if (mostrarMensajeEnConsola)
        {
            Debug.Log($"[OBJETIVO] {mensajeVictoria}");
        }

        // Avisa a todos los sistemas que el jugador gano
        GameEvents.RaiseGoalReached();

        // Espera un poquito para que se escuche el sonido
        Invoke(nameof(CargarEscenaVictoria), 1.5f);
    }

    /// <summary>
    /// Carga la escena de victoria.
    /// </summary>
    private void CargarEscenaVictoria()
    {
        SceneManager.LoadScene(nombreEscenaVictoria);
    }

    /// <summary>
    /// Para testing: fuerza la activacion de la victoria.
    /// </summary>
    public void ForzarVictoria()
    {
        ActivarVictoria();
    }

    private void OnDrawGizmos()
    {
        // Dibuja una estrella o marker en el editor para ver donde esta el objetivo
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Flecha hacia arriba
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);
    }
}
