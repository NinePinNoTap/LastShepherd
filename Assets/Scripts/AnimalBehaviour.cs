using UnityEngine;
using System.Collections;

public enum AnimalColour
{
    WHITE,
    GREEN,
    BLUE
};

public class AnimalBehaviour : MonoBehaviour
{
    [Header("Components")]
    public StackManager stackManager;
    public AnimalStack parentStack;
    public int stackIndex;
    public LayerMask layerMask;
    public ObjectHighlighter objHighlighter;

    [Header("Properties")]
    public AnimalColour animalColour = AnimalColour.WHITE;
    public bool beingThrown;

    [Header("Stack Merging")]
    public float disableMergeDuration = 1.0f;
    public bool canMerge;
    public float animalHeight = 1.0f;

    [Header("Movement")]
    public bool isControllable;
    public float disableMoveDuration = 0.5f;
    public bool canMove;
    public float moveSpeed = 5.0f;
    public Vector3 currentVelocity;

    void Awake()
    {
        isControllable = false;
        beingThrown = false;
        canMerge = true;
        canMove = true;
    }

    void Start()
    {
        if(!stackManager)
        {
            stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();
        }
    }

    void Update()
    {

    }
    
    protected virtual void FixedUpdate()
    {
        if (IsGrounded())
        {
            beingThrown = false;
        }

        if (!beingThrown)
        {
            if (isControllable)
            {
                HandleCollision();
            }

            // Update velocity
            GetComponent<Rigidbody>().velocity = new Vector3(currentVelocity.x, GetComponent<Rigidbody>().velocity.y, currentVelocity.z);
        }

        // Force position
        if (stackIndex > 0)
        {
            Vector3 correctedPosition = parentStack.Get(0).gameObject.transform.position;
            correctedPosition.y += stackIndex * animalHeight;
            transform.position = correctedPosition;
        }
    }

    
    //===========================================================================
    // COLLISION AND MERGING
    //===========================================================================

    //
    // THIS NEEDS TO BE FIXED .. SHOULD NOT BE RAYCASTING 13 TIMES
    //
    protected void HandleCollision()
    {
        RaycastHit hit;
        RaycastHit hit2;
        RaycastHit bottom;

        for (int i = 0; i< 13; i++)
        {
            float x = animalHeight * 0.5f * Mathf.Cos(((360.0f / 13.0f) * i) * (Mathf.PI / 180.0f));
            float z = animalHeight * 0.5f * Mathf.Sin(((360.0f / 13.0f) * i) * (Mathf.PI / 180.0f));

            if (Physics.Raycast(this.gameObject.transform.position, new Vector3(x, 0.0f, z), out hit, animalHeight * 0.55f))
            {
                if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Animal") && canMerge)
                {
                    AnimalStack colliderStack = hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.TOP);

                    StartCoroutine(DisableMovement());
                }
                else if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Tile") && canMove)
                {
                    Vector3 start = hit.transform.position;
                    //start.y += animalHeight*0.5f;
                    
                    if (Physics.Raycast(start, Vector3.up, out hit2, animalHeight))
                    {
                        if ((hit.transform.gameObject != hit2.transform.gameObject) && (hit2.transform.gameObject.tag.Equals("Tile")))
                        {
                            continue;
                        }
                    }
                    
                    for (int j=0; j<parentStack.GetSize(); j++)
                    {
                        parentStack.Get(j).transform.position = (hit.transform.position + new Vector3(0.0f, animalHeight * (j + 1), 0.0f));
                    }

                    StartCoroutine(DisableMovement());
                }
                
                return;
            }
            else if (Physics.Raycast(this.gameObject.transform.position, new Vector3(x, z, 0.0f), out bottom, animalHeight * 0.55f))
            {
                if ((stackIndex == 0) && bottom.transform.gameObject.tag.Equals("Animal") && !(parentStack.Equals(bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack())))
                {
                    AnimalStack colliderStack = bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stackManager.MergeStack(colliderStack, parentStack, ExecutePosition.BOTTOM);
                }
            }
        }       
    }

    //===========================================================================
    // MOVEMENT
    //===========================================================================

    public virtual void MoveAnimal(Vector3 direction)
    {
        if(!canMove)
            return;

        // Check what position we are in the stack
        if (stackIndex == 0)
        {
            // Move normally
            currentVelocity = direction * moveSpeed;
        }
        else
        {
            // Split the stack
            stackManager.SplitStack(parentStack, stackManager.animalIndex, ExecutePosition.TOP, direction);

            // Start a cooldown so we dont auto merge again
            if(canMerge)
            {
                StartCoroutine(DisableMerge());
            }
        }
    }

    public void Stop()
    {
        foreach(GameObject obj in parentStack.GetList())
        {
            obj.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
    }

    //===========================================================================
    // CHECKS
    //===========================================================================

    public bool IsGrounded()
    {
        RaycastHit bottom;
        
        if (Physics.Raycast(this.gameObject.transform.position, -Vector3.up, out bottom, animalHeight * 0.51f, layerMask))
        {
            if (GetComponent<Rigidbody>().velocity.y == 0)
            {
                return true;
            }
        }
        
        return false;
    }

    //===========================================================================
    // ANIMAL CONTROL
    //===========================================================================

    public void Activate()
    {
        isControllable = true;
        objHighlighter.Toggle(true);
        GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
    }
    
    public void Deactivate()
    {
        isControllable = false;
        objHighlighter.Toggle(false);
        GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
    }
    
    protected IEnumerator DisableMerge()
    {
        canMerge = false;
        
        yield return new WaitForSeconds(disableMergeDuration);
        
        canMerge = true;
    }

    protected IEnumerator DisableMovement()
    {
        canMove = false;

        yield return new WaitForSeconds(disableMoveDuration);

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
        stackIndex = i; 
    }

    public bool CanThrow()
    {
        int stackSize = parentStack.GetSize();
        
        if (stackSize <= 1)
            return false;
        
        if (stackManager.animalIndex < stackSize - 1)
        {
            return true;
        } else
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