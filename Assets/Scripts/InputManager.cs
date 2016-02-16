using UnityEngine;
using System.Collections;
using Helper;

public class InputManager : MonoBehaviour
{
	[Header("Components")]
	public StackManager stackManager;					// Access to stacks and animals
	public ThrowManager throwManager;					// Access to throwing

	[Header("Properties")]
	public bool useXboxController = false;		// Flag for whether to use Xbox Controller input
	public bool usePS4Controller = false;  // Flag for whether to use PS4 Controller input
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

	public KeyCode throwingWinKey = KeyCode.E;
	public KeyCode throwWinKey = KeyCode.Space;

	[Header("Xbox Keys")]
	public string animalMoveX = "XBOX_THUMBSTICK_LX";
	public string animalMoveY = "XBOX_THUMBSTICK_LY";

	public KeyCode animalXboxKey1 = KeyCode.Joystick1Button1;
	public KeyCode animalXboxKey2 = KeyCode.Joystick1Button2;
	public KeyCode animalXboxKey3 = KeyCode.Joystick1Button0;
	public KeyCode animalXboxKey4 = KeyCode.Joystick1Button3;

	public KeyCode animalPreviousXboxKey = KeyCode.Joystick1Button4;
	public KeyCode animalNextXboxKey = KeyCode.Joystick1Button5;

	[Header("PS4 Keys")]
	public string animalPS4MoveX = "PS4_THUMBSTICK_LX";
	public string animalPS4MoveY = "PS4_THUMBSTICK_LY";
	
	public KeyCode animalPS4Key1 = KeyCode.Joystick1Button0;
	public KeyCode animalPS4Key2 = KeyCode.Joystick1Button1;
	public KeyCode animalPS4Key3 = KeyCode.Joystick1Button2;
	public KeyCode animalPS4Key4 = KeyCode.Joystick1Button3;
	
	public KeyCode animalPreviousPS4Key = KeyCode.Joystick1Button4;
	public KeyCode animalNextPS4Key = KeyCode.Joystick1Button5;

	public bool throwingMode = false;


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
		if (controlledAnimal == null) {
			controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour> ();
		} else {
			// Handle input based on input type
			if (useXboxController) {
				HandleXboxInput ();
			} else if (usePS4Controller) {
				HandlePS4Input ();
			} else {
				HandleKeyboardInput ();
			}
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

		HandleModes ();

		if (!throwingMode) {
			// Moving Animals
			HandleAnimalControllerMovement ();
		}
	}

	private void HandlePS4Input()
	{
		// Setting Animals
		HandleAnimalSwitching(animalPS4Key1, 0); // X
		HandleAnimalSwitching(animalPS4Key2, 1); // Circle
		HandleAnimalSwitching(animalPS4Key3, 2); // Square
		HandleAnimalSwitching(animalPS4Key4, 3); // Triangle
		
		// Switching between stacks
		HandleAnimalSwitching(animalPreviousPS4Key, animalIndex-1); // LB
		HandleAnimalSwitching(animalNextPS4Key, animalIndex+1); // RB

		HandleModes ();

		if (!throwingMode) {
			// Moving Animals
			HandleAnimalControllerMovement ();
		}
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

		HandleModes ();

		if (!throwingMode) {
			// Moving Animals
			HandleAnimalKeyboardMovement ();
		} else {
			// Throwing Animals
			HandleAnimalKeyboardThrowing ();
		}
	}

	private void HandleAnimalSwitching(KeyCode key, int value)
	{
		// Check to make sure we do want to switch
		if(!Input.GetKeyDown(key))
			return;

		controlledAnimal.MoveStack(new Vector3(0,0,0));

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

		// Revert to movement mode upon animal change
		if (throwingMode) {
			throwingMode = false;
			throwManager.DeactivateThrowingMode ();
		}
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

	private void ControllerThrowCall ()
	{
		throwManager.CallThrow (controlledAnimal.GetAnimalAbove (), true);
		controlledAnimal = controlledAnimal.GetAnimalAbove().GetComponent<AnimalBehaviour>();
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

	public void HandleAnimalKeyboardThrowing ()
	{
		if (Input.GetKey (KeyCode.RightArrow)) {
			throwManager.RotateLeft ();
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			throwManager.RotateRight ();
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			throwManager.RotateUp ();
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			throwManager.RotateDown ();
		}
		
		if (Input.GetKeyDown (throwWinKey)) {
			throwManager.CallThrow (controlledAnimal.GetAnimalAbove (), false);
			throwingMode = false;
			controlledAnimal = controlledAnimal.GetAnimalAbove().GetComponent<AnimalBehaviour>();
		}
	}

	private void HandleModes ()
	{
		if (controlledAnimal.CanThrow ()) {
			if (useXboxController) {
				if (Input.GetAxis ("XBOX_TRIGGER_R") > 0.0f) {
					// Setup throwing mode
					if (throwingMode == false) {
						//Stop animal if moving
						controlledAnimal.currentVelocity = Vector3.zero;
						throwManager.ActivateThrowingMode (controlledAnimal);
					}
					throwManager.HandleCannonRotation ();
					throwingMode = true;
				} else {
					// Trigger released - throw
					if (throwingMode == true) {
						ControllerThrowCall ();
					}
					throwingMode = false;
				}
				
			} else if(usePS4Controller){
				if (Input.GetAxis ("PS4_TRIGGER_R") > 0.0f) {
					// Setup throwing mode
					if (throwingMode == false) {
						//Stop animal if moving
						controlledAnimal.currentVelocity = Vector3.zero;
						throwManager.ActivateThrowingMode (controlledAnimal);
					}
					throwManager.HandleCannonRotation ();
					throwingMode = true;
				} else {
					// Trigger released - throw
					if (throwingMode == true) {
						ControllerThrowCall ();
					}
					throwingMode = false;
				}
			} else {
				if (Input.GetKeyDown (throwingWinKey)) {
					throwingMode = !throwingMode;
					
					if (throwingMode) {
						throwManager.ActivateThrowingMode (controlledAnimal);
					} else {
						throwManager.DeactivateThrowingMode ();
					}
				}
			}
		}
	}

}
