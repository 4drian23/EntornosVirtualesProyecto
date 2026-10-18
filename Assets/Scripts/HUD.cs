//using Microsoft.Unity.VisualStudio.Editor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    [SerializeField] Image imageComponentPlayer1; //Referencia al componente Image del hud del jugador 1;
    [SerializeField] Image imageComponentPlayer2; //Referencia al componente Image del hud del jugador 2;


    private Dictionary<string, Sprite> bancoDeCartas = new Dictionary<string, Sprite>();
    
    private Sprite cartaActiva; //Carta que se muestra;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void ActualizarImagen(string nombreID)
    {
        if (bancoDeCartas.ContainsKey(nombreID))
        {
            cartaActiva = bancoDeCartas[nombreID];
            imageComponentPlayer1.sprite = cartaActiva;
        }
    }

    public void RecibirCartaNueva(string nombreID, Sprite nuevaCarta)
    {
        if (bancoDeCartas.ContainsKey(nombreID)) return;

        Debug.Log($"Se recibio la siguiente info: {nombreID}, {nuevaCarta}");
        bancoDeCartas.Add(nombreID, nuevaCarta);



       ActualizarImagen(nombreID);
    }


}
