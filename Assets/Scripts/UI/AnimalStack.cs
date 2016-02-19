using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a stack of animals, and holds information relating to those animals
public class AnimalStack
{
	private List<GameObject> animals;
	private bool hasShepherd;
	
	public AnimalStack()
	{
		animals = new List<GameObject> ();
	}

	public void Add(GameObject obj)
	{
		if(obj.tag == "Shepherd")
			hasShepherd = true;

		animals.Add(obj);
	}

	public void Remove(GameObject obj)
	{
		if(obj.tag == "Shepherd")
			hasShepherd = false;

		animals.Remove(obj);
	}

	public void Clear()
	{
		animals.Clear();
	}

	public bool Contains(GameObject obj)
	{
		return animals.Contains(obj);
	}

	public int GetIndex(GameObject obj)
	{
		return animals.IndexOf(obj);
	}

	public GameObject Get(int Index)
	{
		return animals[Index];
	}

	public bool HasShepherd()
	{
		return hasShepherd;
	}

	public GameObject[] GetAll()
	{
		return animals.ToArray();
	}

	public List<GameObject> GetList()
	{
		return animals;
	}

	public int GetSize()
	{
		return animals.ToArray().Length;
	}
}