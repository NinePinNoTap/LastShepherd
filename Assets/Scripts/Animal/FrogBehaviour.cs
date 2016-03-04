using UnityEngine;
using System.Collections;

public class FrogBehaviour : AnimalBehaviour
{
    [Header("Gecko Behaviour")]
    public bool onWall = false;

    void FixedUpdate()
    {        
        // Check if we are moving
        MovingCheck();

        // Check if we are grounded
        if (isGrounded)
        {
            // Unflag we are on the wall
            onWall = false;

            // Unflag we are being thrown
            beingThrown = false;
        }

        // Check if the animal is being controlled
        if(isControllable)
        {
            // Check if we can move
            if(canMove && !beingThrown)
            {
                // Check if we are moving
                if(isMoving)
                {                    
                    // Handle collisions with tiles and animals
                    //HandleCollisions();
                }
                
                // Update velocity
                rigidBody.velocity = currentVelocity;
            }
        }
        else
        {
            // Check if we are at the bottom of the stack
            if(animalIndex == 0)
            {
                // If the animal is on the ground but has gravity enabled
                if(isGrounded)
                {
                    // Disable gravity
                    rigidBody.useGravity = false;
                }
                else
                {
                    // Make sure we aren't on a wall
                    if(!onWall)
                    {
                        // Enable gravity
                        rigidBody.useGravity = true;
                    }
                }
            }
            else
            {
                // Animal is above another so just force its position
                transform.position = parentStack.Get(0).gameObject.transform.position + stackLocalPosition;
                rigidBody.velocity = Vector3.zero;
            }
        }
    }

    public override void MoveAnimal(Vector3 direction)
    {
        if(direction.Equals(Vector3.zero))
        {
            // Don't allow to be moved so it can act as a platform
            rigidBody.isKinematic = true;

            // Remove velocity
            currentVelocity = Vector3.zero;
            return;
        }
        
        // Allow to be moved
        rigidBody.isKinematic = false;

        if(!canMove)
            return;

        // Check what its doing
        if (animalIndex == 0)
        {
            // Check if there are others in the stack above
            if(parentStack.GetSize() > 1)
            {
				// Get animal directly beneath animal leaving the stack
				GameObject animalBeneath = parentStack.Get(stackManager.animalIndex-1);

                stackManager.SplitStack(parentStack, 1, ExecutePosition.BOTTOM, direction * moveSpeed * Time.deltaTime);
                if(stackManager.canMerge)
                {
                    stackManager.DisableMerge();
                }
            }
            HandleGeckoMovement(direction);
        }
        else
        {
            Debug.Log("MOVE GECKO!");
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction * moveSpeed * Time.deltaTime);

            if(stackManager.canMerge)
            {
                stackManager.DisableMerge();
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
                rigidBody.useGravity = false;

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

