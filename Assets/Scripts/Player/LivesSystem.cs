using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de vidas del jugador.
/// Cuando se quedan en 0, el Mikilo gana y se carga la escena de Game Over.
/// </summary>
public class LivesSystem : MonoBehaviour
{
    // Singleton para acceder desde cualquier parte
    public static LivesSystem I { get; private set; }

    [Header("Configuracion")]
    [Tooltip("Cantidad inicial de vidas")]
    [SerializeField] private int startingLives = 3;

    [Tooltip("Si es true, reinicia la escena al perder una vida. Si es false, solo resta vida.")]
    [SerializeField] private bool restartSceneOnLoseLife = false;

    [Header("Escenas")]
    [Tooltip("Nombre de la escena de Game Over")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Text livesText;
    [SerializeField] private UnityEngine.UI.Slider livesSlider;

    [Header("Audio")]
    [SerializeField] private AudioClip vidaPerdidaSonido;
    [SerializeField] private AudioSource audioSource;

    private int _lives;

    /// <summary>
    /// Inicializa el singleton y las vidas.
    /// </summary>
    void Awake()
    {
        // Singleton pattern
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);

        // Inicializa las vidas
        _lives = startingLives;

        // Asegura que hay AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Cuando se carga una escena, actualiza la UI de vidas.
    /// </summary>
    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        UpdateUI();
    }

    /// <summary>
    /// Actualiza los elementos de UI (texto y slider).
    /// </summary>
    private void UpdateUI()
    {
        if (livesText != null)
        {
            livesText.text = $"VIDAS: {_lives}";
        }

        if (livesSlider != null)
        {
            livesSlider.maxValue = startingLives;
            livesSlider.value = _lives;
        }
    }

    /// <summary>
    /// El Mikilo atrapa al jugador, pierde una vida.
    /// </summary>
    public void LoseLife()
    {
        _lives = Mathf.Max(0, _lives - 1);
        UpdateUI();

        // Sonido de vida perdida
        if (vidaPerdidaSonido != null && audioSource != null)
        {
            audioSource.PlayOneShot(vidaPerdidaSonido);
        }

        // Verifica si perdio todas las vidas
        if (_lives <= 0)
        {
            Debug.Log("[LivesSystem] GAME OVER - El Mikilo te atrapo!");
            TriggerGameOver();
            return;
        }

        // Si esta configurado para reiniciar escena
        if (restartSceneOnLoseLife)
        {
            Debug.Log($"[LivesSystem] Perdiste una vida. Vidas restantes: {_lives}");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Carga la escena de Game Over cuando el jugador pierde todas las vidas.
    /// </summary>
    void TriggerGameOver()
    {
        // Avisa a todos los sistemas que el juego termino
        // (Por ejemplo, para pausar musica, etc.)
        GameEvents.RaiseLevelRestart();

        // Espera un poco antes de cargar la escena
        Invoke(nameof(CargarGameOver), 1.5f);
    }

    /// <summary>
    /// Carga la escena de Game Over.
    /// </summary>
    private void CargarGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }

    /// <summary>
    /// Resetea las vidas al valor inicial. Usado para nuevos juegos.
    /// </summary>
    public void ResetLives()
    {
        _lives = startingLives;
        UpdateUI();
    }

    /// <summary>
    /// Agrega una vida extra.
    /// </summary>
    public void AddLife()
    {
        _lives++;
        UpdateUI();
    }

    /// <summary>
    /// Propiedad para saber las vidas actuales (encapsulamiento).
    /// </summary>
    public int CurrentLives => _lives;

    /// <summary>
    /// Verifica si el jugador sigue vivo.
    /// </summary>
    public bool IsAlive => _lives > 0;
}
