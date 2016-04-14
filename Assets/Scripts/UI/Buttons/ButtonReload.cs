using UnityEngine;
using System.Collections;

public class ButtonReload : MonoBehaviour
{

	public void ReloadScene()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
