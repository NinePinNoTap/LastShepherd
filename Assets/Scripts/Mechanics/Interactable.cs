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
// TODO: Unparent animal from moving blocks if animals fall off
public class Interactable : MonoBehaviour
{
    [Header("Objects")]
    public MoveableObject[] movingObjs;
    
    [Header("Animation Properties")]
    public float animationTime = 2.0f;      // How long we want it to take
    private float animationFrame = 0.0f;    // Counter for animating object
    public bool isActivated = false;        // Whether the box has been activated
    
    public List<GameObject> animalsOnBlocks;    // Animals stood on moving blocks when button is pressed
    public List<GameObject> animalControlObject; // Which controlObj each animal is on
    
    void Start ()
    {
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
		// Set obj move data
		for(int i = 0; i < movingObjs.Length; i++)
		{
			movingObjs[i].currentPosition = movingObjs[i].controlObj.transform.position;
			movingObjs[i].targetPosition = movingObjs[i].currentPosition + movingObjs[i].moveAmount;
			movingObjs[i].isFinished = false;
		}

        isActivated = true;
        
        // FINDS ANIMALS WHICH ARE ON MOVING BLOCKS
        animalsOnBlocks = new List<GameObject>();
        animalControlObject = new List<GameObject>();
        
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Animal");
        for (int i=0; i<animals.Length; i++)
        {
            for(int j=0;j<movingObjs.Length; j++){
                Collider[] childrenColliders = movingObjs[j].controlObj.GetComponentsInChildren<Collider>();
                for(int k=0; k<childrenColliders.Length; k++){
                    GameObject controlObjChild = childrenColliders[k].gameObject;
                    if(animals[i].GetComponentInChildren<AnimalCollider>().objInRange.Contains(controlObjChild)){
                        animalsOnBlocks.Add(animals[i]);
                        animalControlObject.Add(movingObjs[j].controlObj);
                        //Debug.Log(animalsOnBlocks[animalsOnBlocks.Count-1].name + " is on " + movingObjs[j].controlObj.name);
                        // Parent animal to moving block they are stood on, so they move along with it
                        animals[i].transform.parent = movingObjs[j].controlObj.transform;
                    }
                }
            }
        }
        
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
                    movingObjs[i].currentPosition = movingObjs[i].controlObj.transform.position;
                    
                    if(Vector3.Distance(movingObjs[i].currentPosition,movingObjs[i].targetPosition)<0.01f)
                    {
                        movingObjs[i].isFinished = true;
                        finishedObjs++;
                        // Unparent animals which were on moving block and remove them from future consideration
                        for(int j=0; j<animalControlObject.Count; j++){
                            if(animalControlObject[j].Equals(movingObjs[i].controlObj)){
                                animalsOnBlocks[j].transform.parent = GameObject.Find("Characters").transform;
                                animalsOnBlocks.RemoveAt(j);
                                animalControlObject.RemoveAt(j);
                            }
                        }
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