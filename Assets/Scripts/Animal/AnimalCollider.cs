using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimalCollider : MonoBehaviour
{
    [Header("Components")]
    public List<GameObject> objInRange =  new List<GameObject>(); // Added this functionality back for animals interacting with moving platforms for demo purposes
    public BoxCollider boxCollider;
    public GameObject objParent;
    public AnimalBehaviour animalBehaviour;
    public StackManager stackManager;

    [Header("Gameplay")]
    public float FOV = 135.0f;
    public float heightThreshold = 1.0f;
    public bool isActivated = false;

    // Use this for initialization
    void Awake()
    {
        // Make sure we have stack manager
        stackManager = Utility.GetComponentFromTag<StackManager>("StackManager");

        // Set up the box collider
        boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.transform.localScale = new Vector3(1.5f, 1.1f ,1.5f);
        boxCollider.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        boxCollider.isTrigger = true;

        // Get parent object
        objParent = transform.parent.gameObject;
        animalBehaviour = objParent.GetComponent<AnimalBehaviour>();

        // Start disabled
        Disable();
    }

    void Update()
    {
        Vector3 LeftRay = Quaternion.AngleAxis(-90 - (FOV/2), Vector3.up) * objParent.transform.forward;
        Vector3 ForwardRay = Quaternion.AngleAxis(-90, Vector3.up) * objParent.transform.forward;
        Vector3 RightRay = Quaternion.AngleAxis(-90 + (FOV/2), Vector3.up) * objParent.transform.forward;
        
        // Line of sight
        Debug.DrawRay(objParent.transform.position, LeftRay * 2);
        Debug.DrawRay(objParent.transform.position, RightRay * 2);
        Debug.DrawRay(objParent.transform.position, ForwardRay * 2);
    }

    //==================================
    // Active and Deactive the Collider 
    //==================================

    public void Enable()
    {
        objInRange.Clear();
        isActivated = true;
    }

    public void Disable()
    {
        objInRange.Clear();
        isActivated = false;
    }
	
    //======================
    // Collision Monitoring
    //======================

    void OnTriggerEnter(Collider col)
    {
        // Don't process collider boxes
        if(col.name == "Collider Box")
            return;

        if(!objInRange.Contains(col.gameObject))
        {
            objInRange.Add(col.gameObject);
        }

        HandleCollision(col.gameObject);
    }

	void OnTriggerStay(Collider col)
	{
		if(col.name == "Collider Box")
            return;

        // We only really want to process the special objects
        if(col.gameObject.tag == "Tile")
            return;

		// Make sure we are moving
		if((animalBehaviour.isMoving && animalBehaviour.isGrounded) || animalBehaviour.beingThrown)
		{
			// Handle collision again
			HandleCollision(col.gameObject);
		}
	}

    void OnTriggerExit(Collider col)
    {
        if(objInRange.Contains(col.gameObject))
        {
            objInRange.Remove(col.gameObject);
        }
    }

    //==========
    // Handlers
    //==========

    private void HandleCollision(GameObject obj)
    {
        if(animalBehaviour.animalIndex > 0)
            return;

        if(!animalBehaviour.isControllable)
            return;
        
        //===============================
        // Check if object is beneath us
        //===============================

        if(CheckBelow(obj))
        {   
            Debug.Log(obj.name + " is below us");
            if(obj.tag == "Animal")
            {
                Debug.Log(objParent.name + " hit " + obj.name + " below!");

                // We don't want to merge if we cant
                if(!stackManager.canMerge)
                    return;

                //if you hit another animal after falling down or throwing onto it, it has to execute the same functions as if it had been grounded
                if (!animalBehaviour.parentStack.Equals(obj.GetComponent<AnimalBehaviour>().GetParentStack()))
                {
                    AnimalStack colliderStack = obj.GetComponent<AnimalBehaviour>().GetParentStack();

                    animalBehaviour.canMove = true;
                    animalBehaviour.beingThrown = false;
                    Debug.Log("Grounded after throwing!");

                    // Reposition/update all animals above thrown animal once it's grounded
                    if(animalBehaviour.parentStack.GetSize()>1){
                        for(int i=1; i<animalBehaviour.parentStack.GetSize(); i++){
                            animalBehaviour.parentStack.Get(i).transform.position = animalBehaviour.parentStack.Get(0).gameObject.transform.position + animalBehaviour.parentStack.Get(i).GetComponent<AnimalBehaviour>().stackLocalPosition;
                            animalBehaviour.parentStack.Get(i).GetComponent<AnimalBehaviour>().canMove = false;
                            animalBehaviour.parentStack.Get(i).GetComponent<AnimalBehaviour>().isMoving = false;
                            animalBehaviour.parentStack.Get(i).GetComponent<AnimalBehaviour>().rigidBody.isKinematic = true;
                            // Enable collisions between bottom animal again
                            Physics.IgnoreCollision(animalBehaviour.parentStack.Get (0).GetComponent<Collider>(), animalBehaviour.parentStack.Get (i).GetComponent<Collider>(),false);
                        }
                    }

                    // Activate invisible walls once thrown animal has landed
                    GameObject.FindGameObjectWithTag("ThrowManager").GetComponent<ThrowManager>().invisibleWalls.SetActive(true);

                    // Reactivate all tile barriers upon thrown animals landing
                    GameObject[] tileBarriers = GameObject.FindGameObjectsWithTag ("TileBarrier");
                    for (int i=0; i<tileBarriers.Length; i++) {
                        tileBarriers[i].GetComponent<BoxCollider>().enabled = true;
                    }

                    stackManager.MergeStack(colliderStack, animalBehaviour.parentStack, ExecutePosition.BOTTOM);

                    StartCoroutine(animalBehaviour.DisableMovement());

                    Debug.Log(objParent.name + " fell onto " + obj.name);
                }
            }
        }
        else if(RaycastToTarget(obj) && isActivated)
        {
            Debug.Log(obj.tag);
            Debug.Log(obj.name + " is in front");
            if(obj.tag == "Animal")
            {
                HandleStacking(obj);
            }
            else if(obj.tag == "Step")
            {
                HandleSteppingUp(obj);
            }
        }
    }
    
    private void HandleStacking(GameObject obj)
    {
        // Can't progress if we cant merge anyway
        if(!stackManager.canMerge)
            return;

        AnimalStack colliderStack = obj.GetComponent<AnimalBehaviour>().GetParentStack();
        
        if (colliderStack != animalBehaviour.parentStack)
        {
            stackManager.MergeStack(colliderStack, animalBehaviour.parentStack, ExecutePosition.TOP);
            
            StartCoroutine(animalBehaviour.DisableMovement());

            animalBehaviour.Stop();

            Debug.Log("Stacks Merged!");
        }
    }
    
    private void HandleSteppingUp(GameObject obj)
    {
        // Can't progress if we cant move anyway
        if(!animalBehaviour.canMove)
            return;

		Debug.Log ("Call");
        
        // Calculate a direction vector between the animal (objParent) and the step (obj)
        Vector3 direction = obj.transform.position - objParent.transform.position;
        direction.y = 0.0f;

        // Calculate the angle between the step and the animal
        float angle = Vector3.Angle(direction, objParent.transform.forward) - 90;

        // Check if we are within the field of view
        if(angle < FOV * 0.5f)
        {
            RaycastHit hit;

            Debug.Log(obj.name + " within view");

            int layerMask = LayerMask.NameToLayer("Step");

            // Create a ray between the step and animal position
            if(Physics.Raycast(objParent.transform.position, direction.normalized, out hit, 1.5f, 1 << layerMask))
            {
                // Check if we hit the step
                if(hit.collider.gameObject.tag == "Step")
                {
                    // Raycast directly upwards by animal height
                    if (Physics.Raycast(obj.transform.position, Vector3.up, out hit, animalBehaviour.animalHeight))
                    {
                        // If the raycast hits another tile above the tile, or the tile is taller than "animalHeight", not possible for animals to step up
                        if ((obj == hit.transform.gameObject) || (hit.transform.gameObject.tag.Equals("Step")))
                        {
                            Debug.Log("Found " + hit.transform.gameObject.name + " above");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("Step Up : The Sequel");

                        Vector3 basePos = obj.transform.position;
                        basePos.y += obj.GetComponent<Collider>().bounds.extents.y / 2;
                        basePos.y += 0.1f;

                        // If possible to move onto tile, move all animals accordingly
                        for (int j=0; j<animalBehaviour.parentStack.GetSize(); j++)
                        {
                            animalBehaviour.parentStack.Get(j).transform.position = basePos + new Vector3(0.0f, animalBehaviour.animalHeight * j, 0.0f);
                        }

						// Disable stepping up
						StartCoroutine(animalBehaviour.DisableMovement());

                        return;
                    }
                }
                // Hit something other than a step
                else
                {
                    Debug.Log("Found : " + hit.transform.name);
                    return;
                }
            }
            else
            {
                Debug.Log("No collision found ");
            }
        }

        Debug.Log("Wrong - Outside View");

        StartCoroutine(animalBehaviour.DisableMovement());
    }    
    
    //===============
    // Functionality
    //===============
    
    private bool CheckBelow(GameObject obj)
    {
        if(obj.transform.position.y <= objParent.transform.position.y - (animalBehaviour.animalHeight-0.05f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Checks if object collided with is in front of animal
    private bool RaycastToTarget(GameObject obj)
    {
        // Calculate a direction vector between the animal and object
        Vector3 direction = obj.transform.position - objParent.transform.position;
        direction.y = 0.0f;

        // Calculate the angle between animal and object
        float angle = Quaternion.FromToRotation(direction, objParent.transform.forward).eulerAngles.y;
        angle -= 45;

        // Check if we are within the field of view
        if(angle < FOV * 0.5f)
        {
            RaycastHit hit;

            int layerMask = 1 << LayerMask.NameToLayer("Animal");
            layerMask |= 1 << LayerMask.NameToLayer("Step");

            // Create a ray between the animal position and the object position
            if(Physics.Raycast(objParent.transform.position, direction.normalized, out hit, 1.5f, layerMask))
            {
                return true;
            }
        }
        
        return false;
    }
}