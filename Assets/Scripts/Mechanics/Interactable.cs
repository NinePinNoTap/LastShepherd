using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour
{
	[Header("Components")]
	public GameObject controlledObject;		// Access to the object to be moved
	public Vector3 objectChangeAmount;		// How much to alter the current position of the object by

	[Header("Transforms")]
	private Vector3 objectCurrentPosition;	// Object current position (self calculated)
	private Vector3 objectTargetPosition;	// Object target position (self calculated)

	[Header("Animation Properties")]
	public float animationTime = 2.0f;		// How long we want it to take
	public bool isActivated = false;		// Whether the box has been activated
	private float plateFrameTime = 0.0f;	// Counter for animating plate
	private float objectFrameTime = 0.0f;	// Counter for animating object
	
	void Start ()
	{
		// Store start and end positions of object
		objectCurrentPosition = controlledObject.transform.position;
		objectTargetPosition = objectCurrentPosition + objectChangeAmount;
	}

	void OnTriggerEnter(Collider collider)
	{
		if(isActivated)
			return;

		// Make sure we are activating with an animal
		if(collider.gameObject.tag == "Animal")
		{
			ActivatePressurePlate();

			Debug.Log ("Triggered!");
		}
	}


	private void ActivatePressurePlate()
	{
		isActivated = true;
		StartCoroutine(MoveObject());
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