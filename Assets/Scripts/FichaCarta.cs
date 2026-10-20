using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Una tarjetita del panel de cartas: imagen, nombre y puntos. PantallaJuego la clona desde la plantilla de la escena, una por carta
public class FichaCarta : MonoBehaviour
{
    [SerializeField] Image imagen;
    [SerializeField] TMP_Text nombre;
    [SerializeField] TMP_Text puntos;

    public void Mostrar(CartaData carta)
    {
        imagen.sprite = carta.imagen;
        imagen.enabled = carta.imagen != null; // si no tiene imagen queda solo el nombre, no un cuadrado blanco

        nombre.text = carta.nombre;
        puntos.text = $"{carta.puntos} pts";
    }
}
