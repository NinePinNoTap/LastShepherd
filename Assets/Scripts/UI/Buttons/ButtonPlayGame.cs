using UnityEngine;
using System.Collections;

public class ButtonPlayGame : MonoBehaviour
{
	public void GoToLevel(string sceneName)
	{
		Application.LoadLevel(sceneName);
	}
}
