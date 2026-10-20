using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Maneja lo q se ve en pantalla: el mensaje de arriba, el cartel para elegir rival y el boton de volver al menu
public class PantallaJuego : MonoBehaviour
{
    [Header("Mensaje")]
    [SerializeField] TMP_Text textoMensaje;

    [Header("Eleccion de rival")]
    [SerializeField] GameObject panelEleccion;
    [SerializeField] TMP_Text tituloEleccion;
    [SerializeField] Button[] botones; // hay 3, se prenden los q hagan falta

    [Header("Fin de la partida")]
    [SerializeField] GameObject botonMenu;

    private Action<int> alElegir;

    void Start()
    {
        panelEleccion.SetActive(false);
        botonMenu.SetActive(false);

        for (int i = 0; i < botones.Length; i++)
        {
            int idx = i; // ojo, si usas i directo en el lambda todos los botones terminan devolviendo el ultimo
            botones[i].onClick.AddListener(() => Elegido(idx));
        }
    }

    public void Mensaje(string txt)
    {
        textoMensaje.text = txt;
    }

    public void MostrarEleccion(string titulo, List<string> opciones, Action<int> callback)
    {
        alElegir = callback;
        tituloEleccion.text = titulo;
        panelEleccion.SetActive(true); // primero prendo el panel, si no GetComponentInChildren no encuentra los textos

        for (int i = 0; i < botones.Length; i++)
        {
            bool hay = i < opciones.Count;
            botones[i].gameObject.SetActive(hay);

            if (hay) botones[i].GetComponentInChildren<TMP_Text>(true).text = opciones[i];
        }
    }

    public void OcultarEleccion()
    {
        panelEleccion.SetActive(false);
    }

    public void MostrarBotonMenu()
    {
        botonMenu.SetActive(true);
    }

    public void VolverAlMenu() // lo llama el boton
    {
        SceneManager.LoadScene("Menu");
    }

    private void Elegido(int idx)
    {
        panelEleccion.SetActive(false);

        var cb = alElegir;
        alElegir = null; // por si tocan dos veces rapido
        cb?.Invoke(idx);
    }
}
