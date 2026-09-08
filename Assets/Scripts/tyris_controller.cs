using UnityEngine;

public class tyris_controller : MonoBehaviour
{
    [Header("Configuracion de Ineracciones de Tyris")]
    [Tooltip("Distancia a la que persigue")]
    [SerializeField] float distanciaMinimaAlObjeto = 300f;

    [Tooltip("Velocidad de Tyris")]
    [SerializeField] float velocidad = 50f;

    [Tooltip("Referencia a Guilius")]
    [SerializeField] GameObject guilius;
    

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (guilius != null && guilius.activeInHierarchy) // Tenemos que revisar que el objeto este activo y no sea nulo.
        {
            var distancia = guilius.transform.position - transform.position; // Calcular vector
            //var distanciaUnity = Vector3.Distance(guilius.transform.position, transform.position); 

            Debug.Log($"Distancia: {distancia}");

            if (distancia.magnitude > distanciaMinimaAlObjeto)
            {
                transform.position += distancia.normalized * velocidad * Time.deltaTime;
            } 
        }
    }
}
