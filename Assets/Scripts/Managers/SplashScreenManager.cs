using UnityEngine;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    public float pauseDuration = 2.0f;
    public string sceneToLoad = "MainMenu";
	public string skipKey = "XBOX_BUTTON_A";

	void Start ()
    {
        StartCoroutine(RunSplashScreen());
	}

	void Update()
	{
		if(Input.GetButtonDown("XBOX_BUTTON_A"))
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
