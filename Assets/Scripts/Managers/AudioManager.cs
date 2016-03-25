//
// AudioManager by Louis Dimmock
// Developed for AG1107A Network Game Development
//

using UnityEngine;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Current Audio")]
    public AudioSource audioSource;
    public AudioClip currentClip;
    public float audioVolume = 1.0f;
    public float fadeDuration = 0.5f;

    // Guarantee this is a singleton
    protected AudioManager()
    {
        // Make sure we have an audio source
        if(!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
    }

    public void PlayClip(AudioClip clip, float volume)
    {
        // Make sure we passed an audio clip
        if(clip == null)
            return;

        // Make sure we have a clip set
        if(currentClip != null)
        {
            if(currentClip.Equals(clip))
            {
                return;
            }
        }

        // Store the current clip
        currentClip = clip;

        StartCoroutine(SwitchAudio(clip, volume));
    }

    private IEnumerator SwitchAudio(AudioClip clip, float volume)
    {
        // Make sure we have a clip to fade out
        if(audioSource.clip)
        {
            // Fade Audio Out
            yield return StartCoroutine(FadeVolumeOut());
        }

        // Set audio and play
        audioSource.clip = clip;
        audioSource.Play();

        // Fade Audio In
        yield return StartCoroutine(FadeVolumeUp(audioVolume));

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator FadeVolumeOut()
    {
        while(true)
        {
            float vol = audioSource.volume;

            vol -= (1/fadeDuration) * Time.deltaTime;

            audioSource.volume = vol;

            if(vol < 0)
                break;

            yield return null;
        }

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator FadeVolumeUp(float targetVol)
    {
        while(true)
        {
            float vol = audioSource.volume;

            vol += (1/fadeDuration) * Time.deltaTime;

            audioSource.volume = vol;

            if(vol > targetVol)
                break;

            yield return null;
        }

        // Make sure we are using correct volume
        audioSource.volume = targetVol;

        yield return new WaitForEndOfFrame();
    }
}
