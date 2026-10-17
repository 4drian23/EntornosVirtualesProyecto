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

    [Tooltip("Prefab del monstruo")]
    public GameObject monstruo; // Todavia no se usa, por ahora solo hay un texto arriba de la carta.
}
