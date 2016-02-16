using UnityEngine;
using System.Collections;

public class ThrowingManager : MonoBehaviour {

	public StackManager stacksManager;
	public Trajectories trajectories;
	public GameObject cannon;							// "Cannon" to aim animals
	//private Quaternion cannonRotation;

	public GameObject animal;

	private bool fire = false;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate(){
		if (fire) {
			TossAnimal();
			fire = false;
		}
	}
	
	public void TossAnimal(){
		AnimalStack oldStack = animal.GetComponent<AnimalBehaviour> ().parentStack;
		AnimalStack newStack = new AnimalStack();
		
		for(int i = stacksManager.animalIndex+1; i < oldStack.GetSize(); i++)
		{
			newStack.Add(oldStack.Get(i));
			oldStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, (i-(stacksManager.animalIndex+1)));
		}

		oldStack.GetList ().RemoveRange(stacksManager.animalIndex+1, oldStack.GetSize()-(stacksManager.animalIndex+1));
		stacksManager.levelStacks.Add(newStack);

		stacksManager.UpdateSelectedAnimal (animal);

		animal.transform.position = trajectories.firingPoint.transform.position;
		animal.SetActive (true);
		animal.GetComponent<Rigidbody>().useGravity = true;
		animal.GetComponent<Rigidbody>().AddForce(trajectories.firingPoint.transform.up * trajectories.fireStrength * Time.deltaTime,ForceMode.Impulse);
		
		animal.GetComponent<AnimalBehaviour> ().beingThrown = true;

		DeactivateThrowingMode ();
	}

	public void ActivateThrowingMode(AnimalBehaviour controlledAnimal){
		cannon.transform.parent.gameObject.SetActive (true);
		// Place cannon at animal and orient it properly
		cannon.transform.position = controlledAnimal.transform.position;
		cannon.transform.rotation = controlledAnimal.transform.rotation;
	}

	public void DeactivateThrowingMode(){
		cannon.transform.rotation = Quaternion.Euler (Vector3.zero);
		cannon.transform.parent.gameObject.SetActive (false);
	}

	public void RotateRight(){
		cannon.transform.Rotate (new Vector3 (0, 1, 0), Space.World);
	}

	public void RotateLeft(){
		cannon.transform.Rotate (new Vector3 (0, -1, 0), Space.World);
	}

	public void RotateUp(){
		cannon.transform.Rotate (new Vector3 (-1, 0, 0), Space.Self);
	}

	public void RotateDown(){
		cannon.transform.Rotate (new Vector3 (1, 0, 0), Space.Self);
	}

	public void CallThrow(GameObject animalToThrow){
		fire = true;
		this.animal = animalToThrow;
	}
	
	public void HandleCannonRotation()
	{
		float x = Input.GetAxis ("XBOX_THUMBSTICK_RX");
		float y = Input.GetAxis ("XBOX_THUMBSTICK_RY");
		
		if (x != 0.0f || y != 0.0f) {
			// Rotation around y-axis
			float yAngle = Mathf.Atan2 (y, x) * Mathf.Rad2Deg + 45;
			
			// Rotate around y-axis only
			cannon.transform.rotation = Quaternion.Euler (cannon.transform.rotation.eulerAngles.x, yAngle, cannon.transform.rotation.eulerAngles.z);
			
			// Distance of thumbstick away from centre
			float thumbstickDistance = Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(y,2));
			
			//Debug.Log (thumbstickDistance);
			
			SnappyRotation(thumbstickDistance, yAngle);
			
		} else {
			// Aim straight up when thumbstick released
			cannon.transform.rotation = Quaternion.Euler (0, cannon.transform.rotation.eulerAngles.y, cannon.transform.rotation.eulerAngles.z);
		}	
	}

	
	void SnappyRotation(float thumbstickDistance, float yAngle){
		if (thumbstickDistance > 0.3f) {
			// 1 block
			if (thumbstickDistance < 0.6f) {
				cannon.transform.rotation = Quaternion.Euler (20, yAngle, cannon.transform.rotation.eulerAngles.z);
				Debug.Log ("1 block");
			}
			// 2 blocks
			else {
				cannon.transform.rotation = Quaternion.Euler (40, yAngle, cannon.transform.rotation.eulerAngles.z);
				Debug.Log ("2 blocks");
			}
		}
	}
}
