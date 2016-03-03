using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimalCollider : MonoBehaviour
{
    [Header("Components")]
    public List<GameObject> objInRange;
    public BoxCollider boxCollider;
    public GameObject objParent;
    public AnimalBehaviour animalBehaviour;
    public StackManager stackManager;

    [Header("Gameplay")]
    public float FOV = 135.0f;
    public float heightThreshold = 1.2f;
    public bool isGrounded = false;

    // Use this for initialization
    void Start()
    {
        // Set up the box collider
        boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.transform.localScale = new Vector3(1.5f, 1.1f ,1.5f);
        boxCollider.isTrigger = true;

        // Create list of objects in range
        objInRange = new List<GameObject>();

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
        boxCollider.enabled = true;
    }

    public void Disable()
    {
        objInRange.Clear();
        boxCollider.enabled = false;
    }
	
    //======================
    // Collision Monitoring
    //======================

    void OnTriggerEnter(Collider col)
    {
        if(!objInRange.Contains(col.gameObject))
        {
            objInRange.Add(col.gameObject);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if(objInRange.Contains(col.gameObject))
        {
            // We dont want to merge if we arent at the base
            if(animalBehaviour.animalIndex == 0)
            {            
                HandleCollision(col.gameObject);
            }
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
        //===============================
        // Check if object is beneath us
        //===============================

        if(CheckBelow(obj))
        {            
            if(obj.tag == "Animal")
            {
                // We don't want to merge if we cant
                if(!stackManager.canMerge)
                    return;
                
                Debug.Log("We fell on an animal!");
                
                if (!animalBehaviour.parentStack.Equals(obj.GetComponent<AnimalBehaviour>().GetParentStack()))
                {
                    AnimalStack colliderStack = obj.GetComponent<AnimalBehaviour>().GetParentStack();
                    
                    stackManager.MergeStack(colliderStack, animalBehaviour.parentStack, ExecutePosition.BOTTOM);
                    
                    StartCoroutine(animalBehaviour.DisableMovement());
                }
            }
            else if(obj.tag == "Tile")
            {
                Debug.Log("We are grounded!");
                isGrounded = true;
            }
        }
        else if(RaycastToTarget(obj))
        {
            if(obj.tag == "Animal")
            {
                Debug.Log("Animal is front");
                HandleStacking(obj);
            }
            else if(obj.tag == "Tile")
            {
                Debug.Log("Tile is front");
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

        Vector3 start = obj.transform.position;
        
        RaycastHit hit;

        // Raycast directly upwards by animal height
        if (Physics.Raycast(start, Vector3.up, out hit, animalBehaviour.animalHeight))
        {
            // If the raycast hits another tile above the tile, or the tile is taller than "animalHeight", not possible for animals to step up
            if ((obj == hit.transform.gameObject) || (hit.transform.gameObject.tag.Equals("Tile")))
            {
                return;
            }
        }
        
        // If possible to move onto tile, move all animals accordingly
        for (int j=0; j<animalBehaviour.parentStack.GetSize(); j++)
        {
            animalBehaviour.parentStack.Get(j).transform.position = (obj.transform.position + new Vector3(0.0f, animalBehaviour.animalHeight * (j + 1), 0.0f));
        }
        
        StartCoroutine(animalBehaviour.DisableMovement());
    }    
    
    //===============
    // Functionality
    //===============
    
    private bool CheckBelow(GameObject obj)
    {
        float bottomAnimal = objParent.transform.position.y - (animalBehaviour.animalHeight / 2);
        float minHeight = bottomAnimal - heightThreshold;

        if(obj.transform.position.y >= minHeight && obj.transform.position.y <= bottomAnimal)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool RaycastToTarget(GameObject obj)
    {
        // Calculate a direction vector between the guard and the animal
        Vector3 direction = obj.transform.position - objParent.transform.position;
        direction.y = 0.0f;
        
        // Calculate the angle between the guard and animal
        float angle = Vector3.Angle(direction, objParent.transform.forward) - 90;

        // Check if we are within the field of view
        if(angle < FOV * 0.5f)
        {
            RaycastHit hit;

            int layerMask = LayerMask.NameToLayer("Animal");

            // Create a ray between the guard position and animal position
            if(Physics.Raycast(objParent.transform.position, direction.normalized, out hit, 1.5f, 1 << layerMask))
            {
                // Check if we hit the player
                if(hit.collider.gameObject.tag == "Animal")
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}