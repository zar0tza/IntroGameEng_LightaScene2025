using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;

public class Trigger1Play : MonoBehaviour
{
    public PlayableDirector timeline;
    public Animator animator;
   

    void Start()
    {
        timeline.GetComponent<PlayableDirector>().enabled = false;
        animator.GetComponent<Animator>().enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
         timeline.Play();
         timeline.GetComponent<PlayableDirector>().enabled = true;
         animator.GetComponent<Animator>().enabled = true;
         Destroy(gameObject, 3f);
    }    
}