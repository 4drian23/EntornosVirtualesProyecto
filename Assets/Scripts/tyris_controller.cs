using System;
using Unity.VisualScripting;
using UnityEngine;

public class Tyris_controller : MonoBehaviour
{
    [Header("Configuracion de Ineracciones de Tyris")]
    [Tooltip("Distancia a la que persigue")]
    [SerializeField] float distanciaMinimaAlObjeto = 0.1f; //La distancia se mide en metros no en pixeles

    [Tooltip("Velocidad de Tyris")]
    [SerializeField] float velocidadDeMov = 0;

    [Tooltip("Referencia a Gilius")]
    [SerializeField] GameObject gilius;
    
    [Tooltip("Animator")]
    [SerializeField] Animator tyrisAnimator;
    
    private bool endAttack;

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
        if (gilius != null && gilius.activeInHierarchy && gilius.GetComponent<Gilius_Controler>().GiveState() == false) // Tenemos que revisar que el objeto este activo y no sea nulo.
        {
            var distancia = gilius.transform.position - transform.position; // Calcular vector
            //var distanciaUnity = Vector3.Distance(guilius.transform.position, transform.position); 

            
            //Debug.Log($"Distancia: {distancia}");

            if (distancia.magnitude > distanciaMinimaAlObjeto)
            {
                //Debug.Log($"If ejecutado");
                velocidadDeMov = 1;
                transform.position += distancia.normalized * velocidadDeMov * Time.deltaTime;
            }
            else
            {
                velocidadDeMov = 0;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Rival"))
        {
            Debug.Log("Tyris entro en contacto con Gilius");
            tyrisAnimator.SetTrigger("enemyClose");
            velocidadDeMov = 0;
        }   
    }
    
    void setendAttack()
    {
        endAttack = true;
        Debug.Log("variable seteada");
    }
    public bool terminoElAtaque()
    {
        return endAttack;
    }

}
