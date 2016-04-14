using UnityEngine;
using System.Collections;

public class ColouredTile : MonoBehaviour
{
    [Header("Components")]
    public StackManager stackManager;
    private BoxCollider boxCollider;

    [Header("Properties")]
    public AnimalSpecies allowedSpecies;

    void Start()
    {
        if(!stackManager)
        {
            stackManager = Utility.GetComponentFromTag<StackManager>("StackManager");
        }

        // Access box collider
        boxCollider = GetComponent<BoxCollider>();

        // Loop through the animals and give permissions
        foreach (GameObject animal in stackManager.gameAnimals)
        {
            if (animal.GetComponent<AnimalBehaviour>().animalSpecies.Equals(allowedSpecies))
            {
                Physics.IgnoreCollision(boxCollider, animal.GetComponent<Collider>());
                Physics.IgnoreCollision(boxCollider, animal.GetComponent<AnimalBehaviour>().triggerBox.GetComponent<AnimalCollider>().boxCollider);
            }
        }
    }
}
