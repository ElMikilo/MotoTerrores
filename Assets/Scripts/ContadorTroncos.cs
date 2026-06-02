using UnityEngine;
using TMPro; // Necesario para interactuar con TextMesh Pro

public class ContadorTroncos : MonoBehaviour
{
    [Header("Configuración del Juego")]
    [SerializeField] private int troncosObjetivo = 15;
    private int troncosActuales = 0;

    [Header("Referencias de Interfaz (UI)")]
    [SerializeField] private TextMeshProUGUI textoMisiones; // El del centro ("Misiones")
    [SerializeField] private TextMeshProUGUI textoContador; // El de la esquina ("Contador")

    private void Start()
    {
        // Inicializamos la interfaz al arrancar el juego
        ActualizarInterfaz();
    }

    // Método que llaman los troncos al ser recolectados
    public void SumarTronco()
    {
        troncosActuales++;
        ActualizarInterfaz();
    }

    // Método público para que la casa verifique el estado
    public bool TieneTroncosSuficientes()
    {
        return troncosActuales >= troncosObjetivo;
    }

    // Centraliza la actualización de todo el texto en pantalla
    private void ActualizarInterfaz()
    {
        // 1. Actualizamos el contador numérico (ej: "0/15")
        if (textoContador != null)
        {
            textoContador.text = troncosActuales + "/" + troncosObjetivo;
        }

        // 2. Evaluamos la condición para cambiar el mensaje del centro
        if (textoMisiones != null)
        {
            if (TieneTroncosSuficientes())
            {
                textoMisiones.text = "Vuelve a tu casa";
            }
            else
            {
                textoMisiones.text = "Recolecta 15 Troncos";
            }
        }
    }
}