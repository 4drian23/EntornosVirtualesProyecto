using UnityEngine;

public class Gilus_Controler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag ("Player"))
        {
            Debug.Log("Entreo Jugaor");
        }   
    }

}
