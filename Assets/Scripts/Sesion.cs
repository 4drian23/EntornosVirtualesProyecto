using UnityEngine;

// Guarda lo q eligio el jugador en el menu. Es static asi no se pierde al cambiar de escena.
public static class Sesion
{
    public static Equipo equipo = Equipo.Rojo;
    public static bool eligio = false; // si abris la escena del juego directo desde el editor queda en rojo
}
