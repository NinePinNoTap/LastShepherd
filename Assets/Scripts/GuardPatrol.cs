using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Helper;

public class GuardPatrol : MonoBehaviour
{
	public enum GuardType { PATROL, LOOKOUT, STAND};

	[Header("AI Properties")]
	public GameObject[] Waypoints;				// List of wayWaypoints
	public GuardType guardType;
	private int destPoint = 0;					// Current ID
	private NavMeshAgent agent;					// Access to pathfinding agent
	private float waypointThreshold = 0.5f;		// Distance away from target before selecting a new one

	[Header("Advanced AI")]
	public float stopTime = 1.0f;
	public float sightDistance = 10.0f;
	public float FOV = 90.0f;
	public SphereCollider sphereCollider;
	public bool isStanding = false;
	public bool isLooking = false;
	private int direction = 1;

	[Header("Other")]
	public IList<GameObject> Animals;
	public GameManager gameManager;
	
	void Start ()
	{
		// Find the waypoints and arrange by name
		// This will be replaced later with manual insertion
		//Waypoints = GameObject.FindGameObjectsWithTag("Waypoint").ToList().OrderBy(obj=>obj.name).ToList();

		// Create a list of animals
		Animals = GameObject.FindGameObjectsWithTag("Animal").ToList();

		// Get access to gamemanager
		gameManager = GameObject.FindGameObjectWithTag("Controller").GetComponent<GameManager>();

		// Gain access to the navigation agent
		agent = GetComponent<NavMeshAgent>();

		// Access the sphere target
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = sightDistance;
	}

	void Update ()
	{
		if(!agent)
			return;

		// Stop once we reach our target
		switch(guardType)
		{
			case GuardType.PATROL:
				PatrolArea();
				break;

			case GuardType.LOOKOUT:
				SearchArea();
				break;

			case GuardType.STAND:
				// Nothing to do
				break;
		}
	}

	private void PatrolArea()
	{
		if(agent.remainingDistance == 0.0f && !isStanding)
		{
			// Flag we are standing
			isStanding = true;
			
			// Allow to stand
			StartCoroutine(Stand ());
		}
	}

	private void SearchArea()
	{
		if(!isLooking)
		{
			StartCoroutine(Look());
		}
	}

	void OnTriggerStay(Collider other)
	{
		// Dont progress if its not an animal
		if(other.tag != "Animal")
			return;

		// See if we can see the object inside the radius
		if(RaycastToTarget(other.gameObject))
		{
			// Process game over
			gameManager.DoGameOver();
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
	
	private void SetNextTarget()
	{
		// Returns if no waypoints have been set up
		if (Waypoints.Length == 0)
			return;
		
		// Update destination
		agent.destination = Waypoints[destPoint].transform.position;
		
		// Choose next destination, wrapping if need be
		destPoint = (destPoint + 1) % Waypoints.Length;
	}

	private IEnumerator Stand()
	{
		yield return new WaitForSeconds(stopTime);
		SetNextTarget();
		isStanding = false;
	}

	private IEnumerator Look()
	{
		isLooking = true;
		agent.Stop();

		while(true)
		{
			direction = (direction * -1) + 1;

			if(transform.rotation.eulerAngles.y == 0.0)
			{
				transform.Rotate(new Vector3(0, -90, 0));
			}
			else
			{
				transform.Rotate(new Vector3(0, 90, 0));
			}
			
			yield return new WaitForSeconds(stopTime);
			
			yield return null;
		}
	}
}
