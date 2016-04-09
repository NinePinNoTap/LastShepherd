using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
    [Header("Components")]
    public StackManager
        stackManager;
    public ThrowManager throwManager;
    [Header("Properties")]
    public bool
        usePS4Controller = false;
    // Flag for whether to use PS4 Controller input
    private AnimalBehaviour controlledAnimal = null;
    // Access to the current controlled animal
    private int animalIndex;
    // Tracker for current animal index

    [Header("Windows Keys")]
    public KeyCode
        forwardWinKey = KeyCode.W;
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
    [Header("PS4 Keys")]
    public string
        animalPS4MoveX = "PS4_THUMBSTICK_LX";
    public string animalPS4MoveY = "PS4_THUMBSTICK_LY";
    public KeyCode animalPS4Square = KeyCode.Joystick1Button0;
    public KeyCode animalPS4X = KeyCode.Joystick1Button1;
    public KeyCode animalPS4Circle = KeyCode.Joystick1Button2;
    public KeyCode animalPS4Triangle = KeyCode.Joystick1Button3;
    public KeyCode animalPreviousPS4Key = KeyCode.Joystick1Button4;
    public KeyCode animalNextPS4Key = KeyCode.Joystick1Button5;
    public KeyCode animalPS4StartKey = KeyCode.Joystick1Button9;
    public bool throwingMode = false;

    void Start()
    {
         // Make sure we have stack manager
        if (!stackManager)
        {
            stackManager = Utility.GetComponentFromTag<StackManager>("StackManager");
        }

        // Make sure we have throw manager
        if (!throwManager)
        {
            throwManager = Utility.GetComponentFromTag<ThrowManager>("ThrowManager");
        }

        // Only use PS4 controller settings if we have an active controller
        if (Input.GetJoystickNames().Length > 0)
        {
            usePS4Controller = true;
            Debug.Log("Using PS4 Controller");
        }
        else
        {
            usePS4Controller = false;
            Debug.Log("Using Keyboard Input");
        }

        // Initialise to first animal
        animalIndex = 0;
    }

    void Update()
    {
        // Dont process if we are frozen
        if (Time.timeScale == 0)
            return;

        if (controlledAnimal == null)
        {
            controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();
        }
        else
        {
            // Handle input based on input type
            if (usePS4Controller)
            {
                HandlePS4Input();
            }
            else
            {
                HandleKeyboardInput();
            }
        }
    }

    //==============================================================================
    // WINDOWS Input
    //==============================================================================

    private void HandleKeyboardInput()
    {
        // Setting Animals
        HandleAnimalSwitching(animalWinKey1, 0);
        HandleAnimalSwitching(animalWinKey2, 1);
        HandleAnimalSwitching(animalWinKey3, 2);
        HandleAnimalSwitching(animalWinKey4, 3);
        
        // Switching between stacks
        HandleAnimalSwitching(animalPreviousWinKey, animalIndex - 1);
        HandleAnimalSwitching(animalNextWinKey, animalIndex + 1);
        
        HandleModes();
        
        if (!throwingMode)
        {
            // Moving Animals
            HandleAnimalKeyboardMovement();
        }
        else
        {
            // Throwing Animals
            HandleAnimalKeyboardThrowing();
        }
    }

    private void HandleAnimalSwitching(KeyCode key, int value)
    {
        // Check to make sure we do want to switch
        if (!Input.GetKeyDown(key))
            return;
        
        // Check animal is not being thrown or in mid-air
        if (!controlledAnimal.beingThrown && controlledAnimal.parentStack.Get(0).GetComponent<AnimalBehaviour>().isGrounded)
        {
            Debug.Log("Able to switch!");
            // Disable move velocity
            controlledAnimal.Stop();
        
            // Change index
            animalIndex = value;
        
            // Keep within the range
            Utility.Wrap(ref animalIndex, 0, stackManager.gameAnimals.Count - 1);
        
            // Tell the stack manager to update to the correct animal
            stackManager.UpdateSelectedAnimal(stackManager.gameAnimals [animalIndex]);
        
            // Update controlled animal
            controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();
        
            // Revert to movement mode upon animal change
            if (throwingMode)
            {
                throwingMode = false;
                throwManager.DeactivateThrowingMode();
            }
            return;
        }

        //look for the stack the controlled animals stack is currently on top of and remerge with it, before controlled animal is changed 
        for (int i =0; i<stackManager.levelStacks.Count; i++)
        {
            if (controlledAnimal.parentStack.Equals(stackManager.levelStacks [i]))
            {
                //Debug.Log("Index of Parentstack:"+i);
                continue;

            }

            //Debug.Log(controlledAnimal.rigidBody.velocity.y);
            //Debug.Log(Mathf.Abs(controlledAnimal.transform.position.x - stackManager.levelStacks[i].Get(0).transform.position.x));
            //Debug.Log(Mathf.Abs(controlledAnimal.transform.position.z - stackManager.levelStacks[i].Get(0).transform.position.z));

            //check if animal is not being thrown, falling and overlapping with another stack on the x- and z-plane
            if (!controlledAnimal.beingThrown && (controlledAnimal.rigidBody.velocity.y >= (-0.001f)) && (Mathf.Abs(controlledAnimal.transform.position.x - stackManager.levelStacks [i].Get(0).transform.position.x) <= (controlledAnimal.animalHeight + 0.2f)) && (Mathf.Abs(controlledAnimal.transform.position.z - stackManager.levelStacks [i].Get(0).transform.position.z) <= (controlledAnimal.animalHeight + 0.2f)))
            {
                //Debug.Log("Here");

                if (throwingMode)
                {
                    throwingMode = false;
                    throwManager.DeactivateThrowingMode();
                }

                controlledAnimal.Stop();


                stackManager.MergeStack(stackManager.levelStacks [i], controlledAnimal.parentStack, ExecutePosition.BOTTOM);

                StartCoroutine(controlledAnimal.DisableMovement());
                controlledAnimal.Stop();

                animalIndex = value;

                // Keep within the range
                Utility.Wrap(ref animalIndex, 0, stackManager.gameAnimals.Count - 1);

                // Tell the stack manager to update to the correct animal
                stackManager.UpdateSelectedAnimal(stackManager.gameAnimals [animalIndex]);

                // Update controlled animal
                controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();

                return;
            }
        }
    }

    private void HandleAnimalKeyboardMovement()
    {
        // Reset movement velocity
        Vector3 moveVelocity = new Vector3(0, 0, 0);

        // Check if we should override the move direction
        bool Override = controlledAnimal.animalIndex > 0;

        // Handle input
        HandleInputDirection(forwardWinKey, ref moveVelocity, -Vector3.forward, Override);
        HandleInputDirection(backWinKey, ref moveVelocity, Vector3.forward, Override);
        HandleInputDirection(leftWinKey, ref moveVelocity, Vector3.right, Override);
        HandleInputDirection(rightWinKey, ref moveVelocity, -Vector3.right, Override);

        // Process movement
        controlledAnimal.MoveAnimal(moveVelocity);
    }

    public void HandleAnimalKeyboardThrowing()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            throwManager.RotateRight();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            throwManager.RotateLeft();
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            throwManager.RotateDown();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            throwManager.RotateUp();
        }
        
        if (Input.GetKeyDown(throwWinKey))
        {
            if (throwManager.trajectories.isThrowAllowed)
            {
                if (throwManager.CallThrow(controlledAnimal.gameObject, false))
                {
                    // If throw was successful - change controlled animal
                    controlledAnimal = controlledAnimal.GetAnimalAbove().GetComponent<AnimalBehaviour>();
                }
                throwingMode = false;
            }
        }
    }


    //==============================================================================
    // PS4 Input
    //==============================================================================

    private void HandlePS4Input()
    {
        if (Input.GetKeyDown(animalPS4StartKey))
        {
            // Reload level
            Application.LoadLevel(Application.loadedLevel);
        }

        // Setting Animals
        HandleAnimalSwitching(animalPS4Square, 0); // Frog
        HandleAnimalSwitching(animalPS4X, 2); // Penguin
        HandleAnimalSwitching(animalPS4Circle, 1); // Monkey
        HandleAnimalSwitching(animalPS4Triangle, 3); // Turtle
        
        // Switching between stacks
        HandleAnimalSwitching(animalPreviousPS4Key, animalIndex - 1); // LB
        HandleAnimalSwitching(animalNextPS4Key, animalIndex + 1); // RB

        HandleModes();

        if (!throwingMode)
        {
            // Moving Animals
            HandleAnimalPS4Movement();
        }
    }

    private void HandleAnimalPS4Movement()
    {
        // Default move velocity
        Vector3 moveVelocity = new Vector3(0, 0, 0);

        // Check if its a single movemement we want or not
        bool Override = controlledAnimal.animalIndex > 0;

        // Check input
        HandleInputDirection(animalPS4MoveX, ref moveVelocity, Vector3.right, Override);
        HandleInputDirection(animalPS4MoveY, ref moveVelocity, Vector3.forward, Override);

        // Process movement
        controlledAnimal.MoveAnimal(moveVelocity);
    }

    //==============================================================================
    // Throwing Input
    //==============================================================================

    private void ControllerThrowCall()
    {
        if (throwManager.CallThrow(controlledAnimal.gameObject, true))
        {
            // If throw was successful - change controlled animal
            controlledAnimal = controlledAnimal.GetAnimalAbove().GetComponent<AnimalBehaviour>();
            animalIndex = stackManager.gameAnimals.IndexOf(controlledAnimal.gameObject);
        }
    }

    private void HandleModes()
    {
        if (!controlledAnimal.CanThrow())
        {
            return;
        }

        if (usePS4Controller)
        {
            if (Input.GetAxis("PS4_BUTTON_R2") > 0.0f)
            {
                // Setup throwing mode
                if (throwingMode == false)
                {
                    //if throwing mode is activated while the controlled animal has been moved a little on top of another stack without stepping off completely, remerge before activating throwing mode
                    for (int i =0; i<stackManager.levelStacks.Count; i++)
                    {
                        if (controlledAnimal.parentStack.Equals(stackManager.levelStacks [i]))
                            continue;

                        if (!controlledAnimal.beingThrown && (controlledAnimal.rigidBody.velocity.y >= (-0.001f)) && (Mathf.Abs(controlledAnimal.transform.position.x - stackManager.levelStacks [i].Get(0).transform.position.x) <= (controlledAnimal.animalHeight + 0.2f)) && (Mathf.Abs(controlledAnimal.transform.position.z - stackManager.levelStacks [i].Get(0).transform.position.z) <= (controlledAnimal.animalHeight + 0.2f)))
                        {
                            controlledAnimal.Stop();

                            stackManager.MergeStack(stackManager.levelStacks [i], controlledAnimal.parentStack, ExecutePosition.BOTTOM);

                            StartCoroutine(controlledAnimal.DisableMovement());
                            controlledAnimal.Stop();

                            // Tell the stack manager to update to the correct animal
                            stackManager.UpdateSelectedAnimal(stackManager.gameAnimals [animalIndex]);

                            // Update controlled animal
                            controlledAnimal = stackManager.currentAnimal.GetComponent<AnimalBehaviour>();


                        }
                    }

                    //Stop animal if moving
                    controlledAnimal.currentVelocity = Vector3.zero;
                    throwManager.ActivateThrowingMode(controlledAnimal);
                }
                throwManager.HandleCannonRotation();
                throwingMode = true;
            }
            else
            {
                // Trigger released - throw
                if (throwingMode == true)
                {
                    ControllerThrowCall();
                }
                throwingMode = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(throwingWinKey))
            {
                throwingMode = !throwingMode;

                if (throwingMode)
                {
                    throwManager.ActivateThrowingMode(controlledAnimal);
                    controlledAnimal.currentVelocity = Vector3.zero;
                }
                else
                {
                    throwManager.DeactivateThrowingMode();
                }
            }
        }
    }

    public void HandleInputDirection(KeyCode key, ref Vector3 vector, Vector3 value, bool Override)
    {
        if (!Input.GetKey(key))
        {
            return;
        }
        
        // Check if we want to override
        if (Override)
        {
            // Reset 
            vector = new Vector3(0, 0, 0);
        }
        
        // Increase vector values
        vector += value;
    }

    public void HandleInputDirection(string key, ref Vector3 vector, Vector3 value, bool Override)
    {
        if (Input.GetAxis(key) == 0)
        {
            return;
        }
        
        // Check if we want to override
        if (Override)
        {
            // Reset 
            vector = new Vector3(0, 0, 0);
        }

        if (Input.GetAxis(key) > 0)
        {
            // Increase vector values
            vector += value;
        }
        else if (Input.GetAxis(key) < 0)
        {
            // Increase vector values
            vector -= value;
        }
    }
}
