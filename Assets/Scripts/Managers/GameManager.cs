using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
    [Header("Constants")]
	public int FALL_THRESHOLD = -10;
	public int levelClock = 100;

    [Header("AudioManager")]
    public AudioClip ambientAudio;
    public float ambientVolume = 1.0f;
    private AudioManager audioManager;

	[Header("Level Overlay")]
	public GameObject canvasOverlay;
	public string keyCode = "PS4_BUTTON_L2";

	[Header("Game Complete / Game Over")]
	public string msgGameOver = "Game Over";
	public string msgGameComplete = "You Win";
	public GameObject completeOverlay;
	public Text outputText;
	public Button restartButton;
	public Button levelButton;
	public EventSystem eventSystem;

	void Start ()
	{
        //==========================
        // Create the Audio Manager
        //==========================

        audioManager = AudioManager.Instance;
        audioManager.gameObject.name = "AudioManager";
        audioManager.transform.parent = transform.parent;
        audioManager.PlayClip(ambientAudio, ambientVolume);

		completeOverlay.SetActive(false);
		canvasOverlay.SetActive(false);
	}

	void Update()
	{
		if(Input.GetButtonDown(keyCode))
		{
			canvasOverlay.SetActive(true);
			Time.timeScale = 0.0f;
		}
		else if(Input.GetButtonUp(keyCode))
		{
			canvasOverlay.SetActive(false);
			Time.timeScale = 1.0f;
		}
	}

    public void DoGameComplete()
	{
		Debug.Log ("GAME COMPLETE");
		
		completeOverlay.SetActive(true);
		outputText.text = msgGameComplete;

		restartButton.interactable = true;
		levelButton.interactable = true;
		restartButton.Select();
		
		eventSystem.SetSelectedGameObject(restartButton.gameObject);
	}

	public void DoGameOver()
	{
		Debug.Log ("GAME OVER");

		completeOverlay.SetActive(true);
		outputText.text = msgGameOver;

		restartButton.interactable = true;
		levelButton.interactable = true;
		restartButton.Select();
		
		eventSystem.SetSelectedGameObject(restartButton.gameObject);
	}

	public void DoNextLevel(string sceneName)
	{
		LoadingScreenManager.LoadScene(sceneName);
	}
}

