using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public enum ExecutePosition { TOP, BOTTOM };

public class StackManager : MonoBehaviour
{
    [Header("Lists")]
	public List<GameObject> gameAnimals;		// List of animals in the game
	public List<AnimalStack> levelStacks;		// List of stacks in the game

    [Header("Current Animal")]
	public int stackIndex;						// Current stack we are working with
    public int animalIndex;						// Current animal in the current stack we are working with
	public AnimalStack currentStack;			// Access to the current stack
	public GameObject currentAnimal;			// Access to the current animal
	public float animalHeight = 1.0f;

    [Header("Merging")]
    public bool canMerge;
    public float disableMergeDuration = 0.2f;

	void Awake ()
	{
		// Get a list of animals in the level (DEPRECIATED IF WE HAVE A SET ORDER)
		gameAnimals = GameObject.FindGameObjectsWithTag("Animal").ToList();

		gameAnimals = gameAnimals.OrderBy(animal => animal.name).ToList();

        // Fixes a bug where a clone is generated
        for(int i = 0; i < gameAnimals.Count; i++)
        {
            if(gameAnimals[i].name.Contains("Clone"))
            {
                gameAnimals.Remove(gameAnimals[i]);
            }
        }

        Debug.Log(gameAnimals[0]);
	}

	void Start()
	{
		// Build a stack for each animal
        InitialiseStacks();

        // Initialise flags
        canMerge = true;
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

		// Get height of the animals
		animalHeight = currentAnimal.transform.localScale.y;
	}

	//
	// Update selection to a particular animal
	//
	public void UpdateSelectedAnimal(GameObject animal)
	{
		// Deactivate the previous animal
		currentAnimal.GetComponent<AnimalBehaviour>().Deactivate();

		UpdateIndices (animal);

		// Activate the new animal
		currentAnimal.GetComponent<AnimalBehaviour>().Activate();
	}

	public void UpdateIndices(GameObject animal)
	{
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
	}

    //
    // Splits a stack, adding everything from the index upwards into its own stack
    // pos represents how we want to split
    // Top represents stepping of a stack
    // Bottom represents gecko stepping out beneath stack
    //
	public void SplitStack(AnimalStack oldStack, int index, ExecutePosition pos, Vector3 moveDirection)
	{
		AnimalStack newStack = new AnimalStack();

        if(pos.Equals(ExecutePosition.TOP))
        {
            // Add animals to new stack and remove from old
            newStack.GetList().AddRange( oldStack.GetList().GetRange(index, oldStack.GetSize() - index) );
            oldStack.GetList ().RemoveRange(animalIndex, oldStack.GetSize()-animalIndex);

            // Update stack
            for(int i = 0; i < newStack.GetSize(); i++)
            {
                // Move animals in direction
				//newStack.Get(i).transform.position = newStack.Get (i).transform.position + moveDirection*1.1f*animalHeight;// + new Vector3(0.0f, -animalHeight*animalIndex,0.0f);
                
                // Update parent and index
                newStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, i);
            }
            
            // Enable base animal
            newStack.Get(0).GetComponent<Rigidbody>().useGravity = true;

            // Add new stack to levelStacks before updating the selection so it can be considered
            levelStacks.Add(newStack);

            // Refresh selection
            UpdateSelectedAnimal(newStack.Get(0));
        }
        else if(pos.Equals(ExecutePosition.BOTTOM))
        {
            Vector3 basePos = oldStack.Get(0).transform.position;

            // Add gecko to the new stack
            newStack.GetList().AddRange( oldStack.GetList().GetRange(0, index));

            // Remove gecko from the old stack
            oldStack.GetList().RemoveRange(0, index);

            // Reposition the old stack
            for(int i = 0; i < oldStack.GetSize(); i++)
            {
                // Move animals in direction
                oldStack.Get(i).transform.position = basePos + new Vector3(0.0f, animalHeight*i,0.0f);

                // Refresh index
                oldStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(oldStack, i);
            }

            // Reposition the new stack
            for(int i = 0; i < newStack.GetSize(); i++)
            {
                // Move animals in direction
				newStack.Get(i).transform.position = basePos + moveDirection*1.1f*animalHeight;// + new Vector3(0.0f, -animalHeight*animalIndex,0.0f);
                
                // Refresh index
                newStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, i);
            }

            // Add new stack to the list
            levelStacks.Add(newStack);
        }

        // Stop merging for a period
        DisableMerge();
	}

	//
	// Merges a stack into the current one 
	// StackA represents the one we want to merge into ours
	// StackB represents our current stack
	// pos represents how we want to merge
	//
    public void MergeStack(AnimalStack stackA, AnimalStack stackB, ExecutePosition pos)
	{
		AnimalStack newStack = new AnimalStack();
		Vector3 basePos = stackA.Get(0).transform.position;

		// Define the order to merge stacks
		switch(pos)
		{
			// Places the current stack on top of the new one (i.e. we should use this for walking into a stack)
            case ExecutePosition.TOP:
				newStack.GetList().AddRange(stackB.GetList());
				newStack.GetList().AddRange(stackA.GetList());
					break;

			// Places the current stack beneath the new one (i.e. we should use this for falling ontop of the stack)
            case ExecutePosition.BOTTOM:
				newStack.GetList().AddRange(stackA.GetList());
				newStack.GetList().AddRange(stackB.GetList());
					break;
		}

		// Refresh stack
		for(int i = 0; i < newStack.GetSize(); i++)
		{
			
			// Make animals above base animal non-kinematic
			if(i>0)
			{
				newStack.Get(i).GetComponent<Rigidbody>().isKinematic = false;
				newStack.Get(i).GetComponent<Rigidbody>().useGravity = false;
			}

			// Refresh position
			newStack.Get(i).transform.position = basePos + new Vector3(0, animalHeight * i, 0);
			newStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, i);
			newStack.Get(i).GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);

			// Keep rotation same as base rotation
			newStack.Get (i).transform.rotation = newStack.Get(0).transform.rotation;
		}

		// Remove the old stacks
		levelStacks.Remove(stackA);
		levelStacks.Remove(stackB);

		// Add the new one
		levelStacks.Add(newStack);

		// Refresh selections
		UpdateIndices(currentAnimal);

        // Stop merging for a period
        DisableMerge();
	}

    public void DisableMerge()
    {
        StartCoroutine(DisableMergeProcess());
    }
    
    private IEnumerator DisableMergeProcess()
    {
        canMerge = false;
        
        yield return new WaitForSeconds(disableMergeDuration);
        
        canMerge = true;
    }
}
