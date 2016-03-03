using UnityEngine;
using System.Collections;

public enum AnimalSpecies
{
    NONE,
    FROG,
    MONKEY,
    PENGUIN,
    TURTLE
}
;

public class AnimalBehaviour : MonoBehaviour
{
    [Header("Components")]
    public StackManager
        stackManager;
    public AnimalStack parentStack;
    public int animalIndex; // Index of animal in its stack
    public LayerMask layerMask;
    public ObjectHighlighter objHighlighter;
    public Rigidbody rigidBody;

    [Header("Game Mechanics")]
    public AnimalSpecies
        animalSpecies = AnimalSpecies.NONE;

    [Header("Stacking and Merging")]
    public float
        disableMergeDuration = 0.1f;
    public float animalHeight = 1.0f;
    public Vector3 stackLocalPosition;

    [Header("Flags")]
    public bool
        isControllable;
    public bool isMoving;
    public bool isGrounded;
    public bool canMerge;
    public bool canMove;
    public bool beingThrown;

    [Header("Movement")]
    public float
        disableMoveDuration = 0.5f;
    public float moveSpeed = 5.0f;
    public Vector3 currentVelocity;
    public Vector3 startPosition;

    [Header("Collision Detectors")]
    public GameObject
        frontCollider;
    public float colliderSize = 0.1f;
    public GameObject bottomCollider;

    protected void Awake()
    {
        // Default flags
        isControllable = false;
        isMoving = false;
        isGrounded = false;
        canMerge = true;
        canMove = true;
        beingThrown = false;
		
        // Get the height of the animal
        animalHeight = transform.localScale.y;

        // Create colliders
        bottomCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), bottomCollider.GetComponent<Collider>());
        bottomCollider.transform.localPosition = new Vector3(transform.position.x, transform.position.y - (animalHeight / 2), transform.position.z);
        bottomCollider.transform.parent = gameObject.transform;
        bottomCollider.transform.localScale = new Vector3(1, colliderSize, 1);
        bottomCollider.GetComponent<Collider>().isTrigger = true;
        bottomCollider.AddComponent<ColliderChecker>();
        bottomCollider.name = "Bottom Collider";
        bottomCollider.GetComponent<MeshRenderer>().enabled = false;

        frontCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), frontCollider.GetComponent<Collider>());
        Physics.IgnoreCollision(frontCollider.GetComponent<Collider>(), bottomCollider.GetComponent<Collider>());
        frontCollider.transform.localPosition = new Vector3(transform.position.x + (animalHeight + colliderSize) / 2, transform.position.y + 0.01f, transform.position.z);
        frontCollider.transform.parent = gameObject.transform;
        frontCollider.transform.localScale = new Vector3(colliderSize, 0.96f, 1); // Raise front collider off the ground
        frontCollider.GetComponent<Collider>().isTrigger = true;
        frontCollider.AddComponent<ColliderChecker>();
        frontCollider.name = "Front Collider";
        frontCollider.GetComponent<MeshRenderer>().enabled = false;

        int colliderLayer = LayerMask.NameToLayer("AnimalColliders");

        bottomCollider.layer = colliderLayer;
        frontCollider.layer = colliderLayer;
        Physics.IgnoreLayerCollision(colliderLayer, colliderLayer);

        startPosition = transform.position;
    }

    protected void Start()
    {
        // Ensure we have access to the stack manager
        if (!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }

        // Ensure we have access to the rigidbody
        if (!rigidBody)
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
        if (isControllable)
        {
            // Check if we can move
            if (canMove && !beingThrown)
            {
                // Check if we are moving
                if (isMoving)
                {
                    // Handle collisions with tiles and animals
                    HandleCollisions();
                }
                
                // Update the rigidbody velocity
                rigidBody.velocity = new Vector3(currentVelocity.x, rigidBody.velocity.y, currentVelocity.z);
            }
        }
        else
        {
            // Check if we are at the bottom of the stack
            if (animalIndex == 0)
            {
                // If the animal is on the ground but has gravity enabled
                if (isGrounded)
                {
                    // Disable gravity
                    rigidBody.useGravity = false;
                }
                else
                {
                    rigidBody.useGravity = true;
                }

                bottomCollider.SetActive(true);
            }
            else
            {
                // Animal is above another so just force its position
                transform.position = parentStack.Get(0).gameObject.transform.position + stackLocalPosition;
                bottomCollider.SetActive(false);
            }
        }
    }

    
    //===========================================================================
    // COLLISION AND MERGING
    //===========================================================================

    protected void HandleCollisions()
    {
        bool frontHasCollided = frontCollider.GetComponent<ColliderChecker>().hasCollided;

        // Check front collisions
        if (frontHasCollided)
        {
            GameObject collidedObject = frontCollider.GetComponent<ColliderChecker>().collidedObject;

            // Move onto animal stack
            if ((animalIndex == 0) && collidedObject.tag.Equals("Animal") && canMerge)
            {
                AnimalStack colliderStack = collidedObject.GetComponent<AnimalBehaviour>().GetParentStack();

                if (colliderStack != parentStack)
                {
                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.TOP);
					
                    StartCoroutine(DisableMovement());
					
                    Debug.Log("MERGED!");
                }

                // Reset front collider
                frontCollider.GetComponent<ColliderChecker>().hasCollided = false;
                frontCollider.GetComponent<ColliderChecker>().collidedObject = null;
            }
			// Move onto tile
			else if ((animalIndex == 0) && collidedObject.tag.Equals("Tile") && canMove)
            {
                Vector3 start = collidedObject.transform.position;

                RaycastHit hit;
                // Raycast directly upwards by animal height
                if (Physics.Raycast(start, Vector3.up, out hit, animalHeight))
                {
                    // If the raycast hits another tile above the tile, or the tile is taller than "animalHeight", not possible for animals to step up
                    if ((collidedObject == hit.transform.gameObject) || (hit.transform.gameObject.tag.Equals("Tile")))
                    {
                        return;
                    }
                }

                // If possible to move onto tile, move all animals accordingly
                for (int j=0; j<parentStack.GetSize(); j++)
                {
                    parentStack.Get(j).transform.position = (collidedObject.transform.position + new Vector3(0.0f, animalHeight * (j + 1), 0.0f));
                }
				
                StartCoroutine(DisableMovement());

                // Reset front collider
                frontCollider.GetComponent<ColliderChecker>().hasCollided = false;
                frontCollider.GetComponent<ColliderChecker>().collidedObject = null;
            }
        }

        bool bottomHasCollided = bottomCollider.GetComponent<ColliderChecker>().hasCollided;

        if (bottomHasCollided)
        {
            GameObject collidedObject = bottomCollider.GetComponent<ColliderChecker>().collidedObject;

            if ((animalIndex == 0) && collidedObject.tag.Equals("Animal") && canMerge)
            {
                Debug.Log("Merging from above!");
                if (!parentStack.Equals(collidedObject.GetComponent<AnimalBehaviour>().GetParentStack()))
                {
                    AnimalStack colliderStack = collidedObject.GetComponent<AnimalBehaviour>().GetParentStack();
					
                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.BOTTOM);

                    StartCoroutine(DisableMovement());
                }
            }
        }
    }

    //===========================================================================
    // MOVEMENT
    //===========================================================================

    public virtual void MoveAnimal(Vector3 direction)
    {
        if (!canMove || beingThrown)
            return;

        if (!direction.Equals(Vector3.zero))
        {
            // Rotate direction to correspond to viewing axis vs controller axis due to isometric projection
            direction = Quaternion.Euler(0, -45, 0) * direction;

            // Normalise direction
            direction.Normalize();

            // Calculate rotation that animal is to face
            float rotation = Mathf.DeltaAngle(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, Mathf.Atan2(0, 1) * Mathf.Rad2Deg);
            // Align rotation to camera view
            rotation -= 90;

            // Rotate animal
            //transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.x, -rotation, transform.rotation.z));
            parentStack.RotateStack(-rotation);
        }

        // Check what position we are in the stack
        if (animalIndex == 0)
        {
            // Move normally
            currentVelocity = direction * moveSpeed;
        }
        else
        {
            // Make sure we have a direction before splitting
            if (direction.Equals(Vector3.zero))
            {
                return;
            }

            // Get animal directly beneath animal leaving the stack
            GameObject animalBeneath = parentStack.Get(stackManager.animalIndex - 1);

            // Split the stack
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction);

            // Reset bottom collider
            bottomCollider.GetComponent<ColliderChecker>().hasCollided = false;
            bottomCollider.GetComponent<ColliderChecker>().collidedObject = null;

            // Start a cooldown so we dont auto merge again
            if (canMerge)
            {
                StartCoroutine(DisableMergeWithBeneath(animalBeneath));
            }
        }
    }

    public void Stop()
    {
        foreach (GameObject obj in parentStack.GetList())
        {
            obj.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    //===========================================================================
    // CHECKS
    //===========================================================================

    protected void GroundCheck()
    {
        // Check for off map
        if (transform.position.y < GameManager.FALL_THRESHOLD)
        {
            transform.position = startPosition + new Vector3(0.0f, 1.0f, 0.0f);
        }

        // We will never be grounded if we are being thrown upwards
        if (Mathf.Abs(rigidBody.velocity.y) > 0.1f && beingThrown)
        {
            isGrounded = false;
            return;
        }
		// Otherwise, check GroundChecker
		else
        {
            isGrounded = bottomCollider.GetComponent<ColliderChecker>().hasCollided;
        }
    }
    
    protected void MovingCheck()
    {
        // Return true if either x/z velocity is not 0
        if (rigidBody.velocity.x == 0 && rigidBody.velocity.z == 0)
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
        rigidBody.velocity = new Vector3(0, 0, 0);

        rigidBody.isKinematic = false;

        bottomCollider.SetActive(true);
    }
    
    public void Deactivate()
    {
        isControllable = false;
        objHighlighter.Toggle(false);
        rigidBody.velocity = new Vector3(0, 0, 0);

        // If animal is above others in a stack, disable isKinematic
        if (animalIndex > 0)
        {
            rigidBody.isKinematic = false;
        }
        else
        {
            rigidBody.isKinematic = true;
        }

        bottomCollider.SetActive(false);
    }
    
    // DisableMerge when animal is hopping off stack with animals beneath it
    // Disables collision between bottom and front colliders of controlled animal and animal previously beneath's collider, to prevent merging back
    protected IEnumerator DisableMergeWithBeneath(GameObject animalBeneath)
    {
        Physics.IgnoreCollision(bottomCollider.GetComponent<Collider>(), animalBeneath.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(frontCollider.GetComponent<Collider>(), animalBeneath.GetComponent<Collider>(), true);
        
        yield return new WaitForSeconds(disableMergeDuration);

        Physics.IgnoreCollision(bottomCollider.GetComponent<Collider>(), animalBeneath.GetComponent<Collider>(), false);
        Physics.IgnoreCollision(frontCollider.GetComponent<Collider>(), animalBeneath.GetComponent<Collider>(), false);
    }

    protected IEnumerator DisableMergeWithBeneath()
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
        animalIndex = i; 

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
            if (Mathf.Abs(GetComponent<Rigidbody>().velocity.y) < 0.1f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
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