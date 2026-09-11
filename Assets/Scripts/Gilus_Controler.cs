using UnityEngine;

public class Gilus_Controler : MonoBehaviour
{
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

}
