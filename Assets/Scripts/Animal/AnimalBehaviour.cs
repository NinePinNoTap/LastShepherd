using UnityEngine;
using System.Collections;

public enum AnimalSpecies
{
    NONE,
    FROG,
    MONKEY,
    PENGUIN,
    TURTLE
};

public class AnimalBehaviour : MonoBehaviour
{
    [Header("Components")]
    public StackManager stackManager;
    public AnimalStack parentStack;
    public int animalIndex; // Index of animal in its stack
    public LayerMask layerMask;
    public Rigidbody rigidBody;

    [Header("Game Mechanics")]
    public AnimalSpecies animalSpecies = AnimalSpecies.NONE;

    [Header("Stacking and Merging")]
    public float animalHeight = 1.0f;
    public Vector3 stackLocalPosition;

    [Header("Flags")]
    public bool isControllable;
    public bool isMoving;
    public bool isGrounded;
    public bool canMove;
    public bool beingThrown;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float disableMoveDuration = 0.5f;
    public Vector3 currentVelocity;
    public Vector3 startPosition;

    [Header("Collision Detectors")]
    public GameObject triggerBox;

	[Header("Particle Systems")]
	public GameObject orbitParticleHandler;

    [Header("Audio")]
    public AudioClip walkAudio;

    protected void Awake()
    {
        if(!stackManager)
        {
            stackManager = Utility.GetComponentFromTag<StackManager>("StackManager");
        }

        // Default flags
        isControllable = false;
        isMoving = false;
        isGrounded = false;
        canMove = true;
        beingThrown = false;

        // Get the height of the animal
        animalHeight = transform.localScale.y;

        // Create a box
        triggerBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        triggerBox.name = "Collider Box";
        triggerBox.transform.SetParent(transform);
        triggerBox.transform.localPosition = Vector3.zero;
        triggerBox.AddComponent<AnimalCollider>();
        triggerBox.GetComponent<MeshRenderer>().enabled = false;

		// Assign particle orbiter
		if (!orbitParticleHandler)
        {
			orbitParticleHandler = GetComponentInChildren<OrbitScript>().transform.parent.gameObject;
		}

        // Ignore our own colliders
        Physics.IgnoreCollision(GetComponent<Collider>(), triggerBox.GetComponent<Collider>());

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
        
        triggerBox.GetComponent<AnimalCollider>().stackManager = stackManager;

		// Initially set orbiting particles to off
		orbitParticleHandler.SetActive (false);
    }

    protected virtual void FixedUpdate()
    { 
        // Check if we are moving
        MovingCheck();

        if(transform.position.y < GameManager.Instance.FALL_THRESHOLD)
        {
            transform.position = startPosition;
        }

        // Check if we are the base animal
        if(animalIndex == 0)
        {
            // Enable gravity
            rigidBody.useGravity = true;

            // Check if we are being thrown
            if(beingThrown)
            {
                // isGrounded will never be true if animals are not considered for "grounding" - results in thrown animals freezing when being thrown onto other animals
                if(isGrounded)
                {
                    canMove = true;
                    beingThrown = false;
                    Debug.Log("Grounded after throwing!");

                    // Reposition/update all animals above thrown animal once it's grounded
                    if(parentStack.GetSize()>1){
                        for(int i=1; i<parentStack.GetSize(); i++){
                            parentStack.Get(i).transform.position = parentStack.Get(0).gameObject.transform.position + parentStack.Get(i).GetComponent<AnimalBehaviour>().stackLocalPosition;
                            parentStack.Get(i).GetComponent<AnimalBehaviour>().canMove = false;
                            parentStack.Get(i).GetComponent<AnimalBehaviour>().isMoving = false;
                            parentStack.Get(i).GetComponent<AnimalBehaviour>().rigidBody.isKinematic = true;
							// Enable collisions between bottom animal again
							Physics.IgnoreCollision(parentStack.Get (0).GetComponent<Collider>(), parentStack.Get (i).GetComponent<Collider>(),false);
                        }
                    }

					// Activate invisible walls once thrown animal has landed
					GameObject.FindGameObjectWithTag("Controller").GetComponent<ThrowManager>().invisibleWalls.SetActive(true);

					// Reactivate all tile barriers upon thrown animals landing
					GameObject[] tileBarriers = GameObject.FindGameObjectsWithTag ("TileBarrier");
					for (int i=0; i<tileBarriers.Length; i++) {
						tileBarriers[i].GetComponent<BoxCollider>().enabled = true;
					}
                }
            }
            else
            {
                if(canMove)
                {
                    currentVelocity.y = rigidBody.velocity.y;

                    if(currentVelocity.y < -0.5f)
                    {
                        // Fall
                        currentVelocity = new Vector3(0, currentVelocity.y, 0);
                    }

                    // Update the rigidbody velocity
                    rigidBody.velocity = currentVelocity;
                }
            }
        }
        else
        {
            isGrounded = false;
            beingThrown = false;
            rigidBody.isKinematic = true;

            // Animal is above another so just force its position
            transform.position = parentStack.Get(0).gameObject.transform.position + stackLocalPosition;
        }
    }

    //===========================================================================
    // MOVEMENT
    //===========================================================================

    public virtual void MoveAnimal(Vector3 direction)
    {
        if (!canMove)
            return;

        if(beingThrown)
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

            // Start a cooldown so we dont auto merge again
            if (stackManager.canMerge)
            {
                stackManager.DisableMerge();
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

	void OnCollisionEnter(Collision col)
	{
		// Check if we collided with a tile
		if (col.gameObject.tag == "Tile") {
			isGrounded = true;
			rigidBody.velocity = Vector3.zero;
		}

	}

	bool ContactPointsBeneathAnimal(Collision col){
		ContactPoint[] contacts = col.contacts;
		for(int i=0; i<contacts.Length; i++){
			// 0.1 used as a small buffer
			if(contacts[i].point.y > transform.position.y - (animalHeight/2-0.1)){
				Debug.Log(col.gameObject.name + " is NOT beneath " + gameObject.name);
				return false;
			}
		}
		Debug.Log(col.gameObject.name + " is beneath " + gameObject.name);
		return true;
	}

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.name == "Tile")
        {
            isGrounded = false;
        }
    }
    
    protected void MovingCheck()
    {
        if(beingThrown)
            return;

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

        // Enable particles
		orbitParticleHandler.SetActive (true);

        // Reset movement velocity
        rigidBody.velocity = new Vector3(0, 0, 0);

        rigidBody.isKinematic = false;
        
        triggerBox.GetComponent<AnimalCollider>().Enable();
    }
    
    public void Deactivate()
    {
        // Default flags
        isControllable = false;
        canMove = false;
        isMoving = false;

        // Disable particles
		orbitParticleHandler.SetActive (false);

        // Reset velocity
        rigidBody.velocity = new Vector3(0, 0, 0);

        // If animal is above others in a stack, disable isKinematic
        rigidBody.isKinematic = true;

        triggerBox.GetComponent<AnimalCollider>().Disable();
    }

    public IEnumerator DisableMovement()
    {
		// Disable movement
        canMove = false;
		currentVelocity = Vector3.zero;
		rigidBody.velocity = Vector3.zero;
		isMoving = false;

        yield return new WaitForSeconds(disableMoveDuration);

		// Re-enable movement
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