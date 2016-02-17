using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Helper;

public enum MergePosition { TOP, BOTTOM };

public class StackManager : MonoBehaviour
{
	public List<GameObject> gameAnimals;		// List of animals in the game
	public List<AnimalStack> levelStacks;		// List of stacks in the game
	public int stackIndex;						// Current stack we are working with
	public int animalIndex;						// Current animal in the current stack we are working with
	public AnimalStack currentStack;			// Access to the current stack
	public GameObject currentAnimal;			// Access to the current animal
	public float animalHeight = 1.0f;

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

	//
	// Create a list of stacks with each animal in its own stack
	//
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

	//
	// Update selection to a particular animal
	//
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

	public void SplitStack(AnimalStack oldStack, int index, Vector3 moveDirection)
	{
		AnimalStack newStack = new AnimalStack();

		// Transfer the stack
		newStack.GetList().AddRange( oldStack.GetList().GetRange(index, oldStack.GetSize() - index) );

		// Update stack
		for(int i = 0; i < newStack.GetSize(); i++)
		{
			// Move animals in direction
			newStack.Get(i).transform.position = newStack.Get (i).transform.position + moveDirection*1.1f*animalHeight + new Vector3(0.0f, -animalHeight*animalIndex,0.0f);

			// Update parent and index
			newStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, i);
		}

		// Enable base animal
		newStack.Get(0).GetComponent<Rigidbody>().useGravity = true;

		// Remove animals from previous stack
		oldStack.GetList ().RemoveRange(animalIndex, oldStack.GetSize()-animalIndex);

		// Add new stack to the list
		levelStacks.Add(newStack);

		// Refresh selection
		UpdateSelectedAnimal(newStack.Get(0));
	}

	//
	// Merges a stack into the current one 
	// StackA represents the one we want to merge into ours
	// StackB represents our current stack
	// pos represents how we want to merge
	//
	public void MergeStack(AnimalStack stackA, AnimalStack stackB, MergePosition pos)
	{
		AnimalStack newStack = new AnimalStack();
		Vector3 basePos = stackA.Get(0).transform.position;

		// Define the order to merge stacks
		switch(pos)
		{
			// Places the current stack on top of the new one (i.e. we should use this for walking into a stack)
			case MergePosition.TOP:
				newStack.GetList().AddRange(stackB.GetList());
				newStack.GetList().AddRange(stackA.GetList());
					break;

			// Places the current stack beneath the new one (i.e. we should use this for falling ontop of the stack)
			case MergePosition.BOTTOM:
				newStack.GetList().AddRange(stackA.GetList());
				newStack.GetList().AddRange(stackB.GetList());
					break;
		}

		// Refresh stack
		for(int i = 0; i < newStack.GetSize(); i++)
		{
			// Refresh position
			newStack.Get(i).transform.position = basePos + new Vector3(0, animalHeight * i, 0);
			newStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, i);
			newStack.Get(i).GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
		}

		// Remove the old stacks
		levelStacks.Remove(stackA);
		levelStacks.Remove(stackB);

		// Add the new one
		levelStacks.Add(newStack);

		// Refresh selections
		UpdateSelectedAnimal(currentAnimal);
	}
}
