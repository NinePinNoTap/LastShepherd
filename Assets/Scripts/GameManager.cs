using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
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
		Debug.Log ("GAME COMPLETE");
		//Application.LoadLevel(Application.loadedLevel);
	}
}

