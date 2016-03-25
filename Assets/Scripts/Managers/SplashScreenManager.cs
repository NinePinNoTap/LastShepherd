using UnityEngine;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    public float pauseDuration = 2.0f;
    public string sceneToLoad = "MainMenu";

	void Start ()
    {
        StartCoroutine(RunSplashScreen());
	}

    private IEnumerator RunSplashScreen()
    {
        // Pause for secondss
        yield return new WaitForSeconds(pauseDuration);

        // Load next scene
        Application.LoadLevel(sceneToLoad);
    }
}
