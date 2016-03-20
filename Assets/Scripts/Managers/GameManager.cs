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

