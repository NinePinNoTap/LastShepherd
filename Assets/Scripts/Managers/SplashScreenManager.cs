using UnityEngine;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    public float pauseDuration = 2.0f;
    public string sceneToLoad = "MainMenu";
	public string skipKey = "PS4_BUTTON_CROSS";

	void Start ()
    {
        StartCoroutine(RunSplashScreen());
	}

	void Update()
	{
		if(Input.GetButtonDown(skipKey))
		{
			// Stop splash screen
			StopCoroutine(RunSplashScreen());

			// Load next scene
			Application.LoadLevel(sceneToLoad);
		}
	}

    private IEnumerator RunSplashScreen()
    {
        // Pause for secondss
        yield return new WaitForSeconds(pauseDuration);

        // Load next scene
        Application.LoadLevel(sceneToLoad);
    }
}
