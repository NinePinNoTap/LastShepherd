using UnityEngine;
using System.Collections;
using Helper;

public class InputManager : MonoBehaviour
{
	[Header("Components")]
	public StackManager stackManager;					// Access to stacks and animals

	[Header("Properties")]
	public bool useXboxController = false;				// Flag for whether to use Xbox Controller input
	private AnimalBehaviour controlledAnimal = null;	// Access to the current controlled animal
	private int animalIndex;							// Tracker for current animal index
	
	void Start ()
	{
		// Manually find the stack manager if we didnt set it
		if(stackManager == null)
		{
			stackManager = GetComponent<StackManager>();
		}

		// Initialise to first animal
		animalIndex = 0;
	}

	void Update ()
	{
		if(controlledAnimal == null)
		{
			controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();
		}
		// Handle input based on input type
		if(useXboxController)
		{
			HandleXboxInput();
		}
		else
		{
			HandleKeyboardInput();
		}
	}

	private void HandleXboxInput()
	{
		// Setting Animals
		HandleAnimalSwitching(KeyCode.JoystickButton0, 0); // A
		HandleAnimalSwitching(KeyCode.JoystickButton1, 1); // B
		HandleAnimalSwitching(KeyCode.JoystickButton2, 2); // X
		HandleAnimalSwitching(KeyCode.JoystickButton3, 3); // Y

		// Switching between stacks
		HandleAnimalSwitching(KeyCode.JoystickButton4, animalIndex-1); // LB
		HandleAnimalSwitching(KeyCode.JoystickButton5, animalIndex+1); // RB

		// Moving Animals
		HandleAnimalControllerMovement();
	}

	private void HandleKeyboardInput()
	{
		// Setting Animals
		HandleAnimalSwitching(KeyCode.Alpha1, 0);
		HandleAnimalSwitching(KeyCode.Alpha2, 1);
		HandleAnimalSwitching(KeyCode.Alpha3, 2);
		HandleAnimalSwitching(KeyCode.Alpha4, 3);
		
		// Switching between stacks
		HandleAnimalSwitching(KeyCode.O, animalIndex-1);
		HandleAnimalSwitching(KeyCode.P, animalIndex+1);

		// Moving Animals
		HandleAnimalKeyboardMovement();
	}

	private void HandleAnimalSwitching(KeyCode key, int value)
	{
		// Check to make sure we do want to switch
		if(!Input.GetKeyDown(key))
			return;

		// Change index
		animalIndex = value;
		
		// Keep within the range
		Utility.Wrap(ref animalIndex, 0, stackManager.gameAnimals.Count - 1);

		// Disable current animal
		controlledAnimal.Deactivate();

		// Tell the stack manager to update to the correct animal
		stackManager.UpdateSelectedAnimal(stackManager.gameAnimals[animalIndex]);

		// Update controlled animal
		controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();
		controlledAnimal.Activate();
	}

	private void HandleAnimalControllerMovement()
	{
		if(controlledAnimal.stackIndex == 0)
		{
			float x = Input.GetAxis("XBOX_THUMBSTICK_LX") * controlledAnimal.moveSpeed;
			float y = Input.GetAxis("XBOX_THUMBSTICK_LY") * controlledAnimal.moveSpeed;

			controlledAnimal.MoveStack(new Vector3(x,0,y) * 3);
		}
		else
		{
			if(Input.GetAxis("XBOX_THUMBSTICK_LY") > 0)
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, 1));
			}
			else if(Input.GetAxis("XBOX_THUMBSTICK_LX") < 0)
			{
				controlledAnimal.HopOffStack(new Vector3(-1, 0, 0));
			}
			else if(Input.GetAxis("XBOX_THUMBSTICK_LY") < 0)
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, -1));	
			}
			else if(Input.GetAxis("XBOX_THUMBSTICK_LX") > 0)
			{
				controlledAnimal.HopOffStack(new Vector3(1, 0, 0));
			}
		}
	}

	private void HandleAnimalKeyboardMovement()
	{
		Vector3 moveVelocity = new Vector3(0,0,0);
		float moveSpeed = controlledAnimal.moveSpeed;

		if(controlledAnimal.stackIndex == 0)
		{
			if(Input.GetKey(KeyCode.W))
			{
				moveVelocity += new Vector3(0,0,-moveSpeed);
			}
			if(Input.GetKey(KeyCode.A))
			{
				moveVelocity += new Vector3(moveSpeed,0,0);
			}
			if(Input.GetKey(KeyCode.S))
			{
				moveVelocity += new Vector3(0,0,moveSpeed);
			}
			if(Input.GetKey(KeyCode.D))
			{
				moveVelocity += new Vector3(-moveSpeed,0,0);
			}

			controlledAnimal.MoveStack(moveVelocity);
		}
		else 
		{
			if(Input.GetKeyDown(KeyCode.W))
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, -1));
			}
			else if(Input.GetKeyDown(KeyCode.A))
			{
				controlledAnimal.HopOffStack(new Vector3(1, 0, 0));
			}
			else if(Input.GetKeyDown(KeyCode.S))
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, 1));	
			}
			else if(Input.GetKeyDown(KeyCode.D))
			{
				controlledAnimal.HopOffStack(new Vector3(-1, 0, 0));
			}
		}
	}
}
