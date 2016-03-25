using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    [Header("Constants")]
	public int FALL_THRESHOLD = -10;

    [Header("AudioManager")]
    public AudioClip ambientAudio;
    public float ambientVolume = 1.0f;
    private AudioManager audioManager;

	void Start ()
	{
        //==========================
        // Create the Audio Manager
        //==========================

        audioManager = AudioManager.Instance;
        audioManager.gameObject.name = "AudioManager";
        audioManager.transform.parent = transform.parent;
        audioManager.PlayClip(ambientAudio, ambientVolume);
	}

    public void DoGameComplete()
	{
		Debug.Log ("GAME COMPLETE");

        PlayerPrefs.SetInt("gameWinState", 1);
        Application.LoadLevel("MissionEnd");
	}

	public void DoGameOver()
	{
		Debug.Log ("GAME OVER");

        PlayerPrefs.SetInt("gameWinState", 0);
        Application.LoadLevel("MissionEnd");
	}

	public void DoNextLevel(string sceneName)
	{
		LoadingScreenManager.LoadScene(sceneName);
	}
}

