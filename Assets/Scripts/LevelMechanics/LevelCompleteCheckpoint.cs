using UnityEngine;
using System.Collections;

public class LevelCompleteCheckpoint : MonoBehaviour
{
	public GameManager gameManager;
	private bool isActivated = false;

	void OnTriggerEnter()
	{
		if(isActivated)
			return;

		isActivated = true;
		gameManager.DoGameComplete();
	}
}