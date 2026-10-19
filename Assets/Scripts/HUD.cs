using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Tooltip("Referencia de los componentes Image")]
    [SerializeField] Image imageComponentPlayer1; //Referencia al componente Image del hud del jugador 1;
    [SerializeField] Image imageComponentPlayer2; //Referencia al componente Image del hud del jugador 2;

    private Dictionary<string, (int atk, Sprite cartaSprite)> bancoDeCartas = new Dictionary<string, (int atk, Sprite cartaSprite)>(); //Diccionario para almacenar la informacion necesaria de cada carta;
    private Sprite cartaActiva; //Referencia de la carta que se muestra al frente;

    private void ActualizarImagen(string nombreKey) //Cada vez que se llame a esta funcion, se actualiza la imagen frontal. Por ahora se llama en RecibirCartaNueva, pero deberia llamarse cuando el jugador toque la cantalla sobre la ui;
    {
        if (bancoDeCartas.ContainsKey(nombreKey))
        {
            cartaActiva = bancoDeCartas[nombreKey].cartaSprite;
            imageComponentPlayer1.sprite = cartaActiva;
        }
    }

    public void RecibirCartaNueva((string nombreID, int ataque, Sprite imagenCarta) datosNuevaCarta) //Se actualiza el diccionario la primera vez que se escanea cada carta en juego;
    {
        if (bancoDeCartas.ContainsKey(datosNuevaCarta.nombreID)) return;

        Debug.Log($"Se recibio la siguiente info: {datosNuevaCarta.nombreID}, {datosNuevaCarta.imagenCarta}");
        bancoDeCartas.Add(datosNuevaCarta.nombreID, (datosNuevaCarta.ataque, datosNuevaCarta.imagenCarta));

       ActualizarImagen(datosNuevaCarta.nombreID);
    }


}
