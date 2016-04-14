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
	public string pauseKey = "PS4_BUTTON_OPTIONS";
    public string resetKey = "PS4_BUTTON_CROSS";
    public string quitKey = "PS4_BUTTON_SQUARE";
    public bool isShown = false;

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
        // Toggle Menu
		if(Input.GetButtonDown(pauseKey))
		{
            isShown = !isShown;

            canvasOverlay.SetActive(isShown);

            if(isShown)
            {
                Time.timeScale = 0.0f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }
		}

        // Pause Screen Controls
        if(isShown)
        {
            if(Input.GetButtonDown(resetKey))
            {
                Time.timeScale = 1.0f;
                Application.LoadLevel(Application.loadedLevel);
            }
            if(Input.GetButtonDown(quitKey))
            {
                Time.timeScale = 1.0f;
                LoadingScreenManager.LoadScene("MainMenu");
            }
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

