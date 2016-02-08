using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Helper;

public class StackManager : MonoBehaviour
{
	public List<GameObject> gameAnimals;		// List of animals in the game
	public List<AnimalStack> levelStacks;		// List of stacks in the game
	public int stackIndex;						// Current stack we are working with
	public int animalIndex;						// Current animal in the current stack we are working with
	public AnimalStack currentStack;			// Access to the current stack
	public GameObject currentAnimal;			// Access to the current animal

	void Awake ()
	{
		// Get a list of animals in the level (DEPRECIATED IF WE HAVE A SET ORDER)
		gameAnimals = GameObject.FindGameObjectsWithTag("Animal").ToList();
	}

	void Start()
	{
		// Build a stack for each animal
		InitialiseStacks();
	}

	private void InitialiseStacks()
	{
		// Create stacks of each animal
		levelStacks = new List<AnimalStack> ();
		for(int i = 0; i < gameAnimals.Count; i++)
		{
			// Create a stack and add an animal to it
			AnimalStack temp = new AnimalStack();
			gameAnimals[i].GetComponent<AnimalBehaviour>().SetParentStack(temp,0);
			temp.Add(gameAnimals[i]);
			levelStacks.Add(temp);
		}

		// Initialise index
		stackIndex = 0;
		animalIndex = 0;

		// Quick access to current stack/animal
		currentStack = levelStacks[stackIndex];
		currentAnimal = levelStacks[stackIndex].Get(animalIndex);

		// Activate the animal
		currentAnimal.GetComponent<AnimalBehaviour>().Activate();
	}

	public void UpdateSelectedAnimal(GameObject animal)
	{
		// Deactivate the previous animal
		currentAnimal.GetComponent<AnimalBehaviour>().Deactivate();

		// Search all stacks and set index's
		for(int stack = 0; stack < levelStacks.Count; stack++)
		{
			// Check if the stack contains the animal
			if(levelStacks[stack].Contains(animal))
			{
				// Store indices and update
				stackIndex = stack;
				currentStack = levelStacks[stackIndex];
				animalIndex = currentStack.GetIndex(animal);
				currentAnimal = levelStacks[stackIndex].Get (animalIndex);

				break;
			}
		}

		// Activate the new animal
		currentAnimal.GetComponent<AnimalBehaviour>().Activate();
	}
}
