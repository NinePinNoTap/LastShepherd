using UnityEngine;
using System.Collections;

public class PenguinBehaviour : AnimalBehaviour
{
    [Header("Penguin Behaviour")]
    public bool isMoving = false;

	void Start ()
    {
        if(!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    void FixedUpdate()
    {
        // Check if we are moving so we can stop if need be
        if(isMoving)
        {
            RaycastHit hit;

            // Raycast forward to see if we need to stop
            if(Physics.Raycast(transform.position, GetComponent<Rigidbody>().velocity.normalized, out hit, 0.52f + (moveSpeed * Time.deltaTime)))
            {
                if(hit.transform.tag.Equals("Wall"))
                {
                    Debug.Log("Hit wall");
                    isMoving = false;
                    GetComponent<Rigidbody>().velocity = currentVelocity = new Vector3(0,0,0);
                }
                else if(hit.transform.tag.Equals("Animal"))
                {
                    Debug.Log("Hit animal");
                    isMoving = false;
                }
            }
        }

        // Call parent fixed update
        base.FixedUpdate();
    }

    public override void MoveAnimal(Vector3 direction)
    {
        if(!canMove)
            return;

        if(isMoving)
        {
            return;
        }

        if(direction.Equals(Vector3.zero))
        {
            return;
        }

        if (stackIndex == 0)
        {
            currentVelocity = direction * moveSpeed;
            isMoving = true;
        }
        else
        {
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction);

            if(canMerge)
            {
                StartCoroutine(DisableMerge());
            }
        }
    }
}
