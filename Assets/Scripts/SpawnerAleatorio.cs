using UnityEngine;

public class SpawnerAleatorio : MonoBehaviour
{
    public GameObject player;
    public GameObject mikilo;

    public Transform[] puntosPlayer; 
    public Transform[] puntosMikilo; 

    void Awake()
    {
        int indexP = Random.Range(0, puntosPlayer.Length);
        int indexM = Random.Range(0, puntosMikilo.Length);

        player.transform.position = puntosPlayer[indexP].position;
        mikilo.transform.position = puntosMikilo[indexM].position;
    }
}