using UnityEngine;
using System.Collections;

public class GameOverCheckpoint : MonoBehaviour
{
	public GameObject gameoverCheckpoint;

	private bool isActivated = false;

	void Start ()
	{
	
	}

	void OnTriggerEnter()
	{
		if(isActivated)
			return;

		isActivated = true;
		HandleGameOver();
	}

	private void HandleGameOver()
	{
		// Do something 
		Debug.Log ("You won!");

		Application.Quit();
	}
}