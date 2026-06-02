using UnityEngine;
using UnityEngine.SceneManagement; 

public class VictoriaCasa : MonoBehaviour
{
    [Header("UI de Advertencia")]
    [SerializeField] private GameObject canvasAdvertencia; 
    [SerializeField] private float tiempoVisible = 3f;     

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ContadorTroncos contador = FindObjectOfType<ContadorTroncos>();

            // Condición de Victoria: Si tiene los troncos, cambia de escena
            if (contador != null && contador.TieneTroncosSuficientes())
            {
                SceneManager.LoadScene("EscenaVictoria"); 
            }
            // Condición de Advertencia: Si NO tiene los troncos suficientes
            else
            {
                MostrarMensajeAdvertencia();
            }
        }
    }

    private void MostrarMensajeAdvertencia()
    {
        if (canvasAdvertencia != null)
        {
            // Cancelamos cualquier ocultado previo por si el jugador entra al trigger repetidamente
            CancelInvoke("OcultarAdvertencia");
            
            // Activamos el Canvas del mensaje para que el jugador lo vea
            canvasAdvertencia.SetActive(true);

            // Programamos que se ejecute la función de ocultar tras los segundos configurados
            Invoke("OcultarAdvertencia", tiempoVisible);
        }
        else
        {
            Debug.LogWarning("No se asignó el Canvas de advertencia en el script VictoriaCasa.");
        }
    }

    private void OcultarAdvertencia()
    {
        if (canvasAdvertencia != null)
        {
            canvasAdvertencia.SetActive(false);
        }
    }
}