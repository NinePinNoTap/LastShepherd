using UnityEngine;
using System.Collections;
using System;

public class ToggleMonobehaviour : MonoBehaviour 
{
    [Header("Components")]
    public MonoBehaviour componentToggle;
    public StackManager stackManager;

    [Header("Settings")]
    public KeyCode activateKey;
    public bool isShowing = true;

	void Start ()
    {
        componentToggle.enabled = isShowing;

        if(!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }
	}
	
	void Update ()
    {
        // Check for input
        if(Input.GetKeyDown(activateKey))
        {
            // Reverse the flag
            isShowing = !isShowing;

            componentToggle.enabled = isShowing;
        }

        if(stackManager.currentAnimal.GetComponent<AnimalBehaviour>().isMoving && isShowing)
        {
            isShowing = false;

            componentToggle.enabled = isShowing;

            Time.timeScale = 1;
        }
	}
}
