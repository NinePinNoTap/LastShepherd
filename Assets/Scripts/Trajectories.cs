using UnityEngine;

public class Trajectories : MonoBehaviour
{
	// LineRenderer to display the trajectory's path
	public LineRenderer trajectoryPath;

	public float fireStrength = 25;
	public Color nextColor = Color.red;
	
	// Number of segments in the trajectory curve
	public int segmentCount = 20;
	// Length of trajectory curve
	public int pathLength = 30;
	
	// Length for each line segment
	public float segmentLength;

	// Place from which animals are thrown from
	public GameObject firingPoint;
	
	// closest GameObject hit by trajectory curve
	private Collider _hitObject;
	public Collider hitObject { get { return _hitObject; } }

	public LayerMask layerMask;

	// Light to shine upon targeted surface
	public Light spotlight;

	void Awake()
	{
		// Determine length of segments in trajectory curve
		segmentLength = (float) pathLength / segmentCount;
	}
	
	void FixedUpdate()
	{
		SimulatePath();
	}

	// Simulate the trajectory of a thrown animal
	public void SimulatePath()
	{
		Vector3[] segments = new Vector3[segmentCount];
		
		// The first line point is where the ball will fire from
		segments[0] = firingPoint.transform.position;
		
		// The initial velocity
		Vector3 segVelocity = firingPoint.transform.up * fireStrength * Time.deltaTime;
		
		// reset our hit object
		_hitObject = null;
		
		for (int i = 1; i < segmentCount; i++)
		{
			// Time it takes to traverse one segment of length segScale (careful if velocity is zero)
			float segTime = (segVelocity.sqrMagnitude != 0) ? segmentLength / segVelocity.magnitude : 0;
			
			// Add velocity from gravity for this segment's timestep
			segVelocity = segVelocity + Physics.gravity * segTime;
			
			// Check to see if we're going to hit a physics object
			RaycastHit hit;
			if (Physics.Raycast(segments[i - 1], segVelocity, out hit, segmentLength, layerMask))
			{
				// remember who we hit
				_hitObject = hit.collider;
				
				// set next position to the position where we hit the physics object
				segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
				// Set ending velocity to zero, since animals will stop there
				segVelocity = Vector3.zero;
				
				// Highlight hit object with spotlight
				spotlight.enabled = true;
				
				int spotlightIndex = (int) Mathf.Round((float)i/6 * 5);
				spotlight.range = Vector3.Magnitude(segments[spotlightIndex] - segments[i]) + 0.5f;
				
				spotlight.transform.position = segments[spotlightIndex];
				
				spotlight.transform.LookAt(segments[i]);
				
				
				/* Uncomment to simulate bouncing
				// correct ending velocity, since we didn't actually travel an entire segment
				segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
				// flip the velocity to simulate a bounce
				segVelocity = Vector3.Reflect(segVelocity, hit.normal);
				*/
				
				/*
				 * Here you could check if the object hit by the Raycast had some property - was 
				 * sticky, would cause the ball to explode, or was another ball in the air for 
				 * instance. You could then end the simulation by setting all further points to 
				 * this last point and then breaking this for loop.
				 */
			}
			// If our raycast hit no objects, then set the next position to the last one plus v*t
			else
			{
				segments[i] = segments[i - 1] + segVelocity * segTime;
			}
		}
		
		// At the end, apply our simulations to the LineRenderer
		
		// Set the colour of our path to the colour of the next ball
		Color startColor = nextColor;
		Color endColor = startColor;
		startColor.a = 1;
		endColor.a = 0;
		trajectoryPath.SetColors(startColor, endColor);
		
		trajectoryPath.SetVertexCount(segmentCount);
		for (int i = 0; i < segmentCount; i++)
			trajectoryPath.SetPosition(i, segments[i]);
	}
}