using UnityEngine;
using System.Collections;

public class PenguinBehaviour : AnimalBehaviour
{	
    [Header("Penguin Behaviour")]
    public float bumpPower = 10;

    void FixedUpdate()
    {
        // Check if we are on the ground
        GroundCheck();

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
            if(Physics.Raycast(transform.position, rigidBody.velocity.normalized, out hit, 0.55f, 1 << LayerMask.NameToLayer("Animal")))
            {
                // Stop moving
                isMoving = false;
                rigidBody.velocity = currentVelocity = new Vector3(0,0,0);

                // We need to bump the stack
                BumpStack(hit.transform.gameObject);
                
                Debug.Log("Hit animal");
            }
        }
        
        // Update the rigidbody velocity
        rigidBody.velocity = new Vector3(currentVelocity.x, rigidBody.velocity.y, currentVelocity.z);
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
        if (stackIndex == 0)
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

            // Disable merging again
            if(canMerge)
            {
                StartCoroutine(DisableMerge());
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
