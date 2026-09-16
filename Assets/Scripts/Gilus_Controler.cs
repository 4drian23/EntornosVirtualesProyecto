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

    bool IsDefeated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Player"))
        {
            Debug.Log("Gilius entro en contacto con Tyris");
            PushDirection(other);
            DefeatAction();
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
    }

    public bool GiveState()
    {
        return IsDefeated;
    }

}
