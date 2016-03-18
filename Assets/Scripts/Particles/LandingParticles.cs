using UnityEngine;
using System.Collections;

public class LandingParticles : MonoBehaviour {

	public GameObject ExplosionBanana3;
	public GameObject Monkey;
	private GameObject cloneBananaBlast;
	public Rigidbody myRigidBody;

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.name == "Cube") 
		{
			myRigidBody.velocity = new Vector3 (0,0,0);
			bananaBlast ();
		}
	}

	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Monkey.transform.Translate(0, 5, 0);
		}
	}

	void bananaBlast () 
	{
		cloneBananaBlast = Instantiate (ExplosionBanana3, this.transform.position, Quaternion.identity) as GameObject;

		cloneBananaBlast.transform.Rotate (90, 0, 0);
		cloneBananaBlast.transform.Translate (0, 0, 1);

		Destroy (cloneBananaBlast, 2);
	}
}
