using System.Collections;
using UnityEngine;

// Va en el objeto "Modelo" de cada carta. Adentro se instancia el prefab q diga la CartaData.
// Todas las animaciones mueven este objeto, asi da igual si adentro hay un cubo o un dragon.
public class Monstruo : MonoBehaviour
{
    [Header("Configuraciones generales")]
    [Tooltip("Grados por segundo q gira cuando esta quieto")]
    [SerializeField] float velGiro = 40f;

    [Tooltip("Cuanto tarda en aparecer")]
    [SerializeField] float tiempoAparecer = 0.4f;

    private GameObject modelo;
    private Light luz;
    private Vector3 posInicial;
    private bool quieto = true; // mientras es true gira solo

    void Awake()
    {
        posInicial = transform.localPosition;
        transform.localScale = Vector3.zero;

        // una luz chiquita para el brillo del combate, se prende solo cuando hace falta
        luz = new GameObject("Luz").AddComponent<Light>();
        luz.transform.SetParent(transform, false);
        luz.type = LightType.Point;
        luz.range = 0.2f;
        luz.enabled = false;
    }

    void Update()
    {
        if (quieto) transform.Rotate(0, velGiro * Time.deltaTime, 0);
    }

    public void Configurar(CartaData datos)
    {
        if (datos == null) return;

        luz.color = datos.equipo == Equipo.Rojo ? new Color(1f, 0.3f, 0.2f) : new Color(0.3f, 0.5f, 1f);

        if (datos.monstruo != null && modelo == null)
        {
            modelo = Instantiate(datos.monstruo, transform);
        }
    }

    public void Aparecer()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        quieto = true;
        transform.localPosition = posInicial;
        StartCoroutine(Escalar(Vector3.one, tiempoAparecer));
    }

    public void Esconder()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.zero;
        transform.localPosition = posInicial;
        quieto = true;
        luz.enabled = false;
        gameObject.SetActive(false);
    }

    // Se acerca al rival, "choca" y vuelve a su carta
    public IEnumerator Atacar(Vector3 haciaDonde)
    {
        quieto = false;

        Vector3 destino = transform.parent.InverseTransformPoint(haciaDonde);
        Vector3 medio = Vector3.Lerp(posInicial, destino, 0.6f); // no llega hasta el otro, se frena un poco antes

        yield return Mover(posInicial, medio, 0.5f);
        yield return Mover(medio, posInicial, 0.4f);

        quieto = true;
    }

    // Resplandor del ganador
    public IEnumerator Festejar()
    {
        quieto = false;
        luz.enabled = true;

        for (float t = 0; t < 1.2f; t += Time.deltaTime)
        {
            float p = Mathf.Sin(t * Mathf.PI * 2.5f) * 0.5f + 0.5f; // va y viene entre 0 y 1
            luz.intensity = p * 3f;
            transform.localScale = Vector3.one * (1f + p * 0.3f);
            transform.Rotate(0, 360f * Time.deltaTime, 0);
            yield return null;
        }

        luz.enabled = false;
        transform.localScale = Vector3.one;
        quieto = true;
    }

    // El perdedor se achica hasta desaparecer
    public IEnumerator Morir()
    {
        quieto = false;
        luz.enabled = true;
        luz.intensity = 2f;

        yield return Escalar(Vector3.zero, 0.8f);

        luz.enabled = false;
        gameObject.SetActive(false);
    }

    private IEnumerator Escalar(Vector3 hasta, float dur)
    {
        Vector3 desde = transform.localScale;

        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0, 1, t / dur));
            yield return null;
        }
        transform.localScale = hasta;
    }

    private IEnumerator Mover(Vector3 desde, Vector3 hasta, float dur)
    {
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            transform.localPosition = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0, 1, t / dur));
            yield return null;
        }
        transform.localPosition = hasta;
    }

    // Falta: particulas o algo mas magico cuando pelean. Con la luz y el giro zafa por ahora.
}
