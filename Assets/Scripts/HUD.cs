using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Tooltip("Referencia de los componentes Image")]
    [SerializeField] Image imageComponentPlayer1; //Referencia al componente Image del hud del jugador 1 (equipo rojo);
    [SerializeField] Image imageComponentPlayer2; //Referencia al componente Image del hud del jugador 2 (equipo azul);

    private Dictionary<string, CartaData> bancoDeCartas = new Dictionary<string, CartaData>(); //Diccionario para almacenar la informacion necesaria de cada carta;

    private void ActualizarImagen(string nombreKey) //Cada vez que se llame a esta funcion, se actualiza la imagen frontal del equipo que corresponda;
    {
        if (!bancoDeCartas.ContainsKey(nombreKey)) return;

        CartaData carta = bancoDeCartas[nombreKey];
        Image destino = carta.equipo == Equipo.Rojo ? imageComponentPlayer1 : imageComponentPlayer2;

        if (destino != null && carta.imagen != null) destino.sprite = carta.imagen;
    }

    public void RecibirCartaNueva(CartaData carta) //Lo llama el GameManager cuando entra una carta a la mesa. Antes recibia una tupla, ahora recibe la CartaData directo;
    {
        if (!bancoDeCartas.ContainsKey(carta.nombre))
        {
            Debug.Log($"Se recibio la siguiente info: {carta.nombre}, {carta.imagen}");
            bancoDeCartas.Add(carta.nombre, carta);
        }

        ActualizarImagen(carta.nombre);
    }


}
