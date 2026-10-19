using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Vuforia;

[System.Serializable]
public class CardEvent : UnityEvent<string, Sprite> { }

public class FalsoGameManager : MonoBehaviour
{
    private List<GameObject> cartasJugadas = new List<GameObject>();

    public CardEvent targetDetectado;

    [SerializeField] GameObject hudPrefab;

    private bool uiActiva = false;
    

    public void EnviarInfoAlHud(GameObject cartaEscaneada)
    {
        if(cartasJugadas.Contains(cartaEscaneada)) return;

        cartasJugadas.Add(cartaEscaneada);

        Debug.Log($"Señal enviada con la siguiente inforamción: {cartaEscaneada.GetComponent<MonsterController>().EntregarNombre()}, {cartaEscaneada.GetComponent<MonsterController>().EntregarSprite()}");
        targetDetectado?.Invoke(cartaEscaneada.GetComponent<MonsterController>().EntregarNombre(), cartaEscaneada.GetComponent<MonsterController>().EntregarSprite());

    }

}
