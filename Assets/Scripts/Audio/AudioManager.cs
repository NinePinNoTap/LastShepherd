using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Components")]
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    [Header("Properties")]
    public float audioVolume = 1.0f;
    public int currentClip = 0;

	void Start ()
    {
        // Get the audio source
        audioSource = Utility.HandleComponent<AudioSource>(gameObject);

        // No source defined
        if(audioClips.Length == 0)
        {
            Debug.Log("No audio defined.");
            return;
        }

        // Loop if we only have one sound defined
        audioSource.loop = (audioClips.Length == 1);

        // Set the starting clip
        audioSource.clip = audioClips[currentClip];
        audioSource.Play();
	}
	
	void Update ()
    {
        // Ambient sound is playing so forget
        if(audioSource.isPlaying)
            return;

        // Don't loop
        if(currentClip >= audioClips.Length)
            return;

        // Move to next clip
        currentClip ++;
        Utility.Wrap(ref currentClip, 0, audioClips.Length - 1);

        // Set the clip and play it
        audioSource.clip = audioClips[currentClip];
        audioSource.Play();
	}
}
