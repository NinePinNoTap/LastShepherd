using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
	[Header("Visuals")]
	public Image progressBar;
	public Image backgroundOverlay;

	[Header("Timings")]
	public float additionalLoadTime = 5.0f;
	public float fadeDuration = 1.0f;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip audioClip;

	[Header("Scenes")]
	public static string loadingScreenName = "LoadingScreen";
	public static string sceneToLoad = "";
	public AsyncOperation asyncOperation;

	public static void LoadScene(string sceneName)
	{
		// Store the name of the scene to load
		sceneToLoad = sceneName;

		// Set loading priority
		Application.backgroundLoadingPriority = ThreadPriority.Low;

		// Load the loading screen
		Application.LoadLevel(loadingScreenName);
	}

	void Start()
	{
		// Make sure we have a level to load
		if(sceneToLoad.Length == 0)
		{
			Debug.Log("No scene defined");
			return;
		}

		// Create audio
		InitialiseAudio();

		// Start loading
		StartCoroutine(InitialiseSceneLoading());

		DontDestroyOnLoad(this);
	}

	void Update()
	{
		// Just for fun
		progressBar.transform.Rotate( new Vector3(0,0, -10 * Time.deltaTime));
	}

	private void InitialiseAudio()
	{
		// Set up audio
		audioSource = GetComponent<AudioSource>();
		audioSource.clip = audioClip;
		audioSource.loop = true;
		audioSource.Play();	
	}

	private IEnumerator InitialiseSceneLoading()
	{
		float lastProgress = 0.0f;
		float currentTime = Time.time;
		float maxTime = Time.time + additionalLoadTime;

		Debug.Log("Loading Scene");

		yield return null; 

		// Fade in
		yield return StartCoroutine(FadeScreen(1.0f));

		//===============
		// Start Loading
		//===============

		LoadLevel();

		//=====================
		// Wait for Scene Load
		//=====================

		while(!IsSceneLoading())
		{
			yield return null;

			// Update the visual
			if(asyncOperation.progress > lastProgress)
			{
				lastProgress = asyncOperation.progress;
			}
		}

		float waitTime = maxTime - Time.time;

		// Check to see if we can continue or wait for a bit
		if(Time.time < maxTime)
		{
			yield return new WaitForSeconds(waitTime);
		}

		// Enable scene to be loaded
		asyncOperation.allowSceneActivation = true;

		// Hide the progress bar
		progressBar.enabled = false;

		// Stop playing audio
		audioSource.Stop();

		//==============
		// Done loading
		//==============

		// Fade out
		yield return StartCoroutine(FadeScreen(0.0f));

		// Destroy the scene manager
		Destroy(this.gameObject);
	}

	private IEnumerator FadeScreen(float targetAlpha)
	{
		float currentAlpha = (targetAlpha*-1) + 1;
		float frame = 0.0f;

		yield return null;

		// Check to see if we have finished
		while(backgroundOverlay.color.a != targetAlpha)
		{
			frame += Time.deltaTime;

			// Adjust alpha
			Color c = backgroundOverlay.color;
			c.a = Mathf.Lerp(currentAlpha, targetAlpha, frame / fadeDuration);
			backgroundOverlay.color = c;
			yield return null;
		}
	}
	
	private void LoadLevel()
	{
		asyncOperation = Application.LoadLevelAsync(sceneToLoad);
		asyncOperation.allowSceneActivation = false;
	}

	private bool IsSceneLoading()
	{
		return (asyncOperation.isDone) || (asyncOperation.progress >= 0.9f);
	}
}
