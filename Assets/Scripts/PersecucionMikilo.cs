using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersecucionMikilo : MonoBehaviour
{
    private Transform objetivo;
    private NavMeshAgent agente;

    [Header("Configuracion de Movimiento")]
    public float velocidadBase = 4f;
    public float velocidadRabia = 6f;
    public float aceleracion = 6f;

    [Header("Referencias")]
    public ContadorTroncos contadorTroncos;

    [Header("Configuracion del Screamer")]
    public GameObject imagenScreamer; 
    public AudioSource sonidoScreamer; 
    public float tiempoDeEspera = 2.0f; 

    [Header("Sonidos de Movimiento")]
    public AudioSource audioPisadas; 

    [Header("Sonidos de Ambiente")]
    public AudioSource audioRisas; 

    private bool yaMeAtrapo = false;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            objetivo = playerObj.transform;
        }
        
        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidadBase;
        agente.acceleration = aceleracion;
    }

    private void Start()
    {
        StartCoroutine(RisasAleatorias());
    }

    private void Update()
    {
        if (yaMeAtrapo || objetivo == null) return;

        // Aumentar velocidad cuando el jugador tiene todos los troncos
        if (contadorTroncos != null && contadorTroncos.TieneTroncosSuficientes())
            agente.speed = velocidadRabia;

        agente.SetDestination(objetivo.position);

        // Lógica de Pisadas
        if (agente.velocity.magnitude > 0.2f && !audioPisadas.isPlaying)
        {
            audioPisadas.Play();
        }
        else if (agente.velocity.magnitude <= 0.2f && audioPisadas.isPlaying)
        {
            audioPisadas.Stop();
        }

        // Lógica de Captura
        float distanciaAlPlayer = Vector3.Distance(transform.position, objetivo.position);

        if (distanciaAlPlayer < 1.5f)
        {
            StartCoroutine(SecuenciaScreamer());
        }
    }

    IEnumerator SecuenciaScreamer()
    {
        yaMeAtrapo = true;
        agente.isStopped = true; 
        audioPisadas.Stop(); 

        if (imagenScreamer != null) imagenScreamer.SetActive(true);
        if (sonidoScreamer != null) sonidoScreamer.Play();

        yield return new WaitForSeconds(tiempoDeEspera);

        SceneManager.LoadScene("EscenaDerrota"); 
    }

    IEnumerator RisasAleatorias()
    {
        while (!yaMeAtrapo)
        {
            float proximaRisa = Random.Range(10f, 25f);
            yield return new WaitForSeconds(proximaRisa);

            if (!yaMeAtrapo && audioRisas != null)
            {
                audioRisas.Play();
            }
        }
    }
}