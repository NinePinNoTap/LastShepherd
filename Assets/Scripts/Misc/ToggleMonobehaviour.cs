using UnityEngine;
using System.Collections;
using System;

public class ToggleMonobehaviour : MonoBehaviour 
{
    public KeyCode activateKey;
    public bool isShowing = true;
    public MonoBehaviour componentToggle;

	void Start ()
    {
        componentToggle.enabled = isShowing;
	}
	
	void Update ()
    {
        // Check for input
        if(Input.GetKeyDown(activateKey))
        {
            // Reverse the flag
            isShowing = !isShowing;

            componentToggle.enabled = isShowing;

            Time.timeScale = (Time.timeScale * -1) + 1;
        }
	}
}
