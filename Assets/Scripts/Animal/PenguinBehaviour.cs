using UnityEngine;
using System.Collections;

public class PenguinBehaviour : AnimalBehaviour
{	
    [Header("Penguin Behaviour")]
    public float bumpPower = 10;

    void FixedUpdate()
    {
        // Check if we were in throwing state
        if (isGrounded && beingThrown)
        {
            // Flag we aren't being thrown anymore
            beingThrown = false;
            
            Debug.Log("Can be thrown again!");
        }

        // Check if we are moving so we can stop if need be
        if(isMoving)
        {
            RaycastHit hit;

            // Raycast forward to see if we need to stop
            if(Physics.Raycast(transform.position, rigidBody.velocity.normalized, out hit, 0.55f))
            {
                // Stop moving
                isMoving = false;
                rigidBody.velocity = currentVelocity = new Vector3(0,0,0);

                if(hit.transform.tag == "Animal")
                {
                    Debug.Log("Hit animal");

                    // We need to bump the stack
                    BumpStack(hit.transform.gameObject);
                }
            }
        }

        // Update the rigidbody velocity
        rigidBody.velocity = new Vector3(currentVelocity.x, rigidBody.velocity.y, currentVelocity.z);

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
                // Enable gravity
                rigidBody.useGravity = true;
            }
        }
        else
        {
            // Animal is above another so just force its position
            transform.position = parentStack.Get(0).gameObject.transform.position + stackLocalPosition;
        }
    }

    public override void MoveAnimal(Vector3 direction)
    {
        // Make sure we can move
        if(!canMove)
            return;

        // Make sure we aren't already moving
        if(isMoving)
            return;

        // Make sure we have given it a direction
        if(direction.Equals(Vector3.zero))
            return;

        // If its at the base of the stack
        if (animalIndex == 0)
        {
            // Set movement velocity
            currentVelocity = direction * moveSpeed;

            // Flag we are now moving
            isMoving = true;
        }
        else
        {
            // Step off the stack
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction);

            currentVelocity = Vector3.zero;

            // Disable merging again
            if(stackManager.canMerge)
            {
                stackManager.DisableMerge();
            }
        }
    }

    private void BumpStack(GameObject bumpedObj)
    {
        // Get the animals stack
        AnimalStack stack = bumpedObj.GetComponent<AnimalBehaviour>().parentStack;

        for(int i = 0; i < stack.GetSize(); i++)
        {
            Rigidbody rb = stack.Get(i).GetComponent<Rigidbody>();
            Vector3 direction = stack.Get(i).transform.position - transform.position;
            direction.Normalize();
            rb.AddRelativeForce(direction * bumpPower, ForceMode.Impulse);
        }
    }
}
