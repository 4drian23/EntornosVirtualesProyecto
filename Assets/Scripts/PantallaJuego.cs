using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Maneja lo q se ve en pantalla: el mensaje de arriba, el cartel para elegir rival, el panel de cartas y el boton de volver al menu
public class PantallaJuego : MonoBehaviour
{
    [Header("Mensaje")]
    [SerializeField] TMP_Text textoMensaje;

    [Header("Eleccion de rival")]
    [SerializeField] GameObject panelEleccion;
    [SerializeField] TMP_Text tituloEleccion;
    [SerializeField] Button[] botones; // hay 3, se prenden los q hagan falta

    [Header("Panel de cartas")]
    [SerializeField] GameObject panelCartas;
    [SerializeField] TMP_Text tituloRojo;
    [SerializeField] TMP_Text tituloAzul;
    [SerializeField] Transform contenedorRojo;  // donde se clonan las fichas de cada equipo
    [SerializeField] Transform contenedorAzul;
    [SerializeField] FichaCarta plantillaFicha; // esta desactivada en la escena, se clona una por carta

    [Header("Fin de la partida")]
    [SerializeField] GameObject botonMenu;

    private Action<int> alElegir;

    void Start()
    {
        panelEleccion.SetActive(false);
        panelCartas.SetActive(false);
        botonMenu.SetActive(false);

        for (int i = 0; i < botones.Length; i++)
        {
            int idx = i; // ojo, si usas i directo en el lambda todos los botones terminan devolviendo el ultimo
            botones[i].onClick.AddListener(() => Elegido(idx));
        }

        ArmarPanelCartas();
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

        // cada equipo tiene 3 cartas asi que con 3 botones alcanza, si algun dia hay mas hay que agregar botones en la escena
        if (opciones.Count > botones.Length) Debug.LogWarning($"Hay {opciones.Count} rivales pero solo {botones.Length} botones");

        for (int i = 0; i < botones.Length; i++)
        {
            bool hay = i < opciones.Count;
            botones[i].gameObject.SetActive(hay);

            if (hay) botones[i].GetComponentInChildren<TMP_Text>(true).text = opciones[i];
        }
    }

    public void OcultarEleccion()
    {
        alElegir = null;
        panelEleccion.SetActive(false);
    }

    public void MostrarCartas() // lo llama el boton "Cartas" y el de cerrar, prende y apaga el panel
    {
        panelCartas.SetActive(!panelCartas.activeSelf);
    }

    public void MostrarBotonMenu()
    {
        botonMenu.SetActive(true);
    }

    public void VolverAlMenu() // lo llama el boton
    {
        StartCoroutine(Cargar());
    }

    // Un frame despues, si no el boton tira MissingReferenceException al quedarse sin su Image
    private System.Collections.IEnumerator Cargar()
    {
        yield return null;
        SceneManager.LoadScene("Menu");
    }

    private void Elegido(int idx)
    {
        panelEleccion.SetActive(false);

        var cb = alElegir;
        alElegir = null; // por si tocan dos veces rapido
        cb?.Invoke(idx);
    }

    // Una ficha por carta de la escena, con imagen, nombre y puntos sacados de su CartaData. Primero las de mas puntos
    private void ArmarPanelCartas()
    {
        var cartas = new List<CartaData>();

        foreach (var t in FindObjectsByType<CartaTracker>())
        {
            if (t.datos != null && !cartas.Contains(t.datos)) cartas.Add(t.datos);
        }

        cartas.Sort((a, b) => b.puntos.CompareTo(a.puntos));

        foreach (var c in cartas)
        {
            var ficha = Instantiate(plantillaFicha, c.equipo == Equipo.Rojo ? contenedorRojo : contenedorAzul);
            ficha.gameObject.SetActive(true);
            ficha.Mostrar(c);
        }

        tituloRojo.text = "Equipo Rojo" + (Sesion.equipo == Equipo.Rojo ? "\n(vos)" : "");
        tituloAzul.text = "Equipo Azul" + (Sesion.equipo == Equipo.Azul ? "\n(vos)" : "");
    }
}
