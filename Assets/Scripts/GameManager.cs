using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    public enum Estado { Esperando, Eligiendo, Peleando, EsperandoRetiro, Terminado }

    [Header("Configuraciones generales")]
    [Tooltip("Tamaño de la carta en metros (ancho, alto). Si queda en 0 lo toma del image target")]
    [SerializeField] Vector2 tamCarta = Vector2.zero;

    [Tooltip("Cuanto espacio puede quedar entre dos cartas para que sigan siendo vecinas, en metros")]
    [SerializeField] float margen = 0.025f;

    [Tooltip("Si las diagonales cuentan como vecinas para pelear (para el tres en linea cuentan siempre)")]
    [SerializeField] bool diagonalCuenta = false;

    [Tooltip("Cuanto se tiene q correr una carta para q cuente como jugada (en anchos de carta)")]
    [SerializeField] float umbralMovimiento = 0.9f;

    [Header("Referencias")]
    [SerializeField] PantallaJuego pantalla;

    public Estado estado { get; private set; } = Estado.Esperando;

    private List<CartaTracker> enMesa = new List<CartaTracker>();
    private Dictionary<CartaTracker, Pose> asentada = new Dictionary<CartaTracker, Pose>();        // posicion y rotacion "firme" de cada carta
    private Dictionary<CartaTracker, Vector3> ultimaPos = new Dictionary<CartaTracker, Vector3>();  // donde estaba cada carta cuando la camara la perdio
    private Dictionary<CartaTracker, int> salioJugada = new Dictionary<CartaTracker, int>();        // y cuantas jugadas iban en ese momento
    private Dictionary<CartaTracker, Vector3> posMovida = new Dictionary<CartaTracker, Vector3>();  // donde esta parada una carta que se corrio de su lugar
    private Dictionary<CartaTracker, float> movidaDesde = new Dictionary<CartaTracker, float>();    // y desde cuando esta quieta ahi

    private HUD hud; // el de Adrian, lo busca solo en la escena

    private CartaTracker ultimaJugada; // la carta q se puso o se movio por ultima vez, es la q ataca
    private int nJugadas;              // se suma cada vez que alguien pone o mueve una carta
    private bool hayCambios;           // para no revisar duelos y tres en linea en cada frame si no paso nada

    private CartaTracker atacanteEligiendo;      // mientras esta abierto el cartel de eleccion
    private List<CartaTracker> rivalesEligiendo;

    private string resultadoDuelo;
    private string faltanSacar = "";

    void Awake()
    {
        inst = this;
    }

    void Start()
    {
        hud = FindAnyObjectByType<HUD>();

        if (tamCarta == Vector2.zero)
        {
            var target = FindAnyObjectByType<ImageTargetBehaviour>();
            tamCarta = target != null ? target.GetSize() : new Vector2(0.063f, 0.088f);
        }

        if (!Sesion.eligio) Debug.LogWarning("Entraste sin pasar por el menu, se juega como equipo Rojo");

        pantalla.Mensaje($"Jugás con el equipo {Sesion.equipo}. Apuntá a las cartas sobre la mesa.");
    }

    void Update()
    {
        ActualizarPosiciones();

        if (estado == Estado.Esperando && hayCambios)
        {
            hayCambios = false;
            BuscarDuelo();
        }
        else if (estado == Estado.EsperandoRetiro)
        {
            RevisarRetiro();
        }
    }

    public void CartaEntro(CartaTracker c)
    {
        if (!enMesa.Contains(c)) enMesa.Add(c);

        Asentar(c);

        // Si vuelve al mismo lugar es que la camara la perdio nomas, no es una jugada nueva.
        // Salvo una derrotada que vuelve despues de que alguien jugo: esa la sacaron y la volvieron a jugar
        bool mismoLugar = ultimaPos.TryGetValue(c, out var antes) && Vector3.Distance(antes, c.transform.position) < tamCarta.x * 0.9f;
        bool reaparecio = mismoLugar && (!c.Derrotada || salioJugada[c] == nJugadas);
        ultimaPos.Remove(c);
        salioJugada.Remove(c);
        hayCambios = true;

        if (!reaparecio)
        {
            c.Derrotada = false; // vuelve a jugar como si nada, perder solo la saca de la mesa
            Jugada(c);
        }
        else if (c.Derrotada && estado == Estado.Esperando)
        {
            // la perdedora sigue en la mesa, volvemos a esperar que la saquen
            estado = Estado.EsperandoRetiro;
        }

        if (hud != null && !c.Derrotada) hud.RecibirCartaNueva(c.datos);

        Debug.Log($"Entro {c.datos.nombre} ({(reaparecio ? "reaparecio" : "jugada nueva")}{(c.Derrotada ? ", derrotada" : "")}) - estado {estado}");
    }

    // enMano = la vimos levantarla. Si no, la camara la perdio nomas y puede que siga ahi
    public void CartaSalio(CartaTracker c, bool enMano)
    {
        if (!enMano && asentada.TryGetValue(c, out var p))
        {
            ultimaPos[c] = p.position;
            salioJugada[c] = nJugadas;
        }

        enMesa.Remove(c);
        asentada.Remove(c);
        movidaDesde.Remove(c);
        posMovida.Remove(c);

        // Si la camara la perdio nomas, sigue siendo la ultima jugada: cuando vuelva se retoma el ataque pendiente
        if (enMano && ultimaJugada == c) ultimaJugada = null;

        if (hud != null) hud.QuitarCarta(c.datos);

        Debug.Log($"Salio {c.datos.nombre}{(c.Derrotada ? " (derrotada)" : "")}{(enMano ? " (en mano)" : "")} - estado {estado}");

        // Si levantaron una de las cartas del cartel de eleccion, lo cerramos y se vuelve a armar con las que quedan
        if (estado == Estado.Eligiendo && enMano && (c == atacanteEligiendo || rivalesEligiendo.Contains(c)))
        {
            pantalla.OcultarEleccion();
            CancelarEleccion("Se cerró la elección, sacaron una de las cartas de la mesa.");
        }
    }

    private void Jugada(CartaTracker c)
    {
        ultimaJugada = c;
        nJugadas++;
    }

    private void Asentar(CartaTracker c)
    {
        asentada[c] = new Pose(c.transform.position, c.transform.rotation);
        movidaDesde.Remove(c);
        posMovida.Remove(c);
    }

    // Donde esta la carta en la mesa (la posicion firme, no la del tracking de este frame)
    public Vector3 PosEnMesa(CartaTracker c)
    {
        return asentada.TryGetValue(c, out var p) ? p.position : c.transform.position;
    }

    // Una carta que esta mucho mas alta que las demas no esta apoyada, la tienen en la mano
    public bool AlturaDeMesa(Vector3 pos)
    {
        foreach (var c in enMesa)
        {
            if (Mathf.Abs(pos.y - asentada[c].position.y) < tamCarta.x) return true;
        }
        return enMesa.Count == 0; // sin cartas no hay con que comparar
    }

    private void ActualizarPosiciones()
    {
        CartaTracker sacada = null;

        foreach (var c in enMesa)
        {
            // Solo miramos la pose si la camara la esta viendo, si no es vieja
            if (!c.Visible)
            {
                movidaDesde.Remove(c);
                continue;
            }

            float corrida = Vector3.Distance(c.transform.position, asentada[c].position);

            // Una derrotada no tiene por que moverse. Si se movio es que la estan sacando
            if (c.Derrotada)
            {
                if (corrida > tamCarta.x * 1.5f) sacada = c;
                continue;
            }

            if (corrida < tamCarta.x * umbralMovimiento)
            {
                movidaDesde.Remove(c);
                continue;
            }

            // Tiene que quedarse quieta medio segundo en el lugar nuevo. Si la llevan en la mano no cuenta
            if (!movidaDesde.ContainsKey(c) || Vector3.Distance(c.transform.position, posMovida[c]) > 0.02f)
            {
                movidaDesde[c] = Time.time;
                posMovida[c] = c.transform.position;
            }
            else if (Time.time - movidaDesde[c] > 0.5f)
            {
                Asentar(c);
                Jugada(c);
                hayCambios = true;
                Debug.Log($"{c.datos.nombre} se movio");
            }
        }

        if (sacada != null)
        {
            Debug.Log($"{sacada.datos.nombre} se movio de su lugar, la estan sacando");
            sacada.Retirar(true); // esto llama a CartaSalio
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

            if (Vecinas(ultimaJugada, otra)) rivales.Add(otra);
        }

        Debug.Log($"{ultimaJugada.datos.nombre} jugo. Enemigas vecinas: {(rivales.Count == 0 ? "ninguna" : Nombres(rivales))}");

        if (rivales.Count == 0) return;

        var atacante = ultimaJugada;
        ultimaJugada = null; // para q no vuelva a disparar el mismo duelo

        if (rivales.Count == 1)
        {
            StartCoroutine(Pelea(atacante, rivales[0]));
        }
        else
        {
            estado = Estado.Eligiendo;
            atacanteEligiendo = atacante;
            rivalesEligiendo = rivales;
            MostrarCartelEleccion();
        }
    }

    private void MostrarCartelEleccion()
    {
        var atacante = atacanteEligiendo;
        var rivales = rivalesEligiendo;

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
            if (estado != Estado.Eligiendo) return; // ya se cancelo por otro lado

            var rival = idx < rivales.Count ? rivales[idx] : null;

            if (!enMesa.Contains(atacante))
            {
                // sin atacante no hay duelo. Si solo la perdio la camara, al volver se rearma solo
                CancelarEleccion("Se canceló el duelo, la carta que atacaba no está en la mesa.");
            }
            else if (rival == null || !enMesa.Contains(rival))
            {
                // la camara no esta viendo a la elegida, que apunten bien y elijan de nuevo
                MostrarCartelEleccion();
                pantalla.Mensaje("No veo esa carta. Apuntá la cámara a la mesa y volvé a elegir.");
            }
            else
            {
                atacanteEligiendo = null;
                rivalesEligiendo = null;
                StartCoroutine(Pelea(atacante, rival));
            }
        });
    }

    // Vuelve a Esperando dejando al atacante como ultima jugada (si no hubo otra despues), asi busca rivales entre las que sigan en la mesa
    private void CancelarEleccion(string msg)
    {
        estado = Estado.Esperando;

        if (ultimaJugada == null) ultimaJugada = atacanteEligiendo;

        atacanteEligiendo = null;
        rivalesEligiendo = null;
        hayCambios = true;

        pantalla.Mensaje(msg);
        Debug.Log(msg);
    }

    private IEnumerator Pelea(CartaTracker a, CartaTracker b)
    {
        estado = Estado.Peleando;
        pantalla.Mensaje($"¡{a.datos.nombre} ({a.datos.puntos}) vs {b.datos.nombre} ({b.datos.puntos})!");
        Debug.Log($"Duelo: {a.datos.nombre} ({a.datos.puntos}) vs {b.datos.nombre} ({b.datos.puntos})");

        yield return new WaitForSeconds(0.8f);

        // los dos se tiran contra el otro a la vez
        var ataqueA = StartCoroutine(a.Modelo.Atacar(b.Modelo.transform.position));
        yield return b.Modelo.Atacar(a.Modelo.transform.position);
        yield return ataqueA;

        if (a.datos.puntos == b.datos.puntos)
        {
            a.Derrotada = true;
            b.Derrotada = true;
            if (hud != null) { hud.QuitarCarta(a.datos); hud.QuitarCarta(b.datos); }

            StartCoroutine(a.Modelo.Morir());
            yield return b.Modelo.Morir();

            resultadoDuelo = $"Empate. {a.datos.nombre} y {b.datos.nombre} salen de la mesa.";
            Debug.Log($"Empate entre {a.datos.nombre} y {b.datos.nombre}");
        }
        else
        {
            var gana = a.datos.puntos > b.datos.puntos ? a : b;
            var pierde = gana == a ? b : a;

            pierde.Derrotada = true;
            if (hud != null) hud.QuitarCarta(pierde.datos);

            StartCoroutine(gana.Modelo.Festejar());
            yield return pierde.Modelo.Morir();

            if (gana.datos.equipo == Sesion.equipo)
                resultadoDuelo = $"¡Victoria! {gana.datos.nombre} venció a {pierde.datos.nombre}. Mantené tu posición en la mesa.";
            else
                resultadoDuelo = $"Derrota. {pierde.datos.nombre} fue vencida por {gana.datos.nombre}. Sacá tu carta de la mesa para seguir.";

            Debug.Log($"Gano {gana.datos.nombre}, {pierde.datos.nombre} tiene que salir de la mesa");
        }

        // Las que pelearon ya jugaron, que no vuelvan a atacar solas
        if (ultimaJugada == a || ultimaJugada == b) ultimaJugada = null;

        pantalla.Mensaje(resultadoDuelo);
        yield return new WaitForSeconds(2f); // que se llegue a leer el resultado

        faltanSacar = "";
        estado = Estado.EsperandoRetiro;
    }

    private void RevisarRetiro()
    {
        string faltan = "";

        foreach (var c in enMesa)
        {
            if (c.Derrotada) faltan += (faltan == "" ? "" : ", ") + c.datos.nombre;
        }

        if (faltan != "")
        {
            // solo actualizamos el cartel si cambio la lista
            if (faltan != faltanSacar)
            {
                faltanSacar = faltan;
                pantalla.Mensaje($"{resultadoDuelo}\nFalta sacar de la mesa (fuera de la cámara): {faltan}");
                Debug.Log($"Esperando que saquen de la mesa: {faltan}");
            }
            return;
        }

        faltanSacar = "";
        estado = Estado.Esperando;
        hayCambios = true; // por si pusieron otra carta mientras esperabamos
        pantalla.Mensaje("Listo. Sigue el juego.");
        Debug.Log("Retiro completo, sigue el juego");
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

        foreach (var a in enMesa)
        {
            if (a.Derrotada) continue;

            foreach (var b in enMesa)
            {
                if (b == a || b.Derrotada || b.datos.equipo != a.datos.equipo) continue;

                int dir = Direccion(a, b);
                if (dir == 0) continue;

                foreach (var c in enMesa)
                {
                    if (c == a || c == b || c.Derrotada || c.datos.equipo != a.datos.equipo) continue;

                    // de a a b y de b a c tienen q ir para el mismo lado, si no es una L. Los dos pares medidos desde a, por si b esta girada
                    if (Direccion(b, c, asentada[a].rotation) == dir)
                    {
                        ganador = a.datos.equipo;
                        Debug.Log($"Tres en linea de {ganador}: {a.datos.nombre}, {b.datos.nombre}, {c.datos.nombre}");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool Vecinas(CartaTracker a, CartaTracker b)
    {
        int dir = Direccion(a, b);
        return (dir >= 1 && dir <= 4) || (diagonalCuenta && dir >= 5);
    }

    // Donde esta b vista desde la carta a. 0 = no es vecina, 1 a 4 = al costado (derecha, izquierda, arriba, abajo), 5 a 8 = en diagonal.
    // Por defecto se mide con la orientacion de a, se puede pasar otra para comparar varios pares con el mismo marco
    private int Direccion(CartaTracker a, CartaTracker b, Quaternion? marco = null)
    {
        var pa = asentada[a];
        var pb = asentada[b];

        Vector3 d = Quaternion.Inverse(marco ?? pa.rotation) * (pb.position - pa.position); // x es hacia el costado, z hacia arriba/abajo de la carta, y es altura
        float dx = Mathf.Abs(d.x);
        float dz = Mathf.Abs(d.z);

        if (Mathf.Abs(d.y) > tamCarta.x) return 0; // no esta apoyada en la mesa, la llevan en la mano

        bool pegadaX = dx <= tamCarta.x + margen;
        bool pegadaZ = dz <= tamCarta.y + margen;
        bool mismaColumna = dx < tamCarta.x * 0.5f;
        bool mismaFila = dz < tamCarta.y * 0.5f;

        if (mismaColumna && mismaFila) return 0; // encimadas, no deberia pasar

        if (pegadaX && mismaFila) return d.x > 0 ? 1 : 2;
        if (pegadaZ && mismaColumna) return d.z > 0 ? 3 : 4;
        if (pegadaX && pegadaZ) return d.x > 0 ? (d.z > 0 ? 5 : 6) : (d.z > 0 ? 7 : 8);

        return 0;
    }

    private string Nombres(List<CartaTracker> cartas)
    {
        string s = "";
        foreach (var c in cartas) s += (s == "" ? "" : ", ") + c.datos.nombre;
        return s;
    }


    // Los dos celulares calculan el duelo por su cuenta, si uno ve una carta y el otro no se desincronizan. Por ahora confiamos en los jugadores.
}
