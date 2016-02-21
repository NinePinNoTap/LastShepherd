using UnityEngine;
using System.Collections;

public class LevelCompleteCheckpoint : MonoBehaviour
{
	[Header("Components")]
	public GameManager gameManager;

	[Header("Properties")]
	public string sceneToLoad = "MainMenu";
	public bool isActivated = false;

	void Awake()
	{
		// Make sure we have access to the game controller
		if(!gameManager)
		{
			GameObject.FindGameObjectWithTag("Controller").GetComponent<GameManager>();
		}
	}

	void OnTriggerEnter()
	{
		if(isActivated)
			return;

		// Flag its activated so we cant activate multiple times
		isActivated = true;

		// Go to next level (-- TEMP SET TO MAIN MENU)
		gameManager.DoNextLevel(sceneToLoad);
	}
}