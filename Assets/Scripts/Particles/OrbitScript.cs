using UnityEngine;
using System.Collections;

public class OrbitScript : MonoBehaviour {

	private Transform myTransform;
	private Quaternion angle;
	[SerializeField] float rotationSpeed;


	// Use this for initialization
	void Start () 
	{
//		myTransform = this.gameObject.GetComponent<Transform>();
//		angle = myTransform.rotation;
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
//		transform.Rotate(Vector3.up * Time.deltaTime, Space.World);
//		angle = new Quaternion (0, rotationSpeed * Time.deltaTime, 0, 0);
	}
}
