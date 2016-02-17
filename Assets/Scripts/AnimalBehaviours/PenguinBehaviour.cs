using UnityEngine;
using System.Collections;

public class PenguinBehaviour : AnimalBehaviour
{
    [Header("Penguin Behaviour")]
    public bool isMoving = false;

	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    void FixedUpdate()
    {
        // Call parent fixed update
        base.FixedUpdate();

        if(isMoving)
        {
            RaycastHit hit;
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
                    // DO SOMETHING HERE
                }
            }
        }
    }

    public override void MoveAnimal(Vector3 direction)
    {
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
        }
        else
        {
            stacksManager.SplitStack(parentStack, stacksManager.animalIndex, direction);
        }

        isMoving = true;
    }
}
