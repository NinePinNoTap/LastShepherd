using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationMovementTest : MonoBehaviour
{
    public bool usePS4 = false;

    [Header("Win Input")]
    public KeyCode forwardWinKey = KeyCode.W;
    public KeyCode backWinKey = KeyCode.S;
    public KeyCode leftWinKey = KeyCode.A;
    public KeyCode rightWinKey = KeyCode.D;

    [Header("PS4 Input")]
    public string animalMoveX = "XBOX_THUMBSTICK_LX";
    public string animalMoveY = "XBOX_THUMBSTICK_LY";
    public string animalPS4MoveX = "PS4_THUMBSTICK_LX";
    public string animalPS4MoveY = "PS4_THUMBSTICK_LY";

    [Header("Movement")]
    public float movementSpeed = 5;
    public Animator animator;
    public Rigidbody rigidBody;

    [Header("Controllables")]
    public GameObject currentAnimal;
    public List<GameObject> animalList;

	void Start ()
    {
        currentAnimal = animalList[0];
        rigidBody = currentAnimal.GetComponent<Rigidbody>();
        animator = currentAnimal.GetComponent<Animator>();
        currentAnimal.transform.GetChild(0).gameObject.SetActive(true);
	}

	void Update ()
    {
        // Handle switching
        HandleAnimalSwitching();

        // Movement
        if(usePS4)
            HandleAnimalPS4Movement();
        else
            HandleAnimalWinMovement();

        // Check if we are moving
        bool isMoving = rigidBody.velocity.magnitude != 0;

        // Send info to animator
        animator.SetBool("isMoving", isMoving);

        if(isMoving)
        {
            Debug.Log("we are moving");
            Vector3 direction = rigidBody.velocity.normalized;

            float rotation = Mathf.DeltaAngle(Mathf.Atan2(direction.x, -direction.z) * Mathf.Rad2Deg, Mathf.Atan2(0, 1) * Mathf.Rad2Deg);
            rotation -= 90;
            currentAnimal.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
	}

    //
    // Movement
    //

    private void HandleAnimalWinMovement()
    {
        // Reset movement velocity
        Vector3 moveVelocity = new Vector3(0, 0, 0);

        // Handle input
        HandleInputDirection(forwardWinKey, ref moveVelocity, Vector3.forward);
        HandleInputDirection(backWinKey, ref moveVelocity, -Vector3.forward);
        HandleInputDirection(leftWinKey, ref moveVelocity, -Vector3.right);
        HandleInputDirection(rightWinKey, ref moveVelocity, Vector3.right);

        rigidBody.velocity = moveVelocity * movementSpeed;
    }

    private void HandleAnimalPS4Movement()
    {
        // Default move velocity
        Vector3 moveVelocity = new Vector3(0, 0, 0);

        // Check input
        HandleInputDirection(animalMoveX, ref moveVelocity, Vector3.right);
        HandleInputDirection(animalMoveY, ref moveVelocity, Vector3.forward);

        rigidBody.velocity = moveVelocity * movementSpeed;
    }

    //
    // Switching
    //

    private void HandleAnimalSwitching()
    {
        for(int i = 0; i < animalList.Count; i++)
        {
            string keyName = "Alpha" + (i+1).ToString();
            KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyName);

            if(Input.GetKeyDown( keyCode ))
            {
                // deactivate
                rigidBody.velocity = Vector3.zero;
                animator.SetBool("isMoving", false);
                currentAnimal.transform.GetChild(0).gameObject.SetActive(false);

                currentAnimal = animalList[i];
                rigidBody = currentAnimal.GetComponent<Rigidbody>();
                animator = currentAnimal.GetComponent<Animator>();
                currentAnimal.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    public void HandleInputDirection(KeyCode key, ref Vector3 vector, Vector3 value )
    {
        if (!Input.GetKey(key))
        {
            return;
        }

        // Increase vector values
        vector += value;
    }
   
    public void HandleInputDirection( string key, ref Vector3 vector, Vector3 value )
    {
        if (Input.GetAxis(key) == 0)
        {
            return;
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
