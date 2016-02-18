using UnityEngine;
using System.Collections;

public class ColouredTile : MonoBehaviour
{
	public AnimalColour allowedAnimalColour;
	public StackManager stackManager;

	private BoxCollider boxCollider;

	void Start ()
	{
		// Access stack manager
		stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();

		boxCollider = GetComponent<BoxCollider>();

		// Loop through the animals and give permissions
		foreach(GameObject animal in stackManager.gameAnimals)
		{
			if(animal.GetComponent<AnimalBehaviour>().animalColour.Equals(allowedAnimalColour))
			{
				Physics.IgnoreCollision(boxCollider, animal.GetComponent<CapsuleCollider>());
			}
		}
	}

	void Update ()
	{
	
	}
}
