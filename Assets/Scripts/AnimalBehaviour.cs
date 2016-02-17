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
    public bool isControllable;
    public float moveSpeed = 5.0f;
    public StackManager stacksManager;
    public AnimalStack parentStack;
    public int stackIndex;
    public float animalHeight = 1.0f;
    public Vector3 currentVelocity;
    public ObjectHighlighter objHighlighter;
    public AnimalColour animalColour = AnimalColour.WHITE;
    public bool beingThrown;
    public LayerMask layerMask;

    void Awake()
    {
        isControllable = false;
        beingThrown = false;
    }

    void Update()
    {

    }
    
    void FixedUpdate()
    {
        if (beingThrown)
        {
            if (IsGrounded())
            {
                beingThrown = false;
            }
        }
        else
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

    //
    // THIS NEEDS TO BE FIXED .. SHOULD NOT BE RAYCASTING 13 TIMES
    //
    private void HandleCollision()
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
                if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Animal"))
                {
                    AnimalStack colliderStack = hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stacksManager.MergeStack(colliderStack, parentStack, MergePosition.TOP);
                } else if ((stackIndex == 0) && hit.transform.gameObject.tag.Equals("Tile"))
                {
                    Vector3 start = hit.transform.position;
                    //start.y += animalHeight*0.5f;
                    
                    if (Physics.Raycast(start, Vector3.up, out hit2, animalHeight))
                    {
                        if ((hit.transform.gameObject != hit2.transform.gameObject) && (hit2.transform.gameObject.tag.Equals("Tile")))
                        {
                            Debug.Log("HI");
                            continue;
                        }
                    }
                    
                    for (int j=0; j<parentStack.GetSize(); j++)
                    {
                        parentStack.Get(j).transform.position = (hit.transform.position + new Vector3(0.0f, animalHeight * (j + 1), 0.0f));
                    }
                }
                
                return;
            } else if (Physics.Raycast(this.gameObject.transform.position, new Vector3(x, z, 0.0f), out bottom, animalHeight * 0.55f))
            {
                if ((stackIndex == 0) && bottom.transform.gameObject.tag.Equals("Animal") && !(parentStack.Equals(bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack())))
                {
                    AnimalStack colliderStack = bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();

                    stacksManager.MergeStack(colliderStack, parentStack, MergePosition.BOTTOM);
                }
            }
        }       
    }

    public void MoveAnimal(Vector3 direction)
    {
        if (stackIndex == 0)
        {
            currentVelocity = direction * moveSpeed;
        }
        else
        {
            stacksManager.SplitStack(parentStack, stacksManager.animalIndex, direction);
        }
    }

    public void Stop()
    {
        foreach(GameObject obj in parentStack.GetList())
        {
            obj.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
    }

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
        
        if (stacksManager.animalIndex < stackSize - 1)
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
            return parentStack.Get(stacksManager.animalIndex + 1);
        }
        else
        {
            return null;
        }
    }
}