using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
	[Header("Buttons")]
	public List<Button> menuButtons;
	public List<Button> levelButtons;

	[Header("Level Select")]
	public GameObject mapBackground;

	[Header("Instructions")]
	public GameObject instructionsImage;

	[Header("Event System")]
	public EventSystem eventSystem;

	void Start ()
	{
		// Initialise screen
		EnableMenuButtons();
		ShowLevelSelect(false);
		ShowInstructions (false);
	}

	public void ShowLevelSelect(bool flag)
	{
		// Loop through and show/hide the level buttons
		for(int i = 0; i < levelButtons.Count; i++)
		{
			levelButtons[i].gameObject.SetActive(flag);
		}

		// Show/hide map
		mapBackground.SetActive(flag);
	}

	public void ShowInstructions(bool flag)
	{
		// Show/hide instructions
		instructionsImage.SetActive(flag);

		if(flag)
		{
			// Select the image
			instructionsImage.GetComponent<Button>().Select();
		}
	}

	public void EnableMenuButtons()
	{
		// Loop through and disable the menu buttons
		for(int i = 0; i < menuButtons.Count; i++)
		{
			menuButtons[i].interactable = true;
		}

		// Select the play game button
		menuButtons[0].Select();
		
		//eventSystem.firstSelectedGameObject = menuButtons[0].gameObject;
	}

	public void DisableMenuButtons()
	{
		// Loop through and disable the menu buttons
		for(int i = 0; i < menuButtons.Count; i++)
		{
			menuButtons[i].interactable = false;
		}

		// Loop through and enable the level buttons
		for(int i = 0; i < levelButtons.Count; i++)
		{
			levelButtons[i].interactable = true;
		}

		// Select the level 1 button
		levelButtons[0].Select();
		//eventSystem.firstSelectedGameObject = levelButtons[0].gameObject;
	}
}