using System.Collections;
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

        StartCoroutine(Cargar());
    }

    // Si cargo la escena en el mismo click el boton se queda sin su Image y tira MissingReferenceException, por eso espero un frame
    private IEnumerator Cargar()
    {
        yield return null;
        SceneManager.LoadScene(escenaJuego);
    }
}
