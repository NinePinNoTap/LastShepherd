using UnityEngine;
using System.Collections;

public class ColouredTile : MonoBehaviour
{
	[Header("Components")]
	public StackManager stackManager;
	private BoxCollider boxCollider;

	[Header("Properties")]
	public AnimalSpecies allowedSpecies;

	void Start ()
	{
		// Access stack manager
		stackManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<StackManager>();

		// Access box collider
		boxCollider = GetComponent<BoxCollider>();

		// Loop through the animals and give permissions
		foreach(GameObject animal in stackManager.gameAnimals)
		{
			if(animal.GetComponent<AnimalBehaviour>().animalSpecies.Equals(allowedSpecies))
			{
				Physics.IgnoreCollision(boxCollider, animal.GetComponent<Collider>());
			}
		}
	}

	void Update ()
	{
	
	}

	void OnTriggerEnter()
	{
		// Implement CK suggestion
		// Check the animal that is going to be thrown
		// If that animals species doesnt match the correct one
		// Send a message back to the throw manager to not allow throwing and change colour of throwing
		// Otherwise send a message to make sure we can throw

		// If the coloured tile will always be thrown on
		// Give this gameobject a trigger collider
		// If this coloured tile will act as a barrier
		// Give it a normal collider (no trigger)
	}
}
