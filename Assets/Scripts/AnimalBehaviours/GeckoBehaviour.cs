using UnityEngine;
using System.Collections;

public class GeckoBehaviour : AnimalBehaviour
{
    [Header("Gecko Behaviour")]
    public bool onWall = false;

    void Start()
    {
        if(!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }
    }

    void FixedUpdate()
    {
        if (IsGrounded())
        {
            beingThrown = false;
            onWall = false;
        }

        if(!beingThrown)
        {
            if (isControllable)
            {
                HandleCollision();
            }
            
            // Update velocity
            GetComponent<Rigidbody>().velocity = currentVelocity;
        }

        // Force position
        if (stackIndex > 0)
        {
            Vector3 correctedPosition = parentStack.Get(0).gameObject.transform.position;
            correctedPosition.y += stackIndex * animalHeight;
            transform.position = correctedPosition;
        }
    }

    public override void MoveAnimal(Vector3 direction)
    {
        if(direction.Equals(Vector3.zero))
        {
            // Don't allow to be moved so it can act as a platform
            GetComponent<Rigidbody>().isKinematic = true;

            // Remove velocity
            currentVelocity = Vector3.zero;
            return;
        }
        
        // Allow to be moved
        GetComponent<Rigidbody>().isKinematic = false;

        if(!canMove)
            return;

        // Check what its doing
        if (stackIndex == 0)
        {
            // Check if there are others in the stack above
            if(parentStack.GetSize() > 1)
            {
                stackManager.SplitStack(parentStack, 1, ExecutePosition.BOTTOM, direction * moveSpeed * Time.deltaTime);
                if(canMerge)
                {
                    StartCoroutine(DisableMerge());
                }
            }
            HandleGeckoMovement(direction);
        }
        else
        {
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction * moveSpeed * Time.deltaTime);

            if(canMerge)
            {
                StartCoroutine(DisableMerge());
            }
        }
    }

    private void HandleGeckoMovement(Vector3 direction)
    {
        RaycastHit hit;

        // Raycast in the movement direction
        if(Physics.Raycast(transform.position, direction, out hit, 0.7f + (moveSpeed * Time.deltaTime)))
        {
            // Check what we hit
            if(hit.transform.tag == "Wall")
            {
                // Move up wall
                currentVelocity = new Vector3(0, 1, 0);
                currentVelocity *= moveSpeed;
                
                // Disable gravity so we can climb walls
                GetComponent<Rigidbody>().useGravity = false;

                // Flag we are on the wall
                onWall = true;
            }
            else
            {
                // DO SOMETHING IF WE COLLIDE WITH ANIMAL?
            }
        }
        else 
        {
            // Check if we are on the wall
            if(onWall)
            {
                // Raycast opposite direction to see if we are moving away from the wall
                if(Physics.Raycast(transform.position - new Vector3(0, animalHeight, 0), direction * -1, out hit, 0.7f + (moveSpeed * Time.deltaTime)))
                {
                    // Check what we hit
                    if(hit.transform.tag == "Wall")
                    {
                        // Move up wall
                        currentVelocity = new Vector3(0, -1, 0);
                        currentVelocity *= moveSpeed;
                        return;
                    }
                }
            }

            // Just move
            currentVelocity = direction * moveSpeed;
        }
    }
}

