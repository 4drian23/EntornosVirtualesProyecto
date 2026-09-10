using System;
using UnityEngine;

public class tyris_controller : MonoBehaviour
{
    [Header("Configuracion de Ineracciones de Tyris")]
    [Tooltip("Distancia a la que persigue")]
    [SerializeField] float distanciaMinimaAlObjeto = 0.1f; //La distancia se mide en metros no en pixeles

    [Tooltip("Velocidad de Tyris")]
    [SerializeField] float velocidadDeMov = 1;

    [Tooltip("Referencia a Guilius")]
    [SerializeField] GameObject guilius;
    
    [Tooltip("Animator")]
    [SerializeField] Animator tyrisAnimator;


    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TargetEnemy();
        ReproducirIdle();
    }

    void ReproducirIdle()
    {
        tyrisAnimator.SetFloat("movement", velocidadDeMov);
    }

    void TargetEnemy()
    {
        if (guilius != null && guilius.activeInHierarchy) // Tenemos que revisar que el objeto este activo y no sea nulo.
        {
            var distancia = guilius.transform.position - transform.position; // Calcular vector
            //var distanciaUnity = Vector3.Distance(guilius.transform.position, transform.position); 

            //Debug.Log($"Distancia: {distancia}");

            if (distancia.magnitude > distanciaMinimaAlObjeto)
            {
                Debug.Log($"If ejecutado");

                transform.position += distancia.normalized * velocidadDeMov * Time.deltaTime;
            } 
        }
    }
}
