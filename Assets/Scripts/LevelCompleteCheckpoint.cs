using UnityEngine;
using System.Collections;

public class LevelCompleteCheckpoint : MonoBehaviour
{
	private bool isActivated = false;

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