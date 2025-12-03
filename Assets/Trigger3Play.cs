using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;

public class Trigger3Play : MonoBehaviour
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
        timeline.GetComponent<PlayableDirector>().enabled = true;
        animator.GetComponent<Animator>().enabled = true;
        timeline.Play();
    }
}
