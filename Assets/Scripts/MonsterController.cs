using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SpawnEvent : UnityEvent<GameObject> { }

public class MonsterController : MonoBehaviour
{
    [Header("Configuraciones generales")]
    [Tooltip("Distancia a la que detecta a un objeivo enemigo")]
    [SerializeField] float distanciaMinima = 0.1f; // La distancia se mide en metros, no en pixeles.

    [Tooltip("Información del Mosntruo")]
    [SerializeField] string nameID;
    [SerializeField] int atk;
    [SerializeField] Sprite cartaSprite;
    [SerializeField] string infoTest;

    [Tooltip("Información del Enemigo")]
    [SerializeField] GameObject enemigo; 

    private string enemyInfo;
    private bool estaActivo;
    public SpawnEvent spawneaoElObjeto;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //[SerializeField] MonsterController enemyScript;

    /*[Tooltip("Animator")]
    [SerializeField] Animator monsterAnimator;*/

    //Lo siguiente es para usar con un GameController. De momento usamos el Inspector.
    /*private MonsterController(string iD, string info)
    {
        this.nameID = iD;
        this.infoTest = info;
    }*/
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        (string nameID, int atk, Sprite cartaSprite) = EntregarDatos();
    }
    void Update()
    {
        if (this.gameObject.activeInHierarchy) //Si esta activo, envia la señal al manager junto a la referencia de su GameObject;
        {
            estaActivo = true;
            spawneaoElObjeto?.Invoke(this.gameObject);
        }

        DetectarEnemigo();
    }

    private void DetectarEnemigo()
    {
        if (enemigo != null && enemigo.activeInHierarchy) // Revisamos que el enemigo esta activo y no es nulo.
        {
            var distancia = enemigo.transform.position - this.transform.position; // Se calcula el vector entre ambos

            //Debug.Log($"{nameID} esta a: {distancia}mts de distancia");

            if (distancia.magnitude < distanciaMinima)
            {
                RecibirInfo(enemigo.GetComponent<MonsterController>().DarInfo());
            }
        }
    }

    string DarInfo()
    {
        return infoTest;
    }

    private void RecibirInfo(string newEnemyInfo)
    {
        enemyInfo = newEnemyInfo;

        Debug.Log($"{nameID} recibio esta información: {enemyInfo}");
    }

    public (string nombre, int ataque, Sprite imagen) EntregarDatos()
    {
        return(nameID, atk, cartaSprite);
    }
}
