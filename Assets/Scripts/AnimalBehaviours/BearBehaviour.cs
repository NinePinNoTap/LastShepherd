using UnityEngine;
using System.Collections;

public class BearBehaviour : AnimalBehaviour
{
    [Header("Bear Behaviour")]
    public float attackRange = 2.0f;
    public SphereCollider sphereCollider;

    void Start()
    {
        // Create a sphere for the attack range
        sphereCollider.radius = attackRange;
    }
	
    void OnTriggerEnter(Collider col)
    {
        // Ignore self
        if(col.gameObject.Equals(this.gameObject))
        {
            return;
        }

        // Ignore triggers
        if(col.isTrigger)
            return;

        // Check for a guard
        if(col.gameObject.tag == "Guard")
        {
            // ROAR
            Debug.Log("ROAR - DIE!");
            Destroy(col.gameObject);
        }
    }
}

