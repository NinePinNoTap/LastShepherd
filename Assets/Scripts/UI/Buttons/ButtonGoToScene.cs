using UnityEngine;
using System.Collections;

public class ButtonGoToScene : MonoBehaviour
{
	public void GoToScene(string sceneName)
	{
		Application.LoadLevel(sceneName);
	}
}
