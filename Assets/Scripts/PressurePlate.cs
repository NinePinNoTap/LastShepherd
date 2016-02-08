using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
	[Header("Components")]
	public GameObject pressurePlate;		// Access to the pressure plate
	public GameObject controlledObject;		// Access to the object to be moved

	[Header("Transforms")]
	private Vector3 plateCurrentPosition;	// Plate current position
	private Vector3 plateTargetPosition;	// Plate target position
	private Vector3 objectCurrentPosition;	// Object current position
	private Vector3 objectTargetPosition;	// Object target position

	[Header("Animation Properties")]
	public Transform targetLocation;		// Where we want to move the object to
	public float animationTime = 2.0f;		// How long we want it to take
	public bool isActivated = false;		// Whether the box has been activated
	private float plateFrameTime = 0.0f;	// Counter for animating plate
	private float objectFrameTime = 0.0f;	// Counter for animating object
	
	void Start ()
	{
		// Calculate final pressure plate transform -- TEMP
		plateCurrentPosition = pressurePlate.transform.position;
		plateTargetPosition = plateCurrentPosition - new Vector3(0.0f, 0.05f, 0.0f);

		// Store start and end positions of object
		objectCurrentPosition = controlledObject.transform.position;
		objectTargetPosition = targetLocation.position;
	}

	void OnTriggerEnter(Collider collider)
	{
		if(isActivated)
			return;

		Debug.Log ("Triggered!");

		// Make sure we are activating with an animal
		if(collider.gameObject.tag == "Animal")
		{
			ActivatePressurePlate();
		}
	}


	private void ActivatePressurePlate()
	{
		isActivated = true;
		StartCoroutine(MovePressurePlate());
		StartCoroutine(MoveObject());
	}

	private IEnumerator MovePressurePlate()
	{
		// THIS IS A TEMPORARY FUNCTION
		// THIS WILL BE SWITCHED OUT FOR ANIMATIONS

		while(true)
		{
			plateFrameTime += Time.deltaTime;
			pressurePlate.transform.position = Vector3.Lerp (plateCurrentPosition, plateTargetPosition, plateFrameTime / animationTime);
			if(pressurePlate.transform.position.Equals(plateTargetPosition))
			{
				break;
			}
			else
			{
				yield return null;
			}
		}

		yield return new WaitForEndOfFrame();
	}

	private IEnumerator MoveObject()
	{
		while(true)
		{
			objectFrameTime += Time.deltaTime;
			controlledObject.transform.position = Vector3.Lerp (objectCurrentPosition, objectTargetPosition, objectFrameTime / animationTime);
			if(controlledObject.transform.position.Equals(objectTargetPosition))
			{
				break;
			}
			else
			{
				yield return null;
			}
		}

		yield return new WaitForEndOfFrame();
	}
}