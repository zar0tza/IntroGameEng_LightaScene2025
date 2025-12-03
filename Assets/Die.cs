using UnityEngine;

public class Die : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Application.Quit();
        }
    }
}
