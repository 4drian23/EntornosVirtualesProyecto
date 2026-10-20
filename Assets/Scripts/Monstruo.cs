using System.Collections;
using UnityEngine;

// Va en el objeto "Modelo" de cada carta. Adentro se instancia el prefab q diga la CartaData.
//
// ===== PARA CUANDO ESTEN LOS MODELOS 3D =====
// Hoy el prefab es un cubo (CuboRojo / CuboAzul en Assets/Prefabs). Para poner el modelo de verdad:
//  1. Armar el prefab del monstruo con escala en metros (la carta mide 6,3 cm, el cubo mide 3 cm) y
//     arrastrarlo al campo "monstruo" de la CartaData de esa carta. No hay que tocar codigo.
//  2. Si el prefab tiene un Animator, este script le dispara los triggers "Ataque", "Victoria" y
//     "Derrota" (los que existan). El idle lo hace el Animator solo, aca no se gira el modelo.
//     El clip de derrota tiene que durar mas o menos tiempoDerrota, pasado ese tiempo se esconde.
//  3. Lo marcado PROVISORIO es lo que hace que el cubo se vea vivo (giro, luz, achicarse al morir).
//     Con modelos animados sobra, pero tampoco rompe nada si queda.
// Todo lo demas mueve este objeto (el padre del modelo), asi que la flotacion, el avance del ataque y
// mirar al rival sirven para cualquier modelo que pongan adentro.
public class Monstruo : MonoBehaviour
{
    [Header("Idle")]
    [Tooltip("Grados por segundo que gira cuando esta quieto. PROVISORIO para el cubo, con un modelo con Animator no se usa")]
    [SerializeField] float velGiro = 40f;

    [Tooltip("Cuanto sube y baja flotando, en metros. 0 para que no flote")]
    [SerializeField] float flotacion = 0.004f;

    [Header("Tiempos")]
    [Tooltip("Cuanto tarda en aparecer")]
    [SerializeField] float tiempoAparecer = 0.4f;

    [Tooltip("Cuanto dura la derrota antes de esconderse. Si el modelo tiene clip de derrota, poner lo que dure el clip")]
    [SerializeField] float tiempoDerrota = 0.8f;

    private GameObject modelo;
    private Animator animator;   // el del prefab, si tiene
    private Light luz;           // PROVISORIO: brillo para que el cubo haga algo en la pelea
    private Vector3 posInicial;
    private bool quieto = true;  // mientras es true hace el idle
    private float reloj;         // para la flotacion

    // Las animaciones de pelea las corre el GameManager con su propio StartCoroutine, asi que StopAllCoroutines no las frena.
    // Cada Aparecer/Esconder suma uno, y la animacion que ve que el numero cambio se corta sola
    private int generacion;

    void Awake()
    {
        posInicial = transform.localPosition;
        transform.localScale = Vector3.zero;

        luz = new GameObject("Luz").AddComponent<Light>();
        luz.transform.SetParent(transform, false);
        luz.type = LightType.Point;
        luz.range = 0.2f;
        luz.enabled = false;
    }

    void Update()
    {
        if (!quieto) return;

        // idle: flota un poco. Vale para el cubo y para cualquier modelo
        reloj += Time.deltaTime;
        transform.localPosition = posInicial + Vector3.up * (Mathf.Sin(reloj * 2f) * flotacion);

        // PROVISORIO: el cubo gira para que se note que esta vivo. Un modelo animado hace su propio idle
        if (animator == null) transform.Rotate(0, velGiro * Time.deltaTime, 0);
    }

    public void Configurar(CartaData datos)
    {
        if (datos == null) return;

        luz.color = datos.equipo == Equipo.Rojo ? new Color(1f, 0.3f, 0.2f) : new Color(0.3f, 0.5f, 1f);

        if (datos.monstruo != null && modelo == null)
        {
            modelo = Instantiate(datos.monstruo, transform);
            animator = modelo.GetComponentInChildren<Animator>(); // null con el cubo, cuando pongan el modelo lo agarra solo
        }
    }

    public void Aparecer()
    {
        generacion++;
        gameObject.SetActive(true);
        StopAllCoroutines();
        quieto = true;
        luz.enabled = false;
        transform.localPosition = posInicial;
        transform.localRotation = Quaternion.identity; // que arranque mirando para donde mira la carta
        transform.localScale = Vector3.zero;
        StartCoroutine(Escalar(Vector3.one, tiempoAparecer, generacion));
    }

    public void Esconder()
    {
        generacion++;
        StopAllCoroutines();
        transform.localScale = Vector3.zero;
        transform.localPosition = posInicial;
        quieto = true;
        luz.enabled = false;
        gameObject.SetActive(false);
    }

    // Ataque: mira al rival, avanza hasta cerca y vuelve a su carta. Si el modelo tiene el trigger "Ataque" lo dispara a la vez
    public IEnumerator Atacar(Vector3 haciaDonde)
    {
        int g = generacion;
        quieto = false;

        Vector3 destino = transform.parent.InverseTransformPoint(haciaDonde);
        Quaternion rotAntes = transform.localRotation;
        MirarA(destino);
        Disparar("Ataque");

        Vector3 medio = Vector3.Lerp(posInicial, destino, 0.6f); // no llega hasta el otro, se frena un poco antes

        yield return Mover(posInicial, medio, 0.5f, g);
        yield return Mover(medio, posInicial, 0.4f, g);
        if (g != generacion) yield break;

        transform.localRotation = rotAntes;
        quieto = true;
    }

    // Victoria: trigger "Victoria" si existe. PROVISORIO: el cubo late y brilla
    public IEnumerator Festejar()
    {
        int g = generacion;
        quieto = false;
        Disparar("Victoria");
        luz.enabled = true;

        for (float t = 0; t < 1.2f; t += Time.deltaTime)
        {
            if (g != generacion) yield break;

            float p = Mathf.Sin(t * Mathf.PI * 2.5f) * 0.5f + 0.5f; // va y viene entre 0 y 1
            luz.intensity = p * 3f;

            if (animator == null)
            {
                transform.localScale = Vector3.one * (1f + p * 0.3f);
                transform.Rotate(0, 360f * Time.deltaTime, 0);
            }
            yield return null;
        }

        if (g != generacion) yield break;

        luz.enabled = false;
        transform.localScale = Vector3.one;
        quieto = true;
    }

    // Derrota: trigger "Derrota" si existe y espera que termine. PROVISORIO: el cubo se achica hasta desaparecer.
    // Despues de esto el GameManager espera a que saquen la carta de la mesa, aca solo se esconde el modelo
    public IEnumerator Morir()
    {
        int g = generacion;
        quieto = false;
        luz.enabled = true;
        luz.intensity = 2f;

        if (Disparar("Derrota"))
        {
            yield return new WaitForSeconds(tiempoDerrota);
        }
        else
        {
            yield return Escalar(Vector3.zero, tiempoDerrota, g);
        }

        if (g != generacion) yield break; // mientras moria la volvieron a jugar, no la escondemos

        luz.enabled = false;
        gameObject.SetActive(false);
    }

    // Gira el modelo para que quede de frente al rival (solo en el plano de la mesa)
    private void MirarA(Vector3 destinoLocal)
    {
        Vector3 dir = destinoLocal - posInicial;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.0001f) transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    // Dispara un trigger del Animator del modelo. Devuelve false si no hay Animator o no tiene ese trigger
    private bool Disparar(string trigger)
    {
        if (animator == null) return false;

        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == trigger)
            {
                animator.SetTrigger(trigger);
                return true;
            }
        }
        return false;
    }

    private IEnumerator Escalar(Vector3 hasta, float dur, int g)
    {
        Vector3 desde = transform.localScale;

        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (g != generacion) yield break;
            transform.localScale = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0, 1, t / dur));
            yield return null;
        }
        if (g == generacion) transform.localScale = hasta;
    }

    private IEnumerator Mover(Vector3 desde, Vector3 hasta, float dur, int g)
    {
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (g != generacion) yield break;
            transform.localPosition = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0, 1, t / dur));
            yield return null;
        }
        if (g == generacion) transform.localPosition = hasta;
    }
}
