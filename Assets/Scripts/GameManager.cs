using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [Header("Configuraciones generales")]
    [Tooltip("Ancho de la carta mas un margen")]
    [SerializeField] float tamCasilla = 0.07f; // En metros, no en pixeles.

    [Tooltip("Si las diagonales cuentan como adyacentes")]
    [SerializeField] bool diagonalCuenta = false;

    private List<CartaTracker> enMesa = new List<CartaTracker>();
    private Dictionary<CartaTracker, Vector2Int> casillas = new Dictionary<CartaTracker, Vector2Int>();

    // La primera carta que aparece marca donde esta el "arriba" de la grilla
    private bool hayRef;
    private Vector3 refPos;
    private Quaternion refRot;

    void Awake()
    {
        inst = this;
    }

    void Update()
    {
        if (!hayRef) return;

        ActualizarCasillas();
        MostrarDebug();
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

        Debug.Log($"Entro {c.datos.nombre}");
    }

    public void CartaSalio(CartaTracker c)
    {
        enMesa.Remove(c);
        casillas.Remove(c);

        Debug.Log($"Salio {c.datos.nombre}");
    }

    private void ActualizarCasillas()
    {
        foreach (var c in enMesa)
        {
            casillas[c] = Casilla(c.transform.position);
        }
    }

    private void MostrarDebug()
    {
        foreach (var c in enMesa)
        {
            var cas = casillas[c];
            string txt = $"{c.datos.nombre} [{cas.x},{cas.y}] {c.datos.puntos}pts";

            foreach (var otra in enMesa)
            {
                if (otra == c || otra.datos.equipo == c.datos.equipo) continue; // Las del mismo equipo no pelean

                if (Adyacentes(cas, casillas[otra]))
                {
                    txt += "\nvs " + otra.datos.nombre;
                }
            }

            c.MostrarTexto(txt);
        }
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


    // Aca despues va el combate, los turnos y la victoria cuando esten las reglas.
    // Hay que rebisar que pasa si la primera carta esta torcida, la grilla queda torcida tambien.
}
