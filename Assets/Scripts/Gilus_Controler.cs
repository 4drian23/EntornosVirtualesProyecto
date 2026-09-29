using System;
using Unity.VisualScripting;
using UnityEngine;

public class Gilius_Controler : MonoBehaviour
{
    [Tooltip("RigidBody")]
    [SerializeField] Rigidbody rb;

    [Tooltip("Animator")]
    [SerializeField] Animator giliusAnimator;

    [Tooltip("Otros")]
    [SerializeField] float PushStr = 0f;

    private GameObject tyris;
    private bool finish;

    bool IsDefeated = false;

    void Update()
    {
        if (tyris != null)
        {
            Debug.Log($"confirmar estado boleano: {tyris.GetComponent<Tyris_controller>().terminoElAtaque()}");
            if (tyris.GetComponent<Tyris_controller>().terminoElAtaque() && !finish)
            {
                Debug.Log("Hola funciona");
                PushDirection(tyris.GetComponent<Collider>());
                DefeatAction(); 
            }
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Player"))
        {
            Debug.Log("Gilius entro en contacto con Tyris");

            tyris = other.gameObject;

            Debug.Log($"confirmar valor: {tyris}");
           
        }   
    }

    void DefeatAction()
    {
        giliusAnimator.SetTrigger("defeat");
        IsDefeated = true;
    }

    void PushDirection(Collider attacker)
    {
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //Direccion del atacante hacia Gilius

        rb.AddForce(dir * PushStr, ForceMode.Impulse); // Se aplica fuerza de empuje

        Debug.Log("Fue empujado");

        finish = true;
    }

    public bool GiveState()
    {
        return IsDefeated;
    }

}
