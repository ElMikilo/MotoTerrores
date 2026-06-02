using UnityEngine;

public class ItemTronco : MonoBehaviour
{
    [Header("Efecto de Sonido")]
    [SerializeField] private AudioClip sonidoRecoleccion; 
    [SerializeField] [Range(0f, 1f)] private float volumen = 1f; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ContadorTroncos contador = FindObjectOfType<ContadorTroncos>();
            
            if (contador != null)
            {
                contador.SumarTronco();

                if (sonidoRecoleccion != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position, volumen);
                }

                Destroy(gameObject); 
            }
        }
    }
}