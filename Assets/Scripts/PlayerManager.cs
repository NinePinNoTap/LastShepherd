/*
	Script for Moving, Stack and Throwing Animals

	TODO
	
	1.
		Better error checking for creating throwing tiles.
		Don't create tiles if the stack we want to throw cant fit.
	2. 
		Allow animals to step off the stack when already on an upper tile
		
	3. 
		Make sure theres a clear path when moving off a stack to maintain realism? Check with designers!

  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Helper;

public class PlayerManager : MonoBehaviour
{
	[Header("Initialising")]
	public LevelManager levelManager;        // Access to LevelManager

	[Header("Stacking")]
    public List<AnimalStack> levelStacks;    // All animal stacks in level
    public GameObject currentAnimal;         // Current object being moved
    public AnimalStack currentStack;         // Current stack we are moving
    public int stackIndex;                   // ID of the current stack we are moving
    public int animalIndex;                  // ID of the animal within the stack

	[Header("Properties")]
	public float animalHeight = 1.0f;        // Height of animals
	public float tileWidth = 1.0f;           // Size of a tile

    [Header("Throwing")]
    public bool isThrowing;                  // Flag for whether we are throwing or not
	public GameObject throwingParent;		 // Parent for throwing boxes
    public List<GameObject>[] throwingBoxes; // List of boxes for each direction
	public GameObject throwTarget;			 // Current highlighted obj
	public int throwDirection;				 // Index of the throw direction
	public int throwIndex;					 // Index within the throw direction

	[Header("GUI")]
	public int stepCount;                    // Counter for number of moves made
	public Text stepText;                    // GUI Text Access

	[Header("Misc")]
	public LayerMask tileMask;				 // Raycasting Layer Mask for ignoring tiles

    // Initialise the Class
	void Start ()
	{
		// Get access the attached level manager
		levelManager = this.GetComponent<LevelManager>();

        // Initialise Level
        CreateStacks();
        CreateThrowing();
		
		// Initialise Starting Vars
		stepCount = 0;
		stepText.text = "Step Count : " + stepCount;
		tileMask = LayerMask.NameToLayer("Tile");
        
		// Highlight starting animal
		UpdateColour(currentAnimal, Color.black, Color.white, true);
	}

    // Frame Updates
    void Update()
    {
        // Check for toggle throwing mode
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isThrowing = !isThrowing;
            throwingParent.SetActive(isThrowing);

            if (isThrowing)
            {
                // Create new ones
                RefreshThrowingBoxes();
            }
            else
            {
                // Destroy boxes
                ClearThrowingBoxes();
            }
        }

        if (isThrowing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Lets Throw!");

                // Throw stack
                Throw();
            }
            else
            {
                // Toggle around the boxes
                MoveThrowTarget(KeyCode.W, 0);
                MoveThrowTarget(KeyCode.A, 1);
                MoveThrowTarget(KeyCode.D, 2);
                MoveThrowTarget(KeyCode.S, 3);
            }
        }
        else
        {
            // Check for movement input
            MoveObject(KeyCode.W, new Vector3(0, 0, 1)); // Forward
            MoveObject(KeyCode.A, new Vector3(-1, 0, 0)); // Left
            MoveObject(KeyCode.D, new Vector3(1, 0, 0)); // Right
            MoveObject(KeyCode.S, new Vector3(0, 0, -1)); // Back

            // Check for switching animals/stacks
            SwitchSelected(KeyCode.Tab, KeyCode.LeftShift);
        }
    }

    // Initialise the level
    private void CreateStacks()
    {
        // Create stacks of each animal
        levelStacks = new List<AnimalStack> ();
        for(int i = 0; i < levelManager.GetNumberOfAnimals(); i++)
        {
            // Create a stack and add an animal to it
            AnimalStack temp = new AnimalStack();
            temp.Add(levelManager.GetAnimal(i));
            levelStacks.Add(temp);
        }

		// Initialise Properties
		stackIndex = 0;
		animalIndex = 0;
		currentStack = levelStacks[stackIndex];
		currentAnimal = currentStack.Get(animalIndex);
    }

    private void CreateThrowing()
    {
        // Create Parent
		throwingParent = new GameObject ();
		throwingParent.name = "Throwing Range Boxes";

        // Create a list for each direction
        throwingBoxes = new List<GameObject>[4];
        for(int i = 0; i < 4; i++)
        {
			// Create a list for each direction
            throwingBoxes[i] = new List<GameObject>();
        }

		// Initialise Throwing
		isThrowing = false;
		throwingParent.SetActive(isThrowing);
		throwDirection = 0;
		throwIndex = 0;
		throwTarget = null;
    }

    // Throwing
	private void MoveThrowTarget(KeyCode Key, int Direction)
	{
		// Check for input
		if(!Input.GetKeyDown(Key))
		{
			// Key not pressed
			return;
		}

		// Check if theres targets in this direction
		if(throwingBoxes[Direction].Count == 0)
		{
			// We cant select anything in this direction
			return;
		}
		
		// Reset highlight
		if(throwTarget)
		{
			UpdateColour(throwTarget, Color.red, Color.white, false);
		}

		// If we are already on the same direction plane
		if(throwDirection == Direction)
		{
			// Change to next target
			throwIndex++;
			Utility.Wrap(ref throwIndex, 0, throwingBoxes[throwDirection].Count - 1);
		}
		else
		{
			// Update to new direction
			throwDirection = Direction;
			throwIndex = 0;
		}

		// Update target
		throwTarget = throwingBoxes[throwDirection][throwIndex];

		// Highlight Object
		UpdateColour(throwTarget, Color.red, Color.white, true);
	}

	private void Throw()
	{
		Vector3 TargetPosition;

		// Grab the target position
		TargetPosition = throwTarget.transform.position;

		// Increase position to where an animal would be
		TargetPosition.y += 0.45f;

		MoveStack(TargetPosition);

		isThrowing = false;
		throwingParent.SetActive(isThrowing);
		ClearThrowingBoxes();
	}

	private void RefreshThrowingBoxes()
	{
		// Refresh positions of each throwing range
		BuildThrowingRange(0, new Vector3(0,0,1)); // Forward
		BuildThrowingRange(1, new Vector3(-1,0,0)); // Left
		BuildThrowingRange(2, new Vector3(1,0,0)); // Right
		BuildThrowingRange(3, new Vector3(0,0,-1)); // Backward

		// Initialise to base
		throwIndex = 0;
        for (int i = 0; i < 4; i++)
        {
			// Check if the list has elements in it
            if(throwingBoxes[i].Count > 0)
			{
				throwDirection = i;
				break;
			}
        }

		// Initialise target
		throwTarget = throwingBoxes[throwDirection][throwIndex];
		UpdateColour(throwTarget, Color.red, Color.white, true);
	}

	private void ClearThrowingBoxes()
	{
		foreach(List<GameObject> list in throwingBoxes)
		{
			// Destroy the game objects
			foreach(GameObject obj in list)
			{
				Destroy (obj);
			}

			// Clear the list
			list.Clear();
		}
	}

	private void BuildThrowingRange(int ID, Vector3 direction)
	{
		Vector3 CurrentPosition;
		Vector3 ObjPos;
		Vector3 ThrowTileFinder;
		Vector3 Height;
		RaycastHit hit;

		// Get starting position
		CurrentPosition = currentAnimal.transform.position;
		CurrentPosition.y -= animalHeight / 2;

		ThrowTileFinder = new Vector3(0,animalHeight*2,0);
		Height = new Vector3(0,animalHeight,0);

		// Recalculate each throwing box
		for(int i = 0; i < 4; i++)
		{
			CurrentPosition += direction * tileWidth;

			// Find the object at the distance
			if(Physics.Raycast(CurrentPosition + ThrowTileFinder, -Vector3.up, out hit, 100.0f, 1 << tileMask))
			{
				// Create a throwing tile
				ObjPos = hit.transform.gameObject.transform.position + new Vector3(0,animalHeight/2,0);
				ObjPos += new Vector3(0,0.05f,0);

				// Check if we can see the throwing range box
				if(!Physics.Linecast(currentAnimal.transform.position + Height, ObjPos, out hit, 1 << tileMask))
				{
					// Create a throwing option
					throwingBoxes[ID].Add(CreateThrowingBox(ObjPos));
				}
			}
		}
	}
	
	private GameObject CreateThrowingBox(Vector3 Position)
	{
		GameObject box = GameObject.CreatePrimitive (PrimitiveType.Cube);
		box.transform.localScale = new Vector3 (tileWidth, 0.1f, tileWidth);
		box.transform.position = Position;
		box.transform.parent = throwingParent.transform;
		Destroy(box.GetComponent<BoxCollider>());
		
		return box;
	}

    // Moving
	private void MoveObject(KeyCode key, Vector3 direction)
	{
		// If the required key hasnt been pressed
		if(!Input.GetKeyDown(key))
		{
			// Dont continue
			return;
		}
		
		RaycastHit hit;
		Vector3 CurrentPosition;
		Vector3 MoveAmount;
		Vector3 DesiredPosition;
		
		// Get position of current animal and move amount
		CurrentPosition = currentAnimal.transform.position;
		MoveAmount = direction * tileWidth;

		// Position we want to move to
		DesiredPosition = CurrentPosition;
		
		// Look forward from current spot in move direction
		if(Physics.Raycast(CurrentPosition, direction, out hit, tileWidth))
		{
			// Theres something in the way
			if(hit.transform.gameObject.tag == "Tile")
			{
				// Theres a tile in the way
				
				// Look forward from space above
				if(Physics.Raycast(CurrentPosition + new Vector3(0,animalHeight,0), direction, out hit, tileWidth))
				{
					if(hit.transform.gameObject.tag == "Tile")
					{
						// Theres a tile in the way
						Debug.Log ("Theres a tile in the way!");
						return;
					}
					else if(hit.transform.gameObject.tag == "Animal")
					{
						// Theres an animal in the way
						Debug.Log ("Step Up and Stack!");

						// Step Up and Stack!
						DesiredPosition = CurrentPosition + MoveAmount + new Vector3(0,animalHeight,0);
					}
				}
				else
				{
                    // There space is free but can we jump up?

                    if (Physics.Raycast(currentStack.Get(currentStack.GetSize()-1).transform.position, Vector3.up, out hit, animalHeight))
                    {
                        // Theres something above us

                        Debug.Log("Theres a tile above us!");
                        return;
                    }

					// Theres nothing there
					Debug.Log ("Step Up!");
					
					// Step Up and Move!
					DesiredPosition = CurrentPosition + MoveAmount + new Vector3(0,animalHeight,0);
				}
				
			}
			else if(hit.transform.gameObject.tag == "Animal")
			{
				// Theres an animal in the way

				// Cycle through stacks and find the object in the way
				foreach(AnimalStack stack in levelStacks)
				{
					if(stack.GetList().Contains(hit.transform.gameObject))
					{
						// Found the object so get the bottom animal position
						DesiredPosition = stack.Get(0).transform.position;
					}
				}
			}
			else
			{
				// Theres something else in the way
				Debug.Log ("Theres something else here!");

				return;
			}
		}
		else
		{
			// The space is free
			
			if(Physics.Raycast(CurrentPosition + MoveAmount, -Vector3.up, out hit, animalHeight))
			{
				// Theres something beneath the desired position
				
				if(hit.transform.gameObject.tag == "Tile")
				{
					// Theres a block beneath
					Debug.Log ("Move Forward!");

					
					// Move Forward!
					DesiredPosition = CurrentPosition + MoveAmount;
				}
				else if(hit.transform.gameObject.tag == "Animal")
				{
					// Theres an animal beneath
					Debug.Log ("Step Down and Stack!");

					
					// Step Down and Stack!
					DesiredPosition = CurrentPosition + MoveAmount - new Vector3(0,animalHeight,0);
				}
				else
				{
					// Theres something else in the way
					Debug.Log ("Theres something else!");

					return;
				}
			}
			else
			{
				// Theres no ground beneath
				
				if(Physics.Raycast(CurrentPosition + MoveAmount, -Vector3.up, out hit, 2 * animalHeight))
				{
					// Theres something 2 tiles beneath the desired position
					
					if(hit.transform.gameObject.tag == "Tile")
					{
						// Theres a block beneath
						Debug.Log ("Step Down!");

						// Step Down and Move!
						DesiredPosition = CurrentPosition + MoveAmount - new Vector3(0,animalHeight,0);
					}
				}
			}	
		}

		// If we have somehow managed to keep same desired position
		if(CurrentPosition.Equals(DesiredPosition))
		{
			// Exit as we are already on the correct place
			return;
		}

        // Check if the desired position can be moved to
		CheckNewPosition(DesiredPosition);
	}

	private void MoveStack(Vector3 position)
	{
		AnimalStack stack;
		
		// Check for a stack on the new tile 
		stack = FindStack(position);

		// Check if there was a stack on the new position
		if(stack == null)
		{
			// Selected base stack animal
			if(animalIndex == 0)
			{
				// Update position of current animal
				currentAnimal.transform.position = position;

				// Reorganise stack
				RepositionStack();

				return;
			}
			else
			{
				// Create a new stack for the empty tile
				stack = new AnimalStack();
				levelStacks.Add(stack);
			}
		}

		// Transfer to new stack
		TransferToNewStack(stack);
		
		// Update position of base animal
		currentAnimal.transform.position = position;

		// Organise stack
		RepositionStack();
    }

    private void CheckNewPosition(Vector3 position)
    {
        int AnimalsToMove;
        RaycastHit hit;

        // Calculate how many animals to move
        AnimalsToMove = currentStack.GetSize() - animalIndex - 1;

        // Draw a line where the stack would be
        if (Physics.Linecast(position, position + new Vector3(0, (AnimalsToMove * animalHeight), 0), out hit))
        {
            // Theres an obstacle in the way so we can't move
            return;
        }

        // Move the stack to the new position
        MoveStack(position);

        // Increase step counter and refresh text
        UpdateGUI();
    }

	private void TransferToNewStack(AnimalStack stack)
	{
		List<GameObject> ObjTransfer;
        
        // Create a list to hold objects being moved
        ObjTransfer = new List<GameObject>();

		// Copy animals to the transfer list
		ObjTransfer = currentStack.GetList().GetRange(animalIndex, currentStack.GetSize() - animalIndex);

		// Remove animals from the current stack
		currentStack.GetList().RemoveRange(animalIndex, currentStack.GetSize () - animalIndex);

		// If the stack is now empty, delete it
		if(currentStack.GetSize() == 0)
		{
			levelStacks.Remove(currentStack);
		}

		// Add the transferring objects to the list
		stack.GetList().AddRange(ObjTransfer);

		// Cycle through and find the new stack
		for(int i = 0; i < levelStacks.Count; i++)
		{
			if(levelStacks[i].Equals(stack))
			{
                // Found the stack so store its ID
				stackIndex = i;
			}
		}

		// Reset Vars
		UpdateColour(currentAnimal, Color.black, Color.white, false);
		animalIndex = 0;
		currentStack = levelStacks[stackIndex];
		currentAnimal = currentStack.Get(animalIndex);
		UpdateColour(currentAnimal, Color.black, Color.white, true);
	}

	private void RepositionStack()
	{		
		Vector3 basePos;

		// Get the base position of the current stack
		basePos = currentStack.Get(0).transform.position;

		// Cycle through and reorganise animal heights
		for(int i = 0; i < currentStack.GetSize(); i++)
		{
			currentStack.Get (i).transform.position = basePos;
			basePos.y += animalHeight;
		}
	}

	private void SwitchSelected(KeyCode Forward, KeyCode Backward)
	{
		// Check to make sure we do want to switch
		if(!Input.GetKeyDown(Forward))
			return;

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
		}

		// Switch to new animal in stack
		UpdateColour(currentAnimal, Color.black, Color.white, false);
		currentAnimal = currentStack.Get(animalIndex);
		UpdateColour(currentAnimal, Color.black, Color.white, true);
	}

	private AnimalStack FindStack(Vector3 position)
	{
		// Cycle through stacks and find the new one
		foreach(AnimalStack stack in levelStacks)
		{
			// Ensure we dont get the same stack as we are already on
			if(!stack.Equals(currentStack))
			{
				// Check if they are in the same position
				if(stack.Get(0).transform.position.Equals(position))
				{
					// Store the stack
					return stack;
				}
			}
		}

		return null;
	}

	// Other
	private void UpdateColour(GameObject obj, Color a, Color b, bool Toggle)
	{
		if(Toggle)
		{
			// Highlight current animal
			obj.GetComponent<Renderer> ().material.SetColor ("_Color", a);
		}
		else
		{
			// Highlight current animal
			obj.GetComponent<Renderer> ().material.SetColor ("_Color", b);
		}
	}
	
	private void UpdateGUI()
	{
		// Update GUI
		stepCount++;
		stepText.text = "Step Count : " + stepCount;
	}
}