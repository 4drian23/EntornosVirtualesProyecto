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
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnviarInfoAlHud(GameObject cartaEscaneada)
    {
        if(cartasJugadas.Contains(cartaEscaneada)) return;

        cartasJugadas.Add(cartaEscaneada);

        Debug.Log($"Señal enviada con la siguiente inforamción: {cartaEscaneada.GetComponent<MonsterController>().EntregarNombre()}, {cartaEscaneada.GetComponent<MonsterController>().EntregarCarta()}");
        targetDetectado?.Invoke(cartaEscaneada.GetComponent<MonsterController>().EntregarNombre(), cartaEscaneada.GetComponent<MonsterController>().EntregarCarta());

    }

   /* public void ActivarHUD()
    {
        if (uiActiva) return;

        if (hudPrefab != null)
        {
            uiActiva = true;
            StartCoroutine(ActivacionSeguraHud());
        }
    }

    private IEnumerator ActivacionSeguraHud()
    {
        yield return new WaitForEndOfFrame();

        hudPrefab.SetActive(true);

    }*/

}
