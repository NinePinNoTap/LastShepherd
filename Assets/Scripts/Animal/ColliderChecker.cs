using UnityEngine;
using System.Collections;

public class ColliderChecker : MonoBehaviour {
	public bool hasCollided;
	public GameObject collidedObject;

	void OnTriggerStay(Collider other){
		hasCollided = true;
		collidedObject = other.gameObject;
	}

	void OnTriggerExit(Collider other){
		hasCollided = false;
		collidedObject = null;
	}

	void FixedUpdate() {
		if (!hasCollided) {
			collidedObject = null;
		}
	}

}
