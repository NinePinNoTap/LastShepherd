using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Helper;

public class GuardPatrol : MonoBehaviour
{
	[Header("AI Properties")]
	public GameObject[] Waypoints;					// List of wayWaypoints
	private int destPoint = 0;					// Current ID
	private NavMeshAgent agent;					// Access to pathfinding agent
	private float waypointThreshold = 0.5f;		// Distance away from target before selecting a new one

	[Header("Advanced AI")]
	public float stopTime = 1.0f;
	public float sightDistance = 10.0f;
	public float FOV = 90.0f;
	public SphereCollider sphereCollider;
	public bool isStanding = false;

	[Header("Other")]
	public IList<GameObject> Animals;

	[Header("Debugging")]
	public Vector3 LeftRay;
	public Vector3 RightRay;
	
	void Start ()
	{
		// Find the waypoints and arrange by name
		// This will be replaced later with manual insertion
		//Waypoints = GameObject.FindGameObjectsWithTag("Waypoint").ToList().OrderBy(obj=>obj.name).ToList();

		// Create a list of animals
		Animals = GameObject.FindGameObjectsWithTag("Animal").ToList();

		// Gain access to the navigation agent
		agent = GetComponent<NavMeshAgent>();

		// Access the sphere target
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = sightDistance;

		// Set initial target
		SetNextTarget();
	}

	void Update ()
	{
		// Stop once we reach our target
		if (agent)
		{
			if(agent.remainingDistance == 0.0f && !isStanding)
			{
				// Flag we are standing
				isStanding = true;

				// Allow to stand
				StartCoroutine(Stand ());
			}
		}

		UpdateRays();
	}

	void OnTriggerStay(Collider other)
	{
		// Dont progress if its not an animal
		if(other.tag != "Animal")
			return;

		// See if we can see the object inside the radius
		if(RaycastToTarget(other.gameObject))
		{
			Application.LoadLevel(Application.loadedLevel);
			// Set target
			//canSeePlayer = true;
			//animalTarget = other.gameObject;
		}
	}

	private bool RaycastToTarget(GameObject obj)
	{
		// Calculate a direction vector between the guard and the animal
		Vector3 direction = obj.transform.position - transform.position;
		
		// Calculate the angle between the guard and animal
		float angle = Vector3.Angle(direction, transform.forward);
		
		// Check if we are within the field of view
		if(angle < FOV * 0.5f)
		{
			RaycastHit hit;
			
			// Create a ray between the guard position and animal position
			if(Physics.Raycast(transform.position, direction.normalized, out hit, sightDistance))
			{
				// Check if we hit the player
				if(hit.collider.gameObject.tag == "Animal")
				{
					return true;
				}
			}
		}

		return false;
	}
		
	private void UpdateRays()
	{
		LeftRay = Quaternion.AngleAxis(-45, Vector3.up) * transform.forward;
		RightRay = Quaternion.AngleAxis(45, Vector3.up) * transform.forward;
		
		// Line of sight
		Debug.DrawRay(transform.position, LeftRay * sightDistance);
		Debug.DrawRay(transform.position, RightRay * sightDistance);
		Debug.DrawRay(transform.position, transform.forward * sightDistance);
	}
	
	private void SetNextTarget()
	{
		// Returns if no waypoints have been set up
		if (Waypoints.Length == 0)
			return;
		
		// Set the agent to go to the currently selected destination.
		agent.destination = Waypoints[destPoint].transform.position;
		
		// Choose the next point in the array as the destination,
		// cycling to the start if necessary.
		destPoint = (destPoint + 1) % Waypoints.Length;
	}

	private IEnumerator Stand()
	{
		yield return new WaitForSeconds(stopTime);
		SetNextTarget();
		isStanding = false;
	}
}
