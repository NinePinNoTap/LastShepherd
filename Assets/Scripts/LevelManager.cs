﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
	public GameObject[] LevelTiles;
	public GameObject[] LevelAnimals;

	void Awake ()
	{
		IEnumerable<GameObject> Tiles;
		IEnumerable<GameObject> Animals;

		// Get a list of objects
		Tiles = GameObject.FindGameObjectsWithTag("Tile");
		Animals = GameObject.FindGameObjectsWithTag("Animal");

		// Sort by their position
		Tiles = Tiles.OrderBy(tile => GridRanking(tile.transform.position));
		Animals = Animals.OrderBy(animal => GridRanking(animal.transform.position));

		// Convert to an array
		LevelTiles = Tiles.ToArray();
		LevelAnimals = Animals.ToArray();
	}

	public int GetNumberOfAnimals()
	{
		// Returns the number of animals in the level
		return LevelAnimals.Length;
	}

	public GameObject GetAnimal(int Index)
	{
		// Returns a specific animal from the list
		return LevelAnimals[Index];
	}

	private float GridRanking(Vector3 pos)
	{
		return pos.x + (pos.z * 100);
	}
}