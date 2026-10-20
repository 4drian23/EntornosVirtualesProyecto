using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEquipo : MonoBehaviour
{
    [Tooltip("Nombre de la escena del juego")]
    [SerializeField] string escenaJuego = "MonsterDuelManaos";

    // Los llaman los botones del menu
    public void ElegirRojo()
    {
        Entrar(Equipo.Rojo);
    }

    public void ElegirAzul()
    {
        Entrar(Equipo.Azul);
    }

    private void Entrar(Equipo eq)
    {
        Sesion.equipo = eq;
        Sesion.eligio = true;

        SceneManager.LoadScene(escenaJuego);
    }
}
