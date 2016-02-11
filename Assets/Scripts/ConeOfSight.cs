using UnityEngine;
using System.Collections;

public class ConeOfSight : MonoBehaviour
{
	[Header("Components")]
	public GuardPatrol guardController;

	[Header("Properties")]
	public float coneRadius = 5;
	public float coneMinWidth = 1;
	public float coneMaxWidth = 5;

	[Header("Rendering")]
	public Material material;
	private MeshFilter meshFilter;
	private Renderer meshRenderer;
	
	void Start ()
	{
		meshFilter = GetComponent<MeshFilter>();
		if(!guardController)
		{
			guardController = transform.parent.GetComponent<GuardPatrol>();
		}

		// Define radius from sight distance
		coneRadius = guardController.sightDistance;
		coneMaxWidth = coneRadius;

		// Get access to the renderer
		meshRenderer = GetComponent<Renderer>();

		CreateMesh();
	}

	private void CreateMesh()
	{
		// Create cone
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[]
		{
			new Vector3(-coneMinWidth/2, 0, 0),
			new Vector3(-coneMaxWidth/2, 0, coneRadius),
			new Vector3(coneMaxWidth/2, 0, coneRadius),
			new Vector3(coneMinWidth/2, 0, 0)
		};
		
		mesh.triangles = new int[]
		{
			0, 1, 2,
			0, 2, 3
		};
		
		mesh.uv = new Vector2[]
		{
			new Vector2(0, 0),
			new Vector2(0, 1),
			new Vector2(1, 1),
			new Vector2(1, 0)
		};
		
		mesh.normals = new Vector3[]
		{
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		
		meshFilter.mesh = mesh;
		meshRenderer.material = material;
	}
}