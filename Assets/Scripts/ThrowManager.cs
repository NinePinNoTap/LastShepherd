using UnityEngine;
using System.Collections;

public class ThrowManager : MonoBehaviour
{
	[Header("Components")]
	public StackManager stackManager;
	public GameObject targetObj;

	[Header("Properties")]
	public bool isThrowing;
	public float throwRadius = 3.0f;
	public float moveSpeed = 3.0f;

	void Start ()
	{
		Deactivate();
	}

	void Update ()
	{
		if(!isThrowing)
			return;

		targetObj.transform.Rotate(0, 3, 0);

		// Could maybe put input here but would think thats the point of InputManager class
	}

	public void MoveTarget(Vector3 moveAmount)
	{
		// Calculate the position we are trying to move to
		Vector3 newPos = targetObj.transform.position + (moveAmount * Time.deltaTime);

		// Find a suitable position
		FindSuitablePosition(ref newPos);

		// Find a new suitable height
		FindSuitableHeight(newPos);
	}
	
	private void FindSuitablePosition(ref Vector3 position)
	{
		// Check the distance between our position 
		float distance = Vector3.Distance(stackManager.currentAnimal.transform.position, position);
		
		// Check if its out of bounds
		if(distance > throwRadius)
		{
			// We aren't okay so calculate a direction vector
			Vector3 direction = (position - stackManager.currentAnimal.transform.position).normalized;
			
			// Calculate the next best position within the range
			position = stackManager.currentAnimal.transform.position + (direction * throwRadius);
		}

		return;
	}

	private void FindSuitableHeight(Vector3 position)
	{
		//
		// TODO--
		// BASICALLY HERE WE WANT TO CHECK IF THE NEW POSITION IS SAFE (HAS GROUND BENEATH)
		// IF NOT THEN WE WANT TO SLOWLY MOVE TOWARDS THE STACK UNTIL WE FIND GROUND
		// THEN SET POSITION TO THAT 
		//

		RaycastHit hit;

		// Find ground beneath
		if(Physics.Raycast(new Vector3(position.x, 100.0f, position.z), -Vector3.up, out hit))
		{
			// Put the object above it
			position.y = hit.point.y;
		}
		
		// Set final position
		targetObj.transform.position = position;
	}
	
	public bool CheckThrow()
	{
		// Return if theres collision beneath
		return Physics.Raycast(targetObj.transform.position, -Vector3.up);
	}

	public void Activate()
	{
		isThrowing = true;
		targetObj.SetActive(true);
		targetObj.transform.position = stackManager.currentAnimal.transform.position + (stackManager.currentAnimal.transform.forward * 2);
	}

	public void Deactivate()
	{
		isThrowing = false;
		targetObj.SetActive(false);
	}
}
