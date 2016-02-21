using UnityEngine;
using System.Collections;

public enum AnimalSpecies
{
    NONE,
    BEAR,
    GECKO,
    PENGUIN,
    TURTLE
};

public class AnimalBehaviour : MonoBehaviour
{
    [Header("Components")]
    public StackManager stackManager;
    public AnimalStack parentStack;
    public int stackIndex;
    public LayerMask layerMask;
    public ObjectHighlighter objHighlighter;
    public Rigidbody rigidBody;

    [Header("Game Mechanics")]
    public AnimalSpecies animalSpecies = AnimalSpecies.NONE;

    [Header("Stacking and Merging")]
    public float disableMergeDuration = 1.0f;
    public float animalHeight = 1.0f;
    public Vector3 stackLocalPosition;

    [Header("Flags")]
    public bool isControllable;
    public bool isMoving;
    public bool isGrounded;
    public bool canMerge;
    public bool canMove;
    public bool beingThrown;

    [Header("Movement")]
    public float disableMoveDuration = 0.5f;
    public float moveSpeed = 5.0f;
    public Vector3 currentVelocity;

    protected void Awake()
    {
        isControllable = false;
        isMoving = false;
        isGrounded = false;
        canMerge = true;
        canMove = true;
        beingThrown = false;
    }

    protected void Start()
    {
        // Ensure we have access to the stack manager
        if(!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }

        // Ensure we have access to the rigidbody
        if(!rigidBody)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
    }

    protected virtual void FixedUpdate()
    {
        // Check if we are on the ground
        GroundCheck();

        // Check if we are moving
        MovingCheck();

        // Check if we were in throwing state
		if (isGrounded && beingThrown)
        {
            // Flag we aren't being thrown anymore
            beingThrown = false;

            Debug.Log("Can be thrown again!");
        }

        // Check if we are controllable
        if(isControllable)
        {
            // Check if we can move
            if(canMove && !beingThrown)
            {
                // Check if we are moving
                if(isMoving)
                {
                    // Handle collisions with tiles and animals
                    HandleCollision();
                }
                
                // Update the rigidbody velocity
                rigidBody.velocity = new Vector3(currentVelocity.x, rigidBody.velocity.y, currentVelocity.z);
            }

			// If the animal is on the ground but has gravity enabled
			if(isGrounded)
			{
				// Disable gravity
				rigidBody.useGravity = false;
			}
			else
			{
				rigidBody.useGravity = true;
			}
        }
        else
        {
            // Check if we are at the bottom of the stack
            if(stackIndex == 0)
            {
                // If the animal is on the ground but has gravity enabled
                if(isGrounded)
                {
                    // Disable gravity
                    rigidBody.useGravity = false;
                }
                else
                {
                    rigidBody.useGravity = true;
                }
            }
            else
            {
                // Animal is above another so just force its position
                transform.position = parentStack.Get(0).gameObject.transform.position + stackLocalPosition;
            }
        }
    }

    
    //===========================================================================
    // COLLISION AND MERGING
    //===========================================================================

    //
    // THIS NEEDS TO BE FIXED .. SHOULD NOT BE RAYCASTING 13 TIMES
    //
    protected void HandleCollision()
    {
        RaycastHit hit;
        RaycastHit hit2;
        RaycastHit bottom;

        for (int i = 0; i< 13; i++)
        {
            float x = animalHeight * 0.5f * Mathf.Cos(((360.0f / 13.0f) * i) * (Mathf.PI / 180.0f));
            float z = animalHeight * 0.5f * Mathf.Sin(((360.0f / 13.0f) * i) * (Mathf.PI / 180.0f));

            if (Physics.Raycast(this.gameObject.transform.position, new Vector3(x, 0.0f, z), out hit, animalHeight * 0.55f))
            {
                if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Animal") && canMerge)
                {
                    AnimalStack colliderStack = hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.TOP);

                    StartCoroutine(DisableMovement());

                    Debug.Log("MERGED!");
                }
                else if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Tile") && canMove)
                {
                    Vector3 start = hit.transform.position;
                    
                    if (Physics.Raycast(start, Vector3.up, out hit2, animalHeight))
                    {
                        if ((hit.transform.gameObject != hit2.transform.gameObject) && (hit2.transform.gameObject.tag.Equals("Tile")))
                        {
                            continue;
                        }
                    }
                    
                    for (int j=0; j<parentStack.GetSize(); j++)
                    {
                        parentStack.Get(j).transform.position = (hit.transform.position + new Vector3(0.0f, animalHeight * (j + 1), 0.0f));
                    }

                    StartCoroutine(DisableMovement());
                }
                
                return;
            }
            else if (Physics.Raycast(this.gameObject.transform.position, new Vector3(x, z, 0.0f), out bottom, animalHeight * 0.55f))
            {
                if ((stackIndex == 0) && bottom.transform.gameObject.tag.Equals("Animal") && !(parentStack.Equals(bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack())))
                {
                    AnimalStack colliderStack = bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.BOTTOM);
                }
            }
        }       
    }

    //===========================================================================
    // MOVEMENT
    //===========================================================================

    public virtual void MoveAnimal(Vector3 direction)
    {
        if(!canMove)
            return;

        // Check what position we are in the stack
        if (stackIndex == 0)
        {
            // Move normally
            currentVelocity = direction * moveSpeed;
        }
        else
        {
			// Make sure we have a direction before splitting
			if(direction.Equals(Vector3.zero))
			{
				return;
			}

            // Split the stack
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction);

            // Start a cooldown so we dont auto merge again
            if(canMerge)
            {
                StartCoroutine(DisableMerge());
            }
        }
    }

    public void Stop()
    {
        foreach(GameObject obj in parentStack.GetList())
        {
            obj.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
    }

    //===========================================================================
    // CHECKS
    //===========================================================================

    protected void GroundCheck()
    {
        RaycastHit hit;

        // We will never be grounded if we are being throw upwards
        if(rigidBody.velocity.y >= 0.0f && beingThrown)
        {
            isGrounded = false;
            return;
        }

        // Raycast downwards (ignoring NPC/PC) and see if we hit ground
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, animalHeight * 0.51f, layerMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    
    protected void MovingCheck()
    {
        // Return true if either x/z velocity is not 0
        if(rigidBody.velocity.x == 0 && rigidBody.velocity.z == 0)
        {
            // We are stationary
            isMoving = false;
        }
        else
        {
            // We are moving
            isMoving = true;
        }
    }

    //===========================================================================
    // ANIMAL CONTROL
    //===========================================================================

    public void Activate()
    {
        // Default flags
        isControllable = true;
        canMove = true;
        isMoving = false;

        // Highlight
        objHighlighter.Toggle(true);

        // Reset movement velocity
        rigidBody.velocity = new Vector3(0,0,0);
    }
    
    public void Deactivate()
    {
        isControllable = false;
        objHighlighter.Toggle(false);
        rigidBody.velocity = new Vector3(0,0,0);
    }
    
    protected IEnumerator DisableMerge()
    {
        canMerge = false;
        
        yield return new WaitForSeconds(disableMergeDuration);
        
        canMerge = true;
    }

    protected IEnumerator DisableMovement()
    {
        canMove = false;

        yield return new WaitForSeconds(disableMoveDuration);

        canMove = true;
    }

    //===========================================================================
    // GETTERS
    //===========================================================================

    public AnimalStack GetParentStack()
    {
        return parentStack;
    }
    
    public void SetParentStack(AnimalStack a, int i)
    {
        parentStack = a;
        stackIndex = i; 

        // Calculate position in stack
        stackLocalPosition = new Vector3(0, i * animalHeight, 0);
    }

    public bool CanThrow()
    {
        int stackSize = parentStack.GetSize();
        
        if (stackSize <= 1)
            return false;
        
        if (stackManager.animalIndex < stackSize - 1)
        {
			// Check animal is not falling
			if(Mathf.Abs(GetComponent<Rigidbody>().velocity.y)<0.1f){
				return true;
			}
			else return false;
        } else
        {
            return false;
        }
    }
    
    public GameObject GetAnimalAbove()
    {
        if (CanThrow())
        {
            return parentStack.Get(stackManager.animalIndex + 1);
        }
        else
        {
            return null;
        }
    }
}