// Lo deje comentado por si lo usamos despues. Ahora el GameManager le avisa al HUD directo cuando entra una carta.
//
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.SceneManagement;
// using Vuforia;
//
// [System.Serializable]
// public class CardEvent : UnityEvent<(string nombreId, int ataque, Sprite imagenCarta)> { }
//
// public class HudManager : MonoBehaviour
// {
//     [SerializeField] GameObject hudPrefab; //Referencia del prefab del HUD;
//     private List<GameObject> cartasEscaneadas = new List<GameObject>();
//     public CardEvent targetDetectado;
//     private bool uiActiva = false;
//
//     public void EnviarInfoAlHud(GameObject nuevaCarta)
//     {
//         if(cartasEscaneadas.Contains(nuevaCarta)) return;
//
//         cartasEscaneadas.Add(nuevaCarta);
//
//         Debug.Log($"Señal enviada al HUD con la siguiente inforamción: {nuevaCarta.GetComponent<MonsterController>().EntregarDatos()}");
//
//         targetDetectado?.Invoke(nuevaCarta.GetComponent<MonsterController>().EntregarDatos());
//
//     }
//
// }
//