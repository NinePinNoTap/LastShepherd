using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static int FALL_THRESHOLD = -10;

	void Start ()
	{
	
	}

	void Update ()
	{
	
	}

	public void DoGameComplete()
	{
		Debug.Log ("GAME COMPLETE");
	}

	public void DoGameOver()
	{
		Debug.Log ("GAME OVER");
		// Reload level
		Application.LoadLevel(Application.loadedLevel);
	}

	public void DoNextLevel(string sceneName)
	{
		LoadingScreenManager.LoadScene(sceneName);
	}
}

