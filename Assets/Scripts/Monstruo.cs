using System.Collections;
using UnityEngine;

// Va en el objeto "Modelo" de cada carta. Adentro se instancia el prefab q diga la CartaData.
// Si el prefab tiene Animator le dispara los triggers "Ataque", "Victoria" y "Derrota" (los estados se llaman igual)
// y espera lo que dure cada clip. Si no tiene (los cubos), hace una animacion simple con el transform.
// Todo lo demas (flotacion, avance del ataque, mirar al rival) mueve este objeto, el padre del modelo, asi que sirve para cualquiera.
public class Monstruo : MonoBehaviour
{
    [Header("Idle")]
    [Tooltip("Grados por segundo que gira cuando esta quieto. Solo para modelos sin Animator")]
    [SerializeField] float velGiro = 40f;

    [Tooltip("Cuanto sube y baja flotando, en metros. 0 para que no flote")]
    [SerializeField] float flotacion = 0.004f;

    [Header("Tiempos (con Animator se usa lo que dure cada clip)")]
    [Tooltip("Cuanto tarda en aparecer")]
    [SerializeField] float tiempoAparecer = 0.4f;

    [Tooltip("Cuanto dura el ataque sin Animator (ir y volver)")]
    [SerializeField] float tiempoAtaque = 0.9f;

    [Tooltip("Cuanto dura la derrota sin Animator antes de esconderse")]
    [SerializeField] float tiempoDerrota = 0.8f;

    private GameObject modelo;
    private Animator animator;   // el del prefab, si tiene
    private Light luz;           // brillo para la pelea
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

        // idle: flota un poco
        reloj += Time.deltaTime;
        transform.localPosition = posInicial + Vector3.up * (Mathf.Sin(reloj * 2f) * flotacion);

        // sin Animator gira para que se note que esta vivo
        if (animator == null) transform.Rotate(0, velGiro * Time.deltaTime, 0);
    }

    public void Configurar(CartaData datos)
    {
        if (datos == null) return;

        luz.color = datos.equipo == Equipo.Rojo ? new Color(1f, 0.3f, 0.2f) : new Color(0.3f, 0.5f, 1f);

        if (datos.monstruo != null && modelo == null)
        {
            modelo = Instantiate(datos.monstruo, transform);
            animator = modelo.GetComponentInChildren<Animator>(); // null si el prefab no tiene
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

        if (animator != null) animator.Rebind(); // si quedo en la pose de derrota de la ultima vez, que vuelva al idle

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

    // Ataque: mira al rival, avanza hasta cerca y vuelve a su carta. Con Animator el ir y volver dura lo que el clip
    public IEnumerator Atacar(Vector3 haciaDonde)
    {
        int g = generacion;
        quieto = false;

        Vector3 destino = transform.parent.InverseTransformPoint(haciaDonde);
        Quaternion rotAntes = transform.localRotation;
        MirarA(destino);

        float dur = tiempoAtaque;
        if (Disparar("Ataque"))
        {
            yield return LargoDelClip("Ataque", valor => dur = valor);
            if (g != generacion) yield break;
        }

        Vector3 medio = Vector3.Lerp(posInicial, destino, 0.6f); // no llega hasta el otro, se frena un poco antes

        yield return Mover(posInicial, medio, dur * 0.55f, g);
        yield return Mover(medio, posInicial, dur * 0.45f, g);
        if (g != generacion) yield break;

        transform.localRotation = rotAntes;
        quieto = true;
    }

    // Victoria: brilla lo que dure el clip. Sin Animator ademas late y gira
    public IEnumerator Festejar()
    {
        int g = generacion;
        quieto = false;
        luz.enabled = true;

        float dur = 1.2f;
        if (Disparar("Victoria"))
        {
            yield return LargoDelClip("Victoria", valor => dur = valor);
            if (g != generacion) yield break;
        }

        for (float t = 0; t < dur; t += Time.deltaTime)
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

    // Derrota: espera que termine el clip (sin Animator se achica hasta desaparecer) y se esconde.
    // Despues el GameManager espera a que saquen la carta de la mesa
    public IEnumerator Morir()
    {
        int g = generacion;
        quieto = false;
        luz.enabled = true;
        luz.intensity = 2f;

        if (Disparar("Derrota"))
        {
            float dur = tiempoDerrota;
            yield return LargoDelClip("Derrota", valor => dur = valor);
            if (g != generacion) yield break;
            yield return new WaitForSeconds(dur);
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

    // Dispara un trigger del Animator. Devuelve false si no hay Animator, esta escondido o no tiene ese trigger
    private bool Disparar(string trigger)
    {
        if (animator == null || !animator.isActiveAndEnabled) return false;

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

    // Despues de disparar un trigger el Animator tarda un par de frames en cambiar de estado, recien ahi se sabe cuanto dura el clip.
    // Si en 10 frames no llego al estado deja el tiempo por defecto
    private IEnumerator LargoDelClip(string estado, System.Action<float> listo)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return null;

            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(estado))
            {
                if (info.length > 0.05f) listo(info.length);
                yield break;
            }
        }
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
