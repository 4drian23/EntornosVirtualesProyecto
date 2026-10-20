using UnityEngine;
using Vuforia;

public class CartaTracker : MonoBehaviour
{
    [Header("Datos de la carta")]
    public CartaData datos;

    [Tooltip("Segundos que espera sin ver la carta antes de darla por sacada")]
    [SerializeField] float gracia = 1.5f; // En segundos.

    [Tooltip("Objeto donde se instancia el modelo 3D")]
    [SerializeField] Monstruo modelo;

    public bool EnMesa { get; private set; }
    public bool Derrotada { get; set; } // la maneja el GameManager

    public Monstruo Modelo => modelo;

    private ObserverBehaviour observer;
    private bool visible;
    private float perdidaDesde;

    void Start()
    {
        if (datos == null) Debug.LogError($"{name} no tiene CartaData asignada");

        // Vuforia nos avisa cada vez que cambia el estado del target
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += CambioStatus;

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
        RevisarSiSeFue();
    }

    private void CambioStatus(ObserverBehaviour ob, TargetStatus st)
    {
        // Solo TRACKED. Con EXTENDED_TRACKED vuforia se acuerda de la carta aunque la saques de la mesa
        visible = st.Status == Status.TRACKED;

        if (visible && !EnMesa)
        {
            EnMesa = true;

            // el manager decide si es una jugada nueva o si la carta solo reaparecio (y si sigue derrotada)
            if (GameManager.inst != null) GameManager.inst.CartaEntro(this);

            if (modelo != null && !Derrotada) modelo.Aparecer();
        }
        else if (!visible)
        {
            perdidaDesde = Time.time; // Guardamos desde cuando no la vemos
        }
    }

    private void RevisarSiSeFue()
    {
        // Si la tapas con la mano un segundo no cuenta, tiene que pasar la gracia
        if (EnMesa && !visible && Time.time - perdidaDesde > gracia)
        {
            EnMesa = false;

            if (modelo != null) modelo.Esconder();

            if (GameManager.inst != null) GameManager.inst.CartaSalio(this);
        }
    }
}
