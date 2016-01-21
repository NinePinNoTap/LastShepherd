using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// Material used for all animals
	public Material diffuseMat;

	// Count and display number of steps player takes
	public Text stepCountText;
	private int stepCount = 0;

	// Dimensions for the grid
	int gridColumnNumber = 12;
	int gridRowNumber = 12;
	// Side length of environment blocks in grid
	private float planeSideLength = 2.5f;

	// List containing stacks of animals. 
	// Note: An individual animal counts as its own stack
	private List<AnimalStack> stacks;
	public AnimalStack currentStack;
	public GameObject currentAnimal;
	public GameObject lastAnimal; // The previous animal selected by the player (used for highlighting)

	// Indexes of current stack/animal in their respective lists
	public int stackIndex;
	public int animalIndex;

	// Shaders
	private Shader diffuse; 	// Default shader for all animals
	private Shader outlined;	// Shader for highlighted animal

	// Number of stacks currently present in game (used for debugging)
	public int stackCount;

	// Default height of all animals
	private float animalHeight = 1.0f;
	
	void Start ()
	{
		CreateGrid ();

		// Create animals and assign them to their respective stacks
		AnimalStack stack1 = new AnimalStack ();
		GameObject animal1 = CreateAnimal ("Animal1", 2, 3, Color.yellow, stack1);
		GameObject shepherd = CreateShepherd ("Shepherd", 2, 3, Color.gray, stack1);

		AnimalStack stack2 = new AnimalStack ();
		GameObject animal2 = CreateAnimal ("Animal2", 9, 8, Color.green, stack2);

		AnimalStack stack3 = new AnimalStack ();
		GameObject animal3 = CreateAnimal ("Animal3", 5, 2, Color.red, stack3);

		AnimalStack stack4 = new AnimalStack ();
		GameObject animal4 = CreateAnimal ("Animal4", 1, 7, Color.magenta, stack4);

		// Add all stacks to the stacks list
		stacks = new List<AnimalStack> ();
		stacks.Add (stack1);
		stacks.Add (stack2);
		stacks.Add (stack3);
		stacks.Add (stack4);

		currentStack = stacks [0];

		stackIndex = 0;
		animalIndex = 0;
		
		currentAnimal = stacks [stackIndex].animals [animalIndex];

		// Set up shaders
		diffuse = Shader.Find ("Legacy Shaders/Diffuse Fast");
		outlined = Shader.Find ("Outlined/Silhouetted Diffuse");

		// Highlight first animal
		ChangeHighlight ();
	}

	void Update ()
	{
		HandleInput ();
		// Update counters
		stepCountText.text = "Step Count: " + stepCount.ToString ();
		stackCount = stacks.Count;
	}

	// Creates an animal at a certain grid location, with a certain colour (animal type), and stack which they belong to
	GameObject CreateAnimal(string animalName, int StartingX, int StartingZ, Color col, AnimalStack animalStack)
	{
		GameObject animal = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
		animal.name = animalName;
		animal.transform.localScale = new Vector3 (1.0f, 0.5f, 1.0f);
		animal.transform.localPosition = new Vector3 (StartingX * planeSideLength, animalHeight/2, StartingZ * planeSideLength);
		animal.GetComponent<Renderer> ().material = diffuseMat;
		animal.GetComponent<Renderer> ().material.color = col;

		animalStack.animals.Add (animal);

		return animal;
	}
	
	GameObject CreateShepherd(string shepherdName, int StartingX, int StartingZ, Color col, AnimalStack animalStack)
	{
		GameObject shepherd = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		shepherd.name = shepherdName;
		shepherd.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		shepherd.transform.localPosition = new Vector3 (StartingX * planeSideLength, animalHeight+animalHeight*0.5f, StartingZ * planeSideLength);
		shepherd.GetComponent<Renderer> ().material = diffuseMat;
		shepherd.GetComponent<Renderer> ().material.color = col;
		
		animalStack.animals.Add (shepherd);
		
		return shepherd;
	}
	
	// Creates grid consisting of planes with alternating white and black materials
	void CreateGrid()
	{
		GameObject Grid = new GameObject ();
		Grid.name = "Grid";
		
		for (int i=0; i<gridColumnNumber; i++) {
			
			for (int j=0; j<gridRowNumber; j++) {
				
				GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
				plane.name = "Plane " + j.ToString () + " , " + i.ToString ();
				plane.transform.localScale = new Vector3 (planeSideLength/10, 1.0f, planeSideLength/10);
				plane.transform.localPosition = new Vector3 (j * planeSideLength, 0.0f, i * planeSideLength);

				plane.GetComponent<Renderer> ().material = diffuseMat;
				
				if (i % 2 == 0) {
					if (j % 2 == 0) {
						plane.GetComponent<Renderer> ().material.color = Color.black;
					} else {
						plane.GetComponent<Renderer> ().material.color = Color.white;
					}
				} else {
					if (j % 2 == 0) {
						plane.GetComponent<Renderer> ().material.color = Color.white;
					} else {
						plane.GetComponent<Renderer> ().material.color = Color.black;
					}
				}
				
				plane.transform.SetParent (Grid.transform);
			}
		}
	}

	//
	void MoveStack(Vector3 movement){
		AnimalStack targetStack = null;

		// Check if stack will move onto another one
		for (int i=0; i<stacks.Count; i++) {
			if(!stacks[i].Equals(currentStack)){
				// If stack's future position intersects with an existing stack, combine them
				if(stacks[i].animals[0].transform.localPosition - currentStack.animals[0].transform.localPosition == movement){
					targetStack = stacks[i];
					Debug.Log("Stack will add to another stack.");
				}
			}
		}

		// Moving onto another stack
		if(targetStack!=null){
			Debug.Log("Moving onto other stack.");
			TransferAnimals(currentStack, targetStack, movement);
		}
		// Move onto ground
		else{
			// If bottom animal is the one moving, there is no need to make new stack
			if(animalIndex==0){
				Debug.Log("No need for new stack.");
				MoveAnimals(currentStack.animals, movement);
			}
			// Otherwise, create a new stack from the animals leaving previous stack
			else {
				Debug.Log("Making new stack.");
				AnimalStack newStack = new AnimalStack();
				stacks.Add(newStack);
				TransferAnimals(currentStack, newStack, movement);
			}
		}

		stepCount++;
	}

	// Transfers animals from one stack to another
	// Note: "Target" is a newly created List if animals are moving off a stack back to the ground
	public void TransferAnimals(AnimalStack source, AnimalStack target, Vector3 movement){
		List<GameObject> sourceStack = source.animals;
		List<GameObject> targetStack = target.animals;
		
		// Get height at which animals should be popped onto
		float movingHeight;
		// If moving onto floor, target stack is empty
		if (targetStack.Count == 0) {
			movingHeight = -animalHeight/2; // Subtract half of animal height to place animal on grid surface
		}
		// Moving onto another stack - get height of topmost animal to place moving animals
		else {
			if(targetStack[targetStack.Count-1].name.Equals("Shepherd"))
			{
				movingHeight = targetStack[targetStack.Count-2].transform.localPosition.y;
			}
			else
			{
				movingHeight = targetStack[targetStack.Count-1].transform.localPosition.y;
			}
			
		}

		// Place moving animals into a temporary list, and remove from previous stack
		List<GameObject> tempStack = new List<GameObject>(sourceStack.GetRange (animalIndex, sourceStack.Count-animalIndex));
		sourceStack.RemoveRange (animalIndex, sourceStack.Count-animalIndex);
		
		// Move animals to correct heights
		Vector3 newHeight = tempStack [0].transform.localPosition;
		for (int i=0; i<tempStack.Count; i++) {
			newHeight.y = movingHeight + 1.0f * (i+1);
			tempStack[i].transform.localPosition = newHeight;
		}
		// Move animals to correct x and z positions
		MoveAnimals(tempStack, movement);
		
		GameObject shepherdTemp = null;
		if((targetStack.Count > 0) && targetStack[targetStack.Count-1].name.Equals("Shepherd"))
		{
			shepherdTemp = targetStack[targetStack.Count-1];
			targetStack.RemoveAt(targetStack.Count-1);
		}
		// Add animals to the target stack
		targetStack.AddRange (tempStack);
		
		if(shepherdTemp != null)
		{	
			Vector3 shepherdHeight = targetStack[targetStack.Count-1].transform.localPosition;
			shepherdHeight.y += 1.0f; 
			shepherdTemp.transform.localPosition = shepherdHeight;
			targetStack.Add(shepherdTemp);
		}
		// Delete AnimalStack source if all animals from it have moved
		if (animalIndex == 0) {
			stacks.Remove(source);
		}
		// Update indexes to give player control to the bottom animal of the new stack
		animalIndex = 0;
		stackIndex = stacks.IndexOf (target);
		// Update current stacks and animals
		currentStack = target;

		// If adding to an existing stack, update the highlight
		if (movingHeight > 0.0f) {
			lastAnimal = currentAnimal;
			currentAnimal = currentStack.animals [animalIndex];
			ChangeHighlight ();
		}
	}

	// Updates all of a stack's animals' position values on the grid
	public void MoveAnimals(List<GameObject> animals, Vector3 movement){
		for (int i=0; i<animals.Count; i++){
			animals[i].transform.localPosition += movement;
		}
	}
	
	void HandleInput()
	{
		// Move current stack right
		if(Input.GetKeyDown(KeyCode.D)) {
			if(currentAnimal.transform.localPosition.x<11*planeSideLength){
				MoveStack(new Vector3(planeSideLength, 0.0f, 0.0f));
			}
		}
		// Move current stack left
		if(Input.GetKeyDown(KeyCode.A)) {
			if(currentAnimal.transform.localPosition.x>0){
				MoveStack(new Vector3(-planeSideLength, 0.0f, 0.0f));
			}
		}
		// Move current stack up
		if(Input.GetKeyDown(KeyCode.W)) {
			if(currentAnimal.transform.localPosition.z<11*planeSideLength){
				MoveStack(new Vector3(0.0f, 0.0f, planeSideLength));
			}
		}
		// Move current stack down
		if(Input.GetKeyDown(KeyCode.S)) {
			if(currentAnimal.transform.localPosition.z>0){
				MoveStack(new Vector3( 0.0f, 0.0f, -planeSideLength));
			}
		}

		// Switch current stack
		if(Input.GetKeyDown(KeyCode.Tab)) {
			SwitchAnimal();
		}

		// End game
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		// Restart level
		if(Input.GetKeyDown(KeyCode.R)) {
			Application.LoadLevel(0);
		}
	}

	void SwitchAnimal(){
		// Check if there are animals remaining in current stack
		if ((animalIndex < currentStack.animals.Count - 1) && !(currentStack.animals[animalIndex+1].name.Equals("Shepherd")) ) {
			animalIndex++;
			lastAnimal = currentAnimal;
			currentAnimal = currentStack.animals [animalIndex];
		} 
		// Otherwise, choose new stack
		else {
			// Start at beginning of next stack
			animalIndex = 0;
			// If at end of stacks list, return to beginning of list
			if(stackIndex==stacks.Count-1){
				stackIndex = 0;
			}
			// Otherwise, choose next stack in list
			else{
				stackIndex++;
			}

			lastAnimal = currentAnimal;
			currentStack = stacks[stackIndex];
			currentAnimal = currentStack.animals[animalIndex];
		}
		// Highlight currently selected animal
		ChangeHighlight ();
	}

	// Gives the current animal selected a highlight, and removes highlight from previously selected animal
	void ChangeHighlight(){
		// Highlight current animal
		currentAnimal.GetComponent<Renderer> ().material.shader = outlined;
		currentAnimal.GetComponent<Renderer> ().material.SetColor ("_OutlineColor", Color.cyan);
		currentAnimal.GetComponent<Renderer> ().material.SetFloat ("_Outline", 0.25f);

		// Remove highlight from previously selected animal
		if (lastAnimal != null) {
			lastAnimal.GetComponent<Renderer> ().material.shader = diffuse;
		}
	}

}
