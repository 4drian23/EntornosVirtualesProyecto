using UnityEngine;

public class Gilus_Controler : MonoBehaviour
{
    [Tooltip("RigidBody")]
    [SerializeField] Rigidbody2D rb;

    [Tooltip("Animator")]
    [SerializeField] Animator giliusAnimator;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag ("Player"))
        {
            Debug.Log("Guilius entro en contacto con Tyris");
            giliusAnimator.SetTrigger("defeat");
        }   
    }


    //void ResibirEmpuje( Vector3 dirYFue)
   // {
    //    rb.AddForce(dirYFue, ForceMode2D.Impulse);
    //}

}
