using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    public enum Estado { Esperando, Eligiendo, Peleando, EsperandoRetiro, Terminado }

    [Header("Configuraciones generales")]
    [Tooltip("Ancho de la carta mas un margen")]
    [SerializeField] float tamCasilla = 0.07f; // En metros, no en pixeles.

    [Tooltip("Si las diagonales cuentan como adyacentes")]
    [SerializeField] bool diagonalCuenta = false;

    [Tooltip("Cuanto se tiene q correr una carta para q cuente como jugada (en casillas)")]
    [SerializeField] float umbralMovimiento = 0.6f;

    [Header("Referencias")]
    [SerializeField] PantallaJuego pantalla;

    public Estado estado { get; private set; } = Estado.Esperando;

    private List<CartaTracker> enMesa = new List<CartaTracker>();
    private Dictionary<CartaTracker, Vector3> posAsentada = new Dictionary<CartaTracker, Vector3>(); // ultima posicion "firme" de cada carta
    private Dictionary<CartaTracker, Vector2Int> casillas = new Dictionary<CartaTracker, Vector2Int>();

    private HUD hud; // el de Adrian, lo busca solo en la escena

    // La primera carta que aparece marca donde esta el "arriba" de la grilla
    private bool hayRef;
    private Vector3 refPos;
    private Quaternion refRot;

    private CartaTracker ultimaJugada; // la carta q se puso o se movio por ultima vez, es la q ataca
    private List<CartaTracker> esperandoRetiro = new List<CartaTracker>(); // perdedoras q todavia estan en la mesa

    void Awake()
    {
        inst = this;
    }

    void Start()
    {
        hud = FindFirstObjectByType<HUD>();

        pantalla.Mensaje($"Jugás con el equipo {Sesion.equipo}. Apuntá a las cartas sobre la mesa.");
    }

    void Update()
    {
        if (!hayRef) return;

        ActualizarCasillas();

        if (estado == Estado.Esperando) BuscarDuelo();
        else if (estado == Estado.EsperandoRetiro) RevisarRetiro();
    }

    public void CartaEntro(CartaTracker c)
    {
        if (!hayRef)
        {
            refPos = c.transform.position;
            refRot = c.transform.rotation;
            hayRef = true;
        }

        if (!enMesa.Contains(c)) enMesa.Add(c);

        posAsentada[c] = c.transform.position;
        casillas[c] = Casilla(posAsentada[c]);
        ultimaJugada = c;

        if (hud != null) hud.RecibirCartaNueva(c.datos);

        Debug.Log($"Entro {c.datos.nombre}");
    }

    public void CartaSalio(CartaTracker c)
    {
        enMesa.Remove(c);
        casillas.Remove(c);
        posAsentada.Remove(c);

        if (ultimaJugada == c) ultimaJugada = null;

        Debug.Log($"Salio {c.datos.nombre}");
    }

    private void ActualizarCasillas()
    {
        foreach (var c in enMesa)
        {
            // Si se corrio bastante la damos por movida. Si no, la dejamos donde estaba para q el tracking no haga ruido
            if (Vector3.Distance(c.transform.position, posAsentada[c]) > tamCasilla * umbralMovimiento)
            {
                posAsentada[c] = c.transform.position;
                casillas[c] = Casilla(posAsentada[c]);

                if (estado == Estado.Esperando && !c.Derrotada) ultimaJugada = c;
            }
        }
    }

    private void BuscarDuelo()
    {
        if (HayTresEnLinea(out Equipo ganador))
        {
            Terminar(ganador);
            return;
        }

        if (ultimaJugada == null || !enMesa.Contains(ultimaJugada)) return;

        var rivales = new List<CartaTracker>();

        foreach (var otra in enMesa)
        {
            if (otra == ultimaJugada || otra.datos.equipo == ultimaJugada.datos.equipo || otra.Derrotada) continue;

            if (Adyacentes(casillas[ultimaJugada], casillas[otra])) rivales.Add(otra);
        }

        if (rivales.Count == 0) return;

        var atacante = ultimaJugada;
        ultimaJugada = null; // para q no vuelva a disparar el mismo duelo

        if (rivales.Count == 1)
        {
            StartCoroutine(Pelea(atacante, rivales[0]));
        }
        else
        {
            PedirEleccion(atacante, rivales);
        }
    }

    private void PedirEleccion(CartaTracker atacante, List<CartaTracker> rivales)
    {
        estado = Estado.Eligiendo;

        // Como los celulares no se hablan entre si, el q no elige tiene q marcar lo q eligio el otro
        bool meToca = atacante.datos.equipo == Sesion.equipo;

        string titulo = meToca
            ? $"{atacante.datos.nombre} quedó al lado de {rivales.Count} rivales. ¿Contra cuál pelea?"
            : $"Tu rival elige contra quién pelea {atacante.datos.nombre}. Marcá la que eligió.";

        pantalla.Mensaje(meToca ? "Elegí a tu rival" : "Esperando al rival...");

        var nombres = new List<string>();
        foreach (var r in rivales) nombres.Add($"{r.datos.nombre} ({r.datos.puntos} pts)");

        pantalla.MostrarEleccion(titulo, nombres, idx =>
        {
            if (idx < rivales.Count && enMesa.Contains(rivales[idx]) && enMesa.Contains(atacante))
            {
                StartCoroutine(Pelea(atacante, rivales[idx]));
            }
            else
            {
                estado = Estado.Esperando; // alguna de las cartas ya no esta, se cancela
            }
        });
    }

    private IEnumerator Pelea(CartaTracker a, CartaTracker b)
    {
        estado = Estado.Peleando;
        pantalla.Mensaje($"¡{a.datos.nombre} ({a.datos.puntos}) vs {b.datos.nombre} ({b.datos.puntos})!");

        yield return new WaitForSeconds(0.8f);

        // los dos se tiran contra el otro a la vez
        var ataqueA = StartCoroutine(a.Modelo.Atacar(b.Modelo.transform.position));
        yield return b.Modelo.Atacar(a.Modelo.transform.position);
        yield return ataqueA;

        esperandoRetiro.Clear();

        if (a.datos.puntos == b.datos.puntos)
        {
            a.Derrotada = true;
            b.Derrotada = true;
            esperandoRetiro.Add(a);
            esperandoRetiro.Add(b);

            StartCoroutine(a.Modelo.Morir());
            yield return b.Modelo.Morir();

            pantalla.Mensaje($"Empate. {a.datos.nombre} y {b.datos.nombre} se retiran de la mesa.");
        }
        else
        {
            var gana = a.datos.puntos > b.datos.puntos ? a : b;
            var pierde = gana == a ? b : a;

            pierde.Derrotada = true;
            esperandoRetiro.Add(pierde);

            StartCoroutine(gana.Modelo.Festejar());
            yield return pierde.Modelo.Morir();

            if (gana.datos.equipo == Sesion.equipo)
                pantalla.Mensaje($"¡Victoria! {gana.datos.nombre} venció a {pierde.datos.nombre}. Mantené tu posición en la mesa.");
            else
                pantalla.Mensaje($"Derrota. {pierde.datos.nombre} fue vencida por {gana.datos.nombre}. Retirá tu carta de la mesa para seguir.");
        }

        estado = Estado.EsperandoRetiro;
    }

    private void RevisarRetiro()
    {
        esperandoRetiro.RemoveAll(c => !enMesa.Contains(c));

        if (esperandoRetiro.Count > 0) return; // todavia no sacaron la carta

        estado = Estado.Esperando;
        pantalla.Mensaje("Listo. Sigue el juego.");
    }

    private void Terminar(Equipo ganador)
    {
        estado = Estado.Terminado;
        pantalla.OcultarEleccion();

        pantalla.Mensaje(ganador == Sesion.equipo
            ? "¡Ganaste la partida! Hiciste tres en línea."
            : $"Perdiste la partida. El equipo {ganador} hizo tres en línea.");

        pantalla.MostrarBotonMenu();
    }

    private bool HayTresEnLinea(out Equipo ganador)
    {
        ganador = Equipo.Rojo;
        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, -1) };

        foreach (var c in enMesa)
        {
            if (c.Derrotada) continue;

            foreach (var d in dirs)
            {
                if (HayCartaEn(casillas[c] + d, c.datos.equipo) && HayCartaEn(casillas[c] + d * 2, c.datos.equipo))
                {
                    ganador = c.datos.equipo;
                    return true;
                }
            }
        }
        return false;
    }

    private bool HayCartaEn(Vector2Int cas, Equipo eq)
    {
        foreach (var c in enMesa)
        {
            if (!c.Derrotada && c.datos.equipo == eq && casillas[c] == cas) return true;
        }
        return false;
    }

    private Vector2Int Casilla(Vector3 pos)
    {
        var local = Quaternion.Inverse(refRot) * (pos - refPos); // Posicion respecto de la primera carta

        // En los image target el Y apunta para arriba, la mesa es el plano XZ
        int x = Mathf.RoundToInt(local.x / tamCasilla);
        int y = Mathf.RoundToInt(local.z / tamCasilla);

        return new Vector2Int(x, y);
    }

    private bool Adyacentes(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        if (dx == 0 && dy == 0) return false; // Misma casilla, no deberia pasar

        if (diagonalCuenta) return dx <= 1 && dy <= 1;

        return dx + dy == 1;
    }


    // Hay que rebisar que pasa si la primera carta esta torcida, la grilla queda torcida tambien.
    // Los dos celulares calculan el duelo por su cuenta, si uno ve una carta y el otro no se desincronizan. Por ahora confiamos en los jugadores.
}
