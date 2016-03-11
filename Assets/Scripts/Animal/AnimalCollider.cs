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
        // Set up the box collider
        boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.transform.localScale = new Vector3(1.5f, 1.1f ,1.5f);
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
        if(col.name == "Collider Box")
            return;

        if(!objInRange.Contains(col.gameObject))
        {
            objInRange.Add(col.gameObject);
        }

        HandleCollision(col.gameObject);
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
            if(obj.tag == "Animal")
            {
                Debug.Log(objParent.name + " hit " + obj.name + " below!");

                // We don't want to merge if we cant
                if(!stackManager.canMerge)
                    return;

                if (!animalBehaviour.parentStack.Equals(obj.GetComponent<AnimalBehaviour>().GetParentStack()))
                {
                    AnimalStack colliderStack = obj.GetComponent<AnimalBehaviour>().GetParentStack();
                    
                    stackManager.MergeStack(colliderStack, animalBehaviour.parentStack, ExecutePosition.BOTTOM);
                    
                    StartCoroutine(animalBehaviour.DisableMovement());

                    Debug.Log(objParent.name + " fell onto " + obj.name);
                }
            }
        }
        else if(RaycastToTarget(obj) && isActivated)
        {
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
        
        // Calculate a direction vector between the animl (objParent) and the step (obj)
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
                            Debug.Log("Found something above");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("Step Up : The Sequel");

                        Vector3 basePos = obj.transform.position;
                        basePos.y += obj.GetComponent<Collider>().bounds.extents.y / 2;
                        basePos.y += 0.1f; // Why do we need to add 0.1 as well?

                        // If possible to move onto tile, move all animals accordingly
                        for (int j=0; j<animalBehaviour.parentStack.GetSize(); j++)
                        {
                            animalBehaviour.parentStack.Get(j).transform.position = basePos + new Vector3(0.0f, animalBehaviour.animalHeight * j, 0.0f);
                        }
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

        StartCoroutine(animalBehaviour.DisableMovement()); // Do we need to disable movement?
    }    
    
    //===============
    // Functionality
    //===============
    
    private bool CheckBelow(GameObject obj)
    {
        // Should animalHeight not be halved here?
        if(obj.transform.position.y < objParent.transform.position.y - animalBehaviour.animalHeight)
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
        
        // Calculate the angle between the animal and object
        // OLD - Returns 0 when animals are facing away from each other - think this is due to Vector3.Angle producing acute angle
        //float angle = Vector3.Angle(direction, objParent.transform.forward) - 90;

        // New calculation of angle between animal and object
        float angle = Quaternion.FromToRotation(direction, objParent.transform.forward).eulerAngles.y;
        angle -= 45;

        //Debug.Log("angle between " + objParent.name + " and " + obj.name + " is: " + angle);

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
                // Check if we hit the player
                //if(hit.collider.gameObject.tag == "Animal")
                //{
                    //return true;
                //}
            }
        }
        
        return false;
    }
}