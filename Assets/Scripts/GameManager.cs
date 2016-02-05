using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Helper;

public class GameManager : MonoBehaviour {

	public LevelManager levelManager;  

	public List<AnimalStack> levelStacks;
	public AnimalStack currentStack;
	public int stackIndex;
	public int animalIndex;
	
	 
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
	}

	void Update ()
	{
		SwitchSelected(KeyCode.Tab, KeyCode.LeftShift);
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
	
	private void SwitchSelected(KeyCode Forward, KeyCode Backward)
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
}
