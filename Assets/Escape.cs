using UnityEngine;

public class Escape : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Application.Quit();
        }
    }
}
