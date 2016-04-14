using UnityEngine;
using System.Collections;

public class animateBall : MonoBehaviour {

	//Variable declares, starting point to ending point
	public bool returnBob;

	public float speed;

	//Top of the wave
	public float bobCrest;


	//Bottom of the wave
	public float bobTrough;

	void Start() {
		returnBob = false;
	}

	void Update() {
		if (returnBob) {
			transform.position = new Vector3(transform.position.x, transform.position.y+speed, transform.position.z); 
			if(transform.position.y > bobCrest){
				returnBob = false;
			}
		} else {
			transform.position = new Vector3(transform.position.x, transform.position.y-speed, transform.position.z); 
			if(transform.position.y < bobTrough){
				returnBob = true;
			}
		}

	}
}