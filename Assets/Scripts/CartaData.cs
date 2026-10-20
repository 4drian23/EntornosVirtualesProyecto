using UnityEngine;

public enum Equipo { Rojo, Azul }

// Un asset por carta. Se crean con click derecho > Create > Duelo > Carta
[CreateAssetMenu(menuName = "Duelo/Carta")]
public class CartaData : ScriptableObject
{
    [Header("Informacion y Estadisticas")]
    public string nombre;
    public int puntos;
    public Equipo equipo;

    [Header("Visual")]
    [Tooltip("Imagen de la carta, se muestra en el HUD")]
    public Sprite imagen;

    [Tooltip("Prefab del monstruo. Por ahora son cubos, despues van los modelos de verdad")]
    public GameObject monstruo;
}
