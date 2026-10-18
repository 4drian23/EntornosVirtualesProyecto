using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Configuraciones generales")]
    [Tooltip("Distancia a la que detecta a un objeivo enemigo")]
    [SerializeField] float distanciaMinima = 0.1f; // La distancia se mide en metros, no en pixeles.

    [Tooltip("Informacion y Estadisticas")]
    //[SerializeField] float velocidadDeMovimiento = 0;
    [SerializeField] string nameID;
    [SerializeField] string infoTest;
    private string enemyInfo;

    [Tooltip("Referencias del Enemigo")]
    [SerializeField] GameObject enemigo; 
    //[SerializeField] MonsterController enemyScript;

    /*[Tooltip("Animator")]
    [SerializeField] Animator monsterAnimator;*/

    //Lo siguiente es para usar con un GameController. De momento usamos el Inspector.
    /*private MonsterController(string iD, string info)
    {
        this.nameID = iD;
        this.infoTest = info;
    }*/ 

    void Start()
    {
        
    }
    void Update()
    {
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



    // Hay que rebisar como se comporta el objeto al escanear varios enemigos en sus alrededores.
    // Por ahora los objetos no se desativan al desaparecer el target del lente. NO se porque
    // El objeto no deberia moverse de su posicion. Simpre moverse junto a la carta.
    // Pareciera que el intercambio de informacion solo se efectua una vez. NO se porque.
}
