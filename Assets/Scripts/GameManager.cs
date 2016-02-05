using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Helper;

public class GameManager : MonoBehaviour
{
	public LevelManager levelManager;  

	public List<AnimalStack> levelStacks;
	public AnimalStack currentStack;
	public int stackIndex;
	public int animalIndex;

	[Header("GUI")]
	public Image[] animalPortaitsBG;
	public Image[] animalPortaits;
	 
	void Awake()
	{
		
	}

	void Start ()
	{
		// Get access the attached level manager
		levelManager = this.GetComponent<LevelManager>();
		
		// Initialise Level
		//Debug.Log("Set GM for" + this.gameObject.name);
		CreateStacks();

		for(int i = 0; i < 3; i++)
		{
			animalPortaits[i].color = levelManager.GetAnimal(i).GetComponent<ObjectHighlighter>().baseColor;
		}
	}

	void Update ()
	{
		// WINDOWS
		HandleAnimalSwitching(KeyCode.Tab, KeyCode.LeftShift);

		// XBOX
		HandleAnimalSwitching("XBOX_BUTTON_LB", -1);
		HandleAnimalSwitching("XBOX_BUTTON_RB", 1);

		HandleAnimalPortaits();
	}

	private void CreateStacks()
	{
		// Create stacks of each animal
		levelStacks = new List<AnimalStack> ();
		for(int i = 0; i < levelManager.GetNumberOfAnimals(); i++)
		{
			// Create a stack and add an animal to it
			AnimalStack temp = new AnimalStack();
			temp.Add(levelManager.GetAnimal(i));
			levelManager.GetAnimal(i).GetComponent<AnimalBehaviour>().SetOwner(temp,0);
			
			levelManager.GetAnimal(i).GetComponent<AnimalBehaviour>().SetGameManager(this);
			levelStacks.Add(temp);
		}
		
		stackIndex = 0;
		animalIndex = 0;
		currentStack = levelStacks[stackIndex];
		
		levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Activate();
	}

	private void HandleAnimalPortaits()
	{
		for(int i = 0; i < 3; i++)
		{
			if(levelManager.GetAnimal(i).Equals(levelStacks[stackIndex].animals[animalIndex]))
			{
				animalPortaitsBG[i].color = Color.yellow;
			}
			else
			{
				animalPortaitsBG[i].color = Color.black;
			}
		}
	}
	
	private void HandleAnimalSwitching(KeyCode Forward, KeyCode Backward)
	{
		// Check to make sure we do want to switch
		if(!Input.GetKeyDown(Forward))
			return;
		
		levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Deactivate();
		
		// Move up index
		int Increment = 1;
		
		// If we are holding shift
		if(Input.GetKey(Backward))
		{
			// Move down index
			Increment *= -1;
		}
		
		// Check to see if we can switch to another animal
		if (animalIndex + Increment > 0 && animalIndex + Increment < currentStack.GetSize())
		{
			// Switch to another animal
			animalIndex += Increment;
			
			// Keep within bounds
			Utility.Wrap (ref animalIndex, 0, currentStack.GetSize() - 1);
			
			levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Activate();
			
		}
		else
		{
			// Change Stack Index
			stackIndex += Increment;
			
			// Keep within bounds
			Utility.Wrap (ref stackIndex, 0, levelStacks.Count - 1);
			
			// Get new stack
			currentStack = levelStacks[stackIndex];
			
			// Reset Animal Index
			animalIndex = 0;
			
			levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Activate();
		}
	}
	private void HandleAnimalSwitching(string buttonname, int value)
	{
		// Check to make sure we do want to switch
		if(!Input.GetButtonDown(buttonname))
			return;
		
		levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Deactivate();
				
		// Check to see if we can switch to another animal
		if (animalIndex + value > 0 && animalIndex + value < currentStack.GetSize())
		{
			// Switch to another animal
			animalIndex += value;
			
			// Keep within bounds
			Utility.Wrap (ref animalIndex, 0, currentStack.GetSize() - 1);
			
			levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Activate();
			
		}
		else
		{
			// Change Stack Index
			stackIndex += value;
			
			// Keep within bounds
			Utility.Wrap (ref stackIndex, 0, levelStacks.Count - 1);
			
			// Get new stack
			currentStack = levelStacks[stackIndex];
			
			// Reset Animal Index
			animalIndex = 0;
			
			levelStacks[stackIndex].animals[animalIndex].GetComponent<AnimalBehaviour>().Activate();
		}
	}
}
