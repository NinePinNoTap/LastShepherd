/*
 * Instructions :-
 * - Attach this script an object (cube will do)
 * - Define the audio clip to play
 * - Script will do the rest
 */

using UnityEngine;
using System.Collections;

public class AudioTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip audioClip;          // Sound to play

    [Header("Properties")]
    public float playCount = 1;
    public float playVolume = 1.0f;
    private bool isActivated;
    private float totalPlays;

    private BoxCollider boxCollider;
    private AudioSource audioSource;

	
	void Start ()
    {
        // Turn off mesh renderer
        GetComponent<MeshRenderer>().enabled = false;

        // Ensure we have correct components
        boxCollider = Utility.HandleComponent<BoxCollider>(gameObject);
        audioSource = Utility.HandleComponent<AudioSource>(gameObject);

        // Ensure components have correct properties
        boxCollider.isTrigger = true;
        audioSource.volume = playVolume;

        // Set properties
        totalPlays = 0;
        isActivated = false;
	}
	
	void Update () 
    {
        // If we are already playing we don't need to do anything
        if(audioSource.isPlaying)
            return;

        // If we have already played enough we dont need to do anything
        if(totalPlays >= playCount)
            return;

        // If we are already deactivated
        if(!isActivated)
            return;

        // Allow to be activated again
        isActivated = false;
	}

    void OnTriggerEnter()
    {
        if(isActivated)
            return;

        if(totalPlays >= playCount)
            return;

        audioSource.PlayOneShot(audioClip);
        isActivated = true;
        totalPlays++;
    }
}
