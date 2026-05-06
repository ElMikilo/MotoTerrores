using UnityEngine;
using UnityEngine.SceneManagement; // Importante para cambiar escenas

public class VictoriaCasa : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("EscenaVictoria"); 
        }
    }
}