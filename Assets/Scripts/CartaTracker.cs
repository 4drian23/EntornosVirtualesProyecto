using UnityEngine;
using Vuforia;

public class CartaTracker : MonoBehaviour
{
    [Header("Datos de la carta")]
    public CartaData datos;

    [Tooltip("Segundos que espera sin ver la carta antes de darla por sacada")]
    [SerializeField] float gracia = 1.5f; // En segundos.

    [Tooltip("Texto que flota arriba de la carta (por ahora es para debug)")]
    [SerializeField] TextMesh etiqueta;

    public bool EnMesa { get; private set; }

    private bool visible;
    private float perdidaDesde;

    void Start()
    {
        // Vuforia nos avisa cada vez que cambia el estado del target
        GetComponent<ObserverBehaviour>().OnTargetStatusChanged += CambioStatus;
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
            GameManager.inst.CartaEntro(this);
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
            GameManager.inst.CartaSalio(this);
        }
    }

    public void MostrarTexto(string txt)
    {
        if (etiqueta != null) etiqueta.text = txt;
    }

    // Falta ver que pasa si la carta se saca y se vuelve a poner en otro lado rapido.
}
