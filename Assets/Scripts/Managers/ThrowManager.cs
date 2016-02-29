using UnityEngine;
using System.Collections;

public class ThrowManager : MonoBehaviour {
	
	public StackManager stacksManager;
	public Trajectories trajectories;
	public GameObject cannon;							// "Cannon" to aim animals
	
	public float smallAngle = 20;
	public float bigAngle = 35;
	
	public GameObject throwAnimal;
	public GameObject throwingAnimal;
	
	private bool fire = false;
	
	
	// Use this for initialization
	void Start () {

	}
	
	void FixedUpdate(){
		if (fire) {
			TossAnimal();
			fire = false;
		}
	}
	
	public void TossAnimal()
	{
		AnimalStack oldStack = throwAnimal.GetComponent<AnimalBehaviour> ().parentStack;
		AnimalStack newStack = new AnimalStack();
		
		for(int i = stacksManager.animalIndex+1; i < oldStack.GetSize(); i++)
		{
			newStack.Add(oldStack.Get(i));
			oldStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, (i-(stacksManager.animalIndex+1)));
		}
		
		oldStack.GetList ().RemoveRange(stacksManager.animalIndex+1, oldStack.GetSize()-(stacksManager.animalIndex+1));
		stacksManager.levelStacks.Add(newStack);
		
		stacksManager.UpdateSelectedAnimal (throwAnimal);
		
		throwAnimal.transform.position = trajectories.firingPoint.transform.position;
		throwAnimal.SetActive (true);
		throwAnimal.GetComponent<Rigidbody>().useGravity = true;
		throwAnimal.GetComponent<Rigidbody>().AddForce(trajectories.firingPoint.transform.up * trajectories.fireStrength * newStack.GetSize() * Time.deltaTime,ForceMode.Impulse);
		
		StartCoroutine (DisableAnimalCollisions ());
		
		throwAnimal.GetComponent<AnimalBehaviour> ().beingThrown = true;
		
		/* THIS WORKS
		// Firing point is the same as in simulation
		// Initial Velocity is the same as well

		Vector3 initialVel = trajectories.firingPoint.transform.up * trajectories.fireStrength * Time.deltaTime;

		throwAnimal = GameObject.Instantiate (throwAnimal);
		throwAnimal.transform.position = trajectories.firingPoint.transform.position;
		throwAnimal.SetActive (true);
		throwAnimal.GetComponent<Rigidbody>().useGravity = true;
		throwAnimal.GetComponent<Rigidbody>().AddForce(initialVel,ForceMode.Impulse);
		//throwAnimal.GetComponent<Rigidbody> ().velocity = initialVel;

		Physics.IgnoreCollision (throwAnimal.GetComponent<Collider>(), throwingAnimal.GetComponent<Collider>());

		Debug.Log ("INITIAL VELOCITY: " + initialVel);
		Debug.Log("Should be same: " + throwAnimal.GetComponent<Rigidbody>().velocity);

*/
		DeactivateThrowingMode ();
	}

	// Temporarily disables collisions between colliders of animal being thrown and animal doing the throwing
	protected IEnumerator DisableAnimalCollisions()
	{
		Physics.IgnoreCollision (throwAnimal.GetComponent<Collider> (), throwingAnimal.GetComponent<Collider> (), true);
		
		yield return new WaitForSeconds(0.1f);
		
		Physics.IgnoreCollision (throwAnimal.GetComponent<Collider> (), throwingAnimal.GetComponent<Collider> (), false);
	}
	
	public void ActivateThrowingMode(AnimalBehaviour controlledAnimal)
	{
		// Place cannon at animal and orient it properly
		cannon.transform.position = controlledAnimal.transform.position;
		cannon.transform.rotation = controlledAnimal.transform.rotation;
		trajectories.SimulatePath ();
		cannon.transform.parent.gameObject.SetActive (true);
	}
	
	public void DeactivateThrowingMode()
	{
		cannon.transform.rotation = Quaternion.Euler (Vector3.zero);
		cannon.transform.parent.gameObject.SetActive (false);
	}
	
	public void RotateRight()
	{
		cannon.transform.Rotate (new Vector3 (0, 1, 0), Space.World);
	}
	
	public void RotateLeft()
	{
		cannon.transform.Rotate (new Vector3 (0, -1, 0), Space.World);
	}
	
	public void RotateUp(){
		cannon.transform.Rotate (new Vector3 (-1, 0, 0), Space.Self);
	}
	
	public void RotateDown(){
		cannon.transform.Rotate (new Vector3 (1, 0, 0), Space.Self);
	}
	
	public bool CallThrow(GameObject throwingAnimal, bool usingController){
		if (usingController) {
			float x = Input.GetAxis ("XBOX_THUMBSTICK_RX");
			float y = Input.GetAxis ("XBOX_THUMBSTICK_RY");
			
			// Check not aiming straight up - if so, throw
			if (x != 0.0f || y != 0.0f) {
				fire = true;
				this.throwingAnimal = throwingAnimal;
				this.throwAnimal = throwingAnimal.GetComponent<AnimalBehaviour>().GetAnimalAbove();
				return true;
			}
			else{
				DeactivateThrowingMode();
				return false;
			}
		} else {
			fire = true;
			this.throwingAnimal = throwingAnimal;
			this.throwAnimal = throwingAnimal.GetComponent<AnimalBehaviour>().GetAnimalAbove();
			return true;
		}
	}
	
	public void HandleCannonRotation()
	{
		float x = Input.GetAxis ("XBOX_THUMBSTICK_RX");
		float y = Input.GetAxis ("XBOX_THUMBSTICK_RY");
		
		if (x != 0.0f || y != 0.0f) {
			// Rotation around y-axis
			float yAngle = Mathf.Atan2 (y, x) * Mathf.Rad2Deg;

			yAngle += 45;
			
			// Rotate around y-axis only
			cannon.transform.rotation = Quaternion.Euler (cannon.transform.rotation.eulerAngles.x, yAngle, cannon.transform.rotation.eulerAngles.z);
			
			// Distance of thumbstick away from centre
			float thumbstickDistance = Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(y,2));
			
			//SnappyRotation(thumbstickDistance, yAngle);
			SmoothRotation(thumbstickDistance, yAngle);
			
		} else {
			// Aim straight up when thumbstick released
			cannon.transform.rotation = Quaternion.Euler (0, cannon.transform.rotation.eulerAngles.y, cannon.transform.rotation.eulerAngles.z);
		}	
	}
	
	void SmoothRotation(float thumbstickDistance, float yAngle){
		
		float xRotation = thumbstickDistance * 12;
		
		cannon.transform.rotation = Quaternion.Euler (xRotation, yAngle, cannon.transform.rotation.eulerAngles.z);
	}
	
	
	void SnappyRotation(float thumbstickDistance, float yAngle){
		if (thumbstickDistance > 0.3f) {
			// 1 block
			if (thumbstickDistance < 0.6f) {
				cannon.transform.rotation = Quaternion.Euler (smallAngle, yAngle, cannon.transform.rotation.eulerAngles.z);
				//Debug.Log ("1 block");
			}
			// 2 blocks
			else {
				cannon.transform.rotation = Quaternion.Euler (bigAngle, yAngle, cannon.transform.rotation.eulerAngles.z);
				//Debug.Log ("2 blocks");
			}
		}
	}
}
