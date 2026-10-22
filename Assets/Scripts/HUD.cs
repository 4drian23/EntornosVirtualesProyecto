using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Tooltip("Referencia de los componentes Image")]
    [SerializeField] Image imageComponentPlayer1; //Referencia al componente Image del hud del jugador 1 (equipo rojo);
    [SerializeField] Image imageComponentPlayer2; //Referencia al componente Image del hud del jugador 2 (equipo azul);

    [Tooltip("Textos donde van los puntos de la carta que se muestra")]
    [SerializeField] TMP_Text puntosPlayer1;
    [SerializeField] TMP_Text puntosPlayer2;

    private List<CartaData> cartasRojas = new List<CartaData>(); //Cartas de cada equipo que estan en la mesa, se muestra la ultima que entro;
    private List<CartaData> cartasAzules = new List<CartaData>();

    private Sprite imagenInicial1; //Lo que tenia cada Image antes de escanear nada (el reverso de la carta), se vuelve a poner cuando no queda ninguna carta del equipo;
    private Sprite imagenInicial2;

    void Awake()
    {
        if (imageComponentPlayer1 != null) imagenInicial1 = imageComponentPlayer1.sprite;
        if (imageComponentPlayer2 != null) imagenInicial2 = imageComponentPlayer2.sprite;

        //Sin carta no se muestra ningun puntaje, los puntos salen siempre de la CartaData;
        Actualizar(Equipo.Rojo);
        Actualizar(Equipo.Azul);
    }

    public void RecibirCartaNueva(CartaData carta) //La llama el GameManager cuando una carta entra a la mesa;
    {
        var lista = Lista(carta.equipo);
        lista.Remove(carta);
        lista.Add(carta);

        Actualizar(carta.equipo);
    }

    public void QuitarCarta(CartaData carta) //Y cuando sale de la mesa;
    {
        Lista(carta.equipo).Remove(carta);

        Actualizar(carta.equipo);
    }

    private void Actualizar(Equipo equipo) //Pone la imagen y los puntos de la ultima carta del equipo que haya en la mesa;
    {
        var lista = Lista(equipo);
        CartaData carta = lista.Count > 0 ? lista[lista.Count - 1] : null;

        Image imagen = equipo == Equipo.Rojo ? imageComponentPlayer1 : imageComponentPlayer2;
        TMP_Text puntos = equipo == Equipo.Rojo ? puntosPlayer1 : puntosPlayer2;

        if (imagen != null)
        {
            imagen.sprite = carta != null && carta.imagen != null ? carta.imagen : (equipo == Equipo.Rojo ? imagenInicial1 : imagenInicial2);
        }

        if (puntos != null)
        {
            puntos.text = carta != null ? carta.puntos.ToString() : "";
        }
    }

    private List<CartaData> Lista(Equipo equipo)
    {
        return equipo == Equipo.Rojo ? cartasRojas : cartasAzules;
    }
}
