using UnityEngine;
using System.Collections;
using Helper;

public class InputManager : MonoBehaviour
{
	[Header("Components")]
	public StackManager stackManager;					// Access to stacks and animals
	public ThrowManager throwManager;					// Access to throwing

	[Header("Properties")]
	public bool useXboxController = false;				// Flag for whether to use Xbox Controller input
	private AnimalBehaviour controlledAnimal = null;	// Access to the current controlled animal
	private int animalIndex;							// Tracker for current animal index

	[Header("Windows Keys")]
	public KeyCode forwardWinKey = KeyCode.W;
	public KeyCode backWinKey = KeyCode.S;
	public KeyCode leftWinKey = KeyCode.A;
	public KeyCode rightWinKey = KeyCode.D;

	public KeyCode animalWinKey1 = KeyCode.Alpha1;
	public KeyCode animalWinKey2 = KeyCode.Alpha2;
	public KeyCode animalWinKey3 = KeyCode.Alpha3;
	public KeyCode animalWinKey4 = KeyCode.Alpha4;

	public KeyCode animalPreviousWinKey = KeyCode.O;
	public KeyCode animalNextWinKey = KeyCode.P;

	public KeyCode throwingWinKey = KeyCode.T;
	public KeyCode throwWinKey = KeyCode.Y;

	[Header("Xbox Keys")]
	public string animalMoveX = "XBOX_THUMBSTICK_LX";
	public string animalMoveY = "XBOX_THUMBSTICK_LY";

	public KeyCode animalXboxKey1 = KeyCode.Joystick1Button0;
	public KeyCode animalXboxKey2 = KeyCode.Joystick1Button1;
	public KeyCode animalXboxKey3 = KeyCode.Joystick1Button2;
	public KeyCode animalXboxKey4 = KeyCode.Joystick1Button3;

	public KeyCode animalPreviousXboxKey = KeyCode.Joystick1Button4;
	public KeyCode animalNextXboxKey = KeyCode.Joystick1Button5;
	
	void Start ()
	{
		// Manually find the stack manager if we didnt set it
		if(stackManager == null)
		{
			stackManager = GetComponent<StackManager>();
		}

		if(throwManager == null)
		{
			throwManager = GetComponent<ThrowManager>();
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
		HandleAnimalSwitching(animalXboxKey1, 0); // A
		HandleAnimalSwitching(animalXboxKey2, 1); // B
		HandleAnimalSwitching(animalXboxKey3, 2); // X
		HandleAnimalSwitching(animalXboxKey4, 3); // Y

		// Switching between stacks
		HandleAnimalSwitching(animalPreviousXboxKey, animalIndex-1); // LB
		HandleAnimalSwitching(animalNextXboxKey, animalIndex+1); // RB

		// Moving Animals
		HandleAnimalControllerMovement();
	}

	private void HandleKeyboardInput()
	{
		// Setting Animals
		HandleAnimalSwitching(animalWinKey1, 0);
		HandleAnimalSwitching(animalWinKey2, 1);
		HandleAnimalSwitching(animalWinKey3, 2);
		HandleAnimalSwitching(animalWinKey4, 3);
		
		// Switching between stacks
		HandleAnimalSwitching(animalPreviousWinKey, animalIndex-1);
		HandleAnimalSwitching(animalNextWinKey, animalIndex+1);

		// Moving and Throwing
		HandleAnimalKeyboardInput();
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
			float x = Input.GetAxis(animalMoveX) * controlledAnimal.moveSpeed;
			float y = Input.GetAxis(animalMoveY) * controlledAnimal.moveSpeed;

			controlledAnimal.MoveStack(new Vector3(x,0,y) * 3);
		}
		else
		{
			if(Input.GetAxis(animalMoveY) > 0)
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, 1));
			}
			else if(Input.GetAxis(animalMoveX) < 0)
			{
				controlledAnimal.HopOffStack(new Vector3(-1, 0, 0));
			}
			else if(Input.GetAxis(animalMoveY) < 0)
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, -1));	
			}
			else if(Input.GetAxis(animalMoveX) > 0)
			{
				controlledAnimal.HopOffStack(new Vector3(1, 0, 0));
			}
		}
	}

	private void HandleAnimalControllerThrowing()
	{
		float x = Input.GetAxis(animalMoveX) * throwManager.moveSpeed;
		float y = Input.GetAxis(animalMoveY) * throwManager.moveSpeed;

		Vector3 moveVelocity = new Vector3(x, 0, y);

		if(moveVelocity.magnitude > 0)
		{
			throwManager.MoveTarget(moveVelocity);
		}
	}

	private void HandleAnimalKeyboardInput()
	{
		if(throwManager.isThrowing)
		{
			// Keyboard Moving
			HandleAnimalKeyboardThrowing();

			// Toggle throwing
			if(Input.GetKeyDown(throwingWinKey))
			{
				throwManager.Deactivate();
			}
		}
		else
		{
			// Keyboard Moving
			HandleAnimalKeyboardMovement();

			// Toggle throwing
			if(Input.GetKeyDown(throwingWinKey))
			{	
				throwManager.Activate();
			}
		}
	}

	private void HandleAnimalKeyboardMovement()
	{
		Vector3 moveVelocity = new Vector3(0,0,0);
		float moveSpeed = controlledAnimal.moveSpeed;

		if(controlledAnimal.stackIndex == 0)
		{
			if(Input.GetKey(forwardWinKey))
			{
				moveVelocity += new Vector3(0,0,-moveSpeed);
			}
			if(Input.GetKey(backWinKey))
			{
				moveVelocity += new Vector3(0,0,moveSpeed);
			}
			if(Input.GetKey(leftWinKey))
			{
				moveVelocity += new Vector3(moveSpeed,0,0);
			}
			if(Input.GetKey(rightWinKey))
			{
				moveVelocity += new Vector3(-moveSpeed,0,0);
			}

			controlledAnimal.MoveStack(moveVelocity);
		}
		else 
		{
			if(Input.GetKeyDown(forwardWinKey))
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, -1));
			}
			else if(Input.GetKeyDown(backWinKey))
			{
				controlledAnimal.HopOffStack(new Vector3(0, 0, 1));	
			}
			else if(Input.GetKeyDown(leftWinKey))
			{
				controlledAnimal.HopOffStack(new Vector3(1, 0, 0));
			}
			else if(Input.GetKeyDown(rightWinKey))
			{
				controlledAnimal.HopOffStack(new Vector3(-1, 0, 0));
			}
		}
	}

	private void HandleAnimalKeyboardThrowing()
	{
		Vector3 moveVelocity = new Vector3(0,0,0);
		float moveSpeed = throwManager.moveSpeed;

		if(Input.GetKey(forwardWinKey))
		{
			moveVelocity += new Vector3(0,0,-moveSpeed);
		}
		if(Input.GetKey(backWinKey))
		{
			moveVelocity += new Vector3(0,0,moveSpeed);
		}
		if(Input.GetKey(leftWinKey))
		{
			moveVelocity += new Vector3(moveSpeed,0,0);
		}
		if(Input.GetKey(rightWinKey))
		{
			moveVelocity += new Vector3(-moveSpeed,0,0);
		}

		// Move target
		if(moveVelocity.magnitude > 0)
		{
			throwManager.MoveTarget(moveVelocity);
		}

		// Throw
		if(Input.GetKeyDown(throwWinKey))
		{
			// Throw
			if(throwManager.CheckThrow())
			{
				Debug.Log ("CAN THROW!");
			}
			else
			{
				Debug.Log ("CANT THROW!");
			}
		}
	}
}
