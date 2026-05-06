using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorNavegacion : MonoBehaviour
{
    [Header("Paneles del Menú Principal")]
    public GameObject panelMenu;           
    public GameObject panelContexto;       
    public GameObject panelInstrucciones;  

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- FUNCIONES PARA EL MENÚ PRINCIPAL ---

    public void IrAContexto()
    {
        if(panelMenu != null) panelMenu.SetActive(false);
        if(panelContexto != null) panelContexto.SetActive(true);
    }

    public void IrAInstrucciones()
    {
        if(panelContexto != null) panelContexto.SetActive(false);
        if(panelInstrucciones != null) panelInstrucciones.SetActive(true);
    }

    public void EmpezarJuego()
    {
        SceneManager.LoadScene("Juego"); 
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}