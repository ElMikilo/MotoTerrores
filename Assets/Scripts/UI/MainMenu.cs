using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menu principal del juego.
/// Este script va en un GameObject vacio en la escena del menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Configuracion de Escenas")]
    [Tooltip("Nombre de la escena del juego principal")]
    [SerializeField] private string nombreEscenaJuego = "GameScene";

    [Tooltip("Nombre de la escena del tutorial (opcional)")]
    [SerializeField] private string nombreEscenaTutorial = "";

    [Header("Audio")]
    [SerializeField] private AudioClip botonSonido;
    [SerializeField] private AudioSource fuenteAudio;

    void Start()
    {
        // Asegura que el cursor este visible en el menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Por si no tiene AudioSource
        if (fuenteAudio == null)
            fuenteAudio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Empieza el juego principal.
    /// Llama esto desde el boton "JUGAR" del menu.
    /// </summary>
    public void Jugar()
    {
        ReproducirSonido();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    /// <summary>
    /// Empieza el tutorial si existe.
    /// Llama esto desde el boton "TUTORIAL" del menu.
    /// </summary>
    public void Tutorial()
    {
        if (string.IsNullOrEmpty(nombreEscenaTutorial))
        {
            Debug.Log("No hay escena de tutorial configurada.");
            return;
        }

        ReproducirSonido();
        SceneManager.LoadScene(nombreEscenaTutorial);
    }

    /// <summary>
    /// Cierra el juego.
    /// Llama esto desde el boton "SALIR" del menu.
    /// </summary>
    public void Salir()
    {
        ReproducirSonido();
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Reproduce el sonido del boton.
    /// </summary>
    private void ReproducirSonido()
    {
        if (botonSonido != null && fuenteAudio != null)
        {
            fuenteAudio.PlayOneShot(botonSonido);
        }
    }
}
