using UnityEngine;
using System.Collections;

public class ThrowManager : MonoBehaviour
{
    [Header("Components")]
    public StackManager stacksManager;
    public Trajectories trajectories;
    public GameObject cannon;                           // "Cannon" to aim animals
    public GameObject invisibleWalls;

    [Header("Properties")]
    public float smallAngle = 20;
    public float bigAngle = 35;
    public float throwRadius = 12;
    public GameObject throwAnimal;
    public GameObject throwingAnimal;
    private bool fire = false;
    
    // Use this for initialization
    void Start()
    {

    }
    
    void FixedUpdate()
    {
        if (fire)
        {
            TossAnimal();
            fire = false;
        }
    }
    
    public void TossAnimal()
    {
		// Deactivate all tile barriers for animals to be thrown through them
		GameObject[] tileBarriers = GameObject.FindGameObjectsWithTag ("TileBarrier");
		for (int i=0; i<tileBarriers.Length; i++) {
			tileBarriers[i].GetComponent<BoxCollider>().enabled = false;
		}


        AnimalStack oldStack = throwingAnimal.GetComponent<AnimalBehaviour>().parentStack;

		for (int i=0; i<oldStack.GetSize(); i++) {
			oldStack.Get(i).layer = LayerMask.NameToLayer("Animal");
		}
                
        // Update the current animal to the one being thrown
        stacksManager.UpdateSelectedAnimal(throwAnimal);

        // Split the stack
        stacksManager.SplitStack(oldStack, stacksManager.animalIndex, ExecutePosition.TOP, Vector3.zero);

        // Calculate fire strength - NO LONGER HAVE TO SCALE as collisions with animals above is ignored
        float fireStrength = trajectories.fireStrength;
        int stackSize = throwAnimal.GetComponent<AnimalBehaviour>().parentStack.GetSize();

		AnimalStack currentStack = stacksManager.currentStack;

		if(currentStack.GetSize()>1){
		// Ignore collisions with animals above
			for(int i=1; i<currentStack.GetSize(); i++){
				Physics.IgnoreCollision(currentStack.Get (0).GetComponent<Collider>(), currentStack.Get (i).GetComponent<Collider>(),true);
			}
		}

        // Throw the animal
        throwAnimal.transform.position = trajectories.firingPoint.transform.position;
        throwAnimal.GetComponent<AnimalBehaviour>().Stop();
        throwAnimal.GetComponent<Rigidbody>().useGravity = true;
        throwAnimal.GetComponent<Rigidbody>().AddForce(trajectories.firingPoint.transform.up * fireStrength * Time.deltaTime, ForceMode.Impulse);

        // Flag we are throwing
        throwAnimal.GetComponent<AnimalBehaviour>().beingThrown = true;
        throwAnimal.GetComponent<AnimalBehaviour>().canMove = false;

        // Stop merging
        stacksManager.DisableMerge();

        // Reset throwing mode
		cannon.transform.rotation = Quaternion.Euler(Vector3.zero);
		cannon.transform.parent.gameObject.SetActive(false);
		// Note: Cannot call DeactivateThrowingMode() as invisible walls must stay inactive as long as thrown animal is in flight

        // Disable animal collisions
        StartCoroutine(DisableAnimalCollisions());

        //Debug.Log("== Current Throw ==");
        //Debug.Log("   Throwing Animals - " + stackSize);
        //Debug.Log("   Throwing Animal - " + throwingAnimal.name);
        //Debug.Log("   Thrown Animal - " + throwAnimal.name);
    }

    // Temporarily disables collisions between colliders of animal being thrown and animal doing the throwing
    protected IEnumerator DisableAnimalCollisions()
    {
        Physics.IgnoreCollision(throwAnimal.GetComponent<Collider>(), throwingAnimal.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(throwAnimal.GetComponent<AnimalBehaviour>().triggerBox.GetComponent<AnimalCollider>().boxCollider, throwingAnimal.GetComponent<Collider>(), true);
        
        yield return new WaitForSeconds(0.1f);
        
        Physics.IgnoreCollision(throwAnimal.GetComponent<Collider>(), throwingAnimal.GetComponent<Collider>(), false);
        Physics.IgnoreCollision(throwAnimal.GetComponent<AnimalBehaviour>().triggerBox.GetComponent<AnimalCollider>().boxCollider, throwingAnimal.GetComponent<Collider>(), false);
    }
    
    public void ActivateThrowingMode(AnimalBehaviour controlledAnimal)
    {
		// Place cannon at animal and orient it properly
        cannon.transform.position = controlledAnimal.transform.position;
        cannon.transform.rotation = controlledAnimal.transform.rotation;
        
		this.throwingAnimal = controlledAnimal.gameObject;
		this.throwAnimal = throwingAnimal.GetComponent<AnimalBehaviour>().GetAnimalAbove();

		trajectories.animalBeingThrown = throwAnimal.GetComponent<AnimalBehaviour> ();
		trajectories.speciesBeingThrown = trajectories.animalBeingThrown.animalSpecies;

		trajectories.SimulatePath();
        cannon.transform.parent.gameObject.SetActive(true);

        invisibleWalls.SetActive(false);
    }
    
    public void DeactivateThrowingMode()
    {
        cannon.transform.rotation = Quaternion.Euler(Vector3.zero);
        cannon.transform.parent.gameObject.SetActive(false);
		invisibleWalls.SetActive(true);
		trajectories.speciesBeingThrown = AnimalSpecies.NONE;
    }
    
    public void RotateRight()
    {
        cannon.transform.Rotate(new Vector3(0, 1, 0), Space.World);
    }
    
    public void RotateLeft()
    {
        cannon.transform.Rotate(new Vector3(0, -1, 0), Space.World);
    }
    
    public void RotateUp()
    {
        cannon.transform.Rotate(new Vector3(-1, 0, 0), Space.Self);
    }
    
    public void RotateDown()
    {
        cannon.transform.Rotate(new Vector3(1, 0, 0), Space.Self);
    }
    
    public bool CallThrow(GameObject throwingAnimal, bool usingController)
    {
		// First check if can throw onto tile aimed at (if any)
		if(!trajectories.isThrowAllowed){
			DeactivateThrowingMode();
			return false;
		}

        if (usingController)
        {
            float x = Input.GetAxis("XBOX_THUMBSTICK_RX");
            float y = Input.GetAxis("XBOX_THUMBSTICK_RY");
            
            // Check not aiming straight up - if so, throw
            if (x != 0.0f || y != 0.0f)
            {
                fire = true;
                // Moved into Activate()
				//this.throwingAnimal = throwingAnimal;
                //this.throwAnimal = throwingAnimal.GetComponent<AnimalBehaviour>().GetAnimalAbove();
                return true;
            }
            else
            {
                DeactivateThrowingMode();
                return false;
            }
        }
        else
        {
            fire = true;
			// Moved into Activate()
			//this.throwingAnimal = throwingAnimal;
            //this.throwAnimal = throwingAnimal.GetComponent<AnimalBehaviour>().GetAnimalAbove();
            return true;
        }
    }
    
    public void HandleCannonRotation()
    {
        float x = Input.GetAxis("XBOX_THUMBSTICK_RX");
        float y = Input.GetAxis("XBOX_THUMBSTICK_RY");
        
        if (x != 0.0f || y != 0.0f)
        {
            // Rotation around y-axis
            float yAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

            yAngle += 45;
            
            // Rotate around y-axis only
            cannon.transform.rotation = Quaternion.Euler(cannon.transform.rotation.eulerAngles.x, yAngle, cannon.transform.rotation.eulerAngles.z);
            
            // Distance of thumbstick away from centre
            float thumbstickDistance = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
            
            //SnappyRotation(thumbstickDistance, yAngle);
            SmoothRotation(thumbstickDistance, yAngle);
            
        }
        else
        {
            // Aim straight up when thumbstick released
            cannon.transform.rotation = Quaternion.Euler(0, cannon.transform.rotation.eulerAngles.y, cannon.transform.rotation.eulerAngles.z);
        }   
    }
    
    void SmoothRotation(float thumbstickDistance, float yAngle)
    {
        float xRotation = thumbstickDistance * throwRadius;
        
        cannon.transform.rotation = Quaternion.Euler(xRotation, yAngle, cannon.transform.rotation.eulerAngles.z);
    }
    
    void SnappyRotation(float thumbstickDistance, float yAngle)
    {
        if (thumbstickDistance > 0.3f)
        {
            // 1 block
            if (thumbstickDistance < 0.6f)
            {
                cannon.transform.rotation = Quaternion.Euler(smallAngle, yAngle, cannon.transform.rotation.eulerAngles.z);
                //Debug.Log ("1 block");
            }
            // 2 blocks
            else
            {
                cannon.transform.rotation = Quaternion.Euler(bigAngle, yAngle, cannon.transform.rotation.eulerAngles.z);
                //Debug.Log ("2 blocks");
            }
        }
    }
}
