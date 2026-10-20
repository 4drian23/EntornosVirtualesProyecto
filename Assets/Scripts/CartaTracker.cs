using UnityEngine;
using Vuforia;

public class CartaTracker : MonoBehaviour
{
    [Header("Datos de la carta")]
    public CartaData datos;

    [Header("Entrada a la mesa")]
    [Tooltip("Segundos que tiene que estar quieta a la vista para darla por puesta")]
    [SerializeField] float quietud = 0.5f;

    [Header("Salida de la mesa")]
    [Tooltip("Segundos mirando el lugar vacio antes de darla por sacada")]
    [SerializeField] float gracia = 2.5f;

    [Tooltip("Segundos sin verla, mire donde mire la camara, antes de darla por sacada")]
    [SerializeField] float graciaMaxima = 12f;

    [Tooltip("Que tan adentro del encuadre tiene q estar el lugar para decir que la camara lo esta mirando (0 a 0.5)")]
    [SerializeField] float borde = 0.1f;

    [Header("Otros")]
    [Tooltip("Objeto donde se instancia el modelo 3D")]
    [SerializeField] Monstruo modelo;

    [Tooltip("Loguea cada vez que la camara pierde o recupera la carta")]
    [SerializeField] bool logDetallado = true;

    public bool EnMesa { get; private set; }
    public bool Derrotada { get; set; } // la maneja el GameManager
    public bool Visible => visible;     // la camara la esta viendo en este momento

    public Monstruo Modelo => modelo;

    private ObserverBehaviour observer;
    private Camera cam;

    private bool visible;
    private float perdidaDesde;
    private float mirandoSinVer;   // segundos que la camara miro el lugar y no la vio

    private bool candidata;        // la estamos viendo pero todavia no la dimos por puesta
    private Vector3 posCandidata;
    private float candidataDesde;

    void Start()
    {
        if (datos == null) Debug.LogError($"{name} no tiene CartaData asignada");

        // Vuforia nos avisa cada vez que cambia el estado del target
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += CambioStatus;

        cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

        if (modelo != null)
        {
            modelo.Configurar(datos);
            modelo.Esconder(); // hasta q la camara no vea la carta no se muestra nada
        }
    }

    void OnDestroy()
    {
        if (observer != null) observer.OnTargetStatusChanged -= CambioStatus;
    }

    void Update()
    {
        if (EnMesa) RevisarSiSeFue();
        else RevisarSiEntro();
    }

    private void CambioStatus(ObserverBehaviour ob, TargetStatus st)
    {
        bool antes = visible;

        // Solo TRACKED. Con EXTENDED_TRACKED vuforia se acuerda de la carta aunque la saques de la mesa
        visible = st.Status == Status.TRACKED;

        if (visible && !antes) mirandoSinVer = 0;
        else if (!visible && antes) perdidaDesde = Time.time;

        if (logDetallado && EnMesa && antes != visible)
        {
            Debug.Log(visible ? $"{datos.nombre}: la camara la volvio a ver" : $"{datos.nombre}: la camara la dejo de ver ({st.Status})");
        }
    }

    private void RevisarSiEntro()
    {
        if (!visible)
        {
            candidata = false;
            return;
        }

        // Si esta mucho mas alta que las demas la tienen en la mano, todavia no es una jugada
        if (GameManager.inst != null && !GameManager.inst.AlturaDeMesa(transform.position))
        {
            candidata = false;
            return;
        }

        // Tiene que quedarse quieta un rato. Si la estan moviendo no cuenta, y la primera pose que da vuforia es medio cualquiera
        if (!candidata || Vector3.Distance(transform.position, posCandidata) > 0.02f)
        {
            candidata = true;
            posCandidata = transform.position;
            candidataDesde = Time.time;
            return;
        }

        if (Time.time - candidataDesde < quietud) return;

        candidata = false;
        EnMesa = true;

        // el manager decide si es una jugada nueva o si la carta solo reaparecio (y si sigue derrotada)
        if (GameManager.inst != null) GameManager.inst.CartaEntro(this);

        if (modelo != null && !Derrotada) modelo.Aparecer();
    }

    private void RevisarSiSeFue()
    {
        if (visible) return;

        // Si la camara apunta al lugar de la mesa donde estaba y no la ve, contamos el tiempo. Si no apunta ahi, no sabemos nada
        Vector3 lugar = GameManager.inst != null ? GameManager.inst.PosEnMesa(this) : transform.position;
        if (EnCamara(lugar)) mirandoSinVer += Time.deltaTime;

        // Una derrotada solo se da por sacada mirando el lugar vacio. Si no, con mirar para otro lado un rato el juego seguia solo
        bool tope = !Derrotada && Time.time - perdidaDesde > graciaMaxima;

        if (mirandoSinVer > gracia || tope) Retirar(false);
    }

    // La saca de la mesa. enMano = la vimos moverse de su lugar, o sea que la estan levantando
    public void Retirar(bool enMano)
    {
        if (!EnMesa) return;

        EnMesa = false;
        candidata = false;
        mirandoSinVer = 0;

        if (modelo != null) modelo.Esconder();

        if (GameManager.inst != null) GameManager.inst.CartaSalio(this, enMano);
    }

    private bool EnCamara(Vector3 punto)
    {
        if (cam == null) return true;

        Vector3 v = cam.WorldToViewportPoint(punto);

        return v.z > 0 && v.x > borde && v.x < 1 - borde && v.y > borde && v.y < 1 - borde;
    }

    // Si la carta que salio se deja apoyada dentro del encuadre, vuelve a entrar como carta nueva. Hay que sacarla de la vista.
}
