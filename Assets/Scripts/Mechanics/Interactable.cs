using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct MoveableObject
{
    public string name;
    public GameObject controlObj;
    public Vector3 moveAmount;
    public Vector3 currentPosition;
    public Vector3 targetPosition;
    public bool isFinished;
}

public class Interactable : MonoBehaviour
{
	[Header("Objects")]
    public MoveableObject[] movingObjs;

	[Header("Animation Properties")]
	public float animationTime = 2.0f;		// How long we want it to take
    private float animationFrame = 0.0f;	// Counter for animating object
    public bool isActivated = false;        // Whether the box has been activated
	
	void Start ()
	{
        // Set obj move data
        for(int i = 0; i < movingObjs.Length; i++)
        {
            movingObjs[i].currentPosition = movingObjs[i].controlObj.transform.position;
            movingObjs[i].targetPosition = movingObjs[i].currentPosition + movingObjs[i].moveAmount;
            movingObjs[i].isFinished = false;
        }
	}

	void OnTriggerEnter(Collider collider)
	{
		if(isActivated)
			return;

		// Make sure we are activating with an animal
		if(collider.gameObject.tag == "Animal")
		{
			Activate();

			Debug.Log ("Triggered!");
		}
	}


	private void Activate()
	{
		isActivated = true;
		StartCoroutine(MoveObject());
	}

	private IEnumerator MoveObject()
	{
        int finishedObjs = 0;

		while(true)
		{
			animationFrame += Time.deltaTime;

            // Set obj move data
            for(int i = 0; i < movingObjs.Length; i++)
            {
                if(!movingObjs[i].isFinished)
                {
                    movingObjs[i].controlObj.transform.position = Vector3.Lerp (movingObjs[i].currentPosition, movingObjs[i].targetPosition, animationFrame / animationTime);
                    movingObjs[i].targetPosition = movingObjs[i].currentPosition + movingObjs[i].moveAmount;

                    if(movingObjs[i].currentPosition.Equals(movingObjs[i].targetPosition))
                    {
                        movingObjs[i].isFinished = true;
                        finishedObjs++;
                    }
                }
            }

            if(movingObjs.Length.Equals(finishedObjs))
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