using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a stack of animals, and holds information relating to those animals
public class AnimalStack
{
	public List<GameObject> animals;

	public AnimalStack(){
		animals = new List<GameObject> ();
	}
}