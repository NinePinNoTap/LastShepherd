using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// Material used for all animals
	public Material diffuseMat;
	// Materials for throwing ranges
	public Material throwYellowMat;
	public Material throwRedMat;

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

	// THROWING
	public bool throwingMode;
	public List<GameObject>[] throwingBoxes;
	public GameObject throwTarget;
	// Directions to throw in
	private enum Directions {Up, Right, Down, Left};
	
	void Start ()
	{
		CreateGrid ();

		// Create animals and assign them to their respective stacks
		AnimalStack stack1 = new AnimalStack ();
		GameObject animal1 = CreateAnimal ("Animal1", 2, 3, Color.yellow, stack1);

		AnimalStack stack2 = new AnimalStack ();
		GameObject animal2 = CreateAnimal ("Animal2", 9, 8, Color.green, stack2);

		AnimalStack stack3 = new AnimalStack ();
		GameObject animal3 = CreateAnimal ("Animal3", 5, 2, Color.red, stack3);

		AnimalStack stack4 = new AnimalStack ();
		GameObject animal4 = CreateAnimal ("Animal4", 1, 7, Color.blue, stack4);

		AnimalStack shepherdStack = new AnimalStack ();
		GameObject shepherd = CreateShepherd ("Shepherd", 6, 7, new Color(230.0f/255.0f, 160.0f/255.0f, 0.0f), shepherdStack);

		// Add all stacks to the stacks list
		stacks = new List<AnimalStack> ();
		stacks.Add (stack1);
		stacks.Add (stack2);
		stacks.Add (stack3);
		stacks.Add (stack4);
		stacks.Add (shepherdStack);

		currentStack = stacks [0];

		stackIndex = 0;
		animalIndex = 0;
		
		currentAnimal = stacks [stackIndex].animals [animalIndex];
		throwingMode = false;

		// Set up shaders
		diffuse = Shader.Find ("Legacy Shaders/Diffuse Fast");
		outlined = Shader.Find ("Outlined/Silhouetted Diffuse");

		// Highlight first animal
		UpdateHighlight ();

		// Set up throwing range lists
		throwingBoxes = new List<GameObject>[4];
		for(int i=0; i<4; i++){
			throwingBoxes[i] = new List<GameObject>();
		}
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
		shepherd.transform.localPosition = new Vector3 (StartingX * planeSideLength, animalHeight/2, StartingZ * planeSideLength);
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

	// Check if stack will move onto another one
	AnimalStack StackAtTarget(Vector3 movement){
		AnimalStack targetStack = null;
		
		for (int i=0; i<stacks.Count; i++) {
			if (!stacks [i].Equals (currentStack)) {
				// If stack's future position intersects with an existing stack, combine them
				if (stacks [i].animals [0].transform.localPosition - currentStack.animals [0].transform.localPosition == movement) {
					targetStack = stacks [i];
					//Debug.Log ("Stack will add to another stack.");
				}
			}
		}
		
		return targetStack;
	}

	// Moves animal stack
	void MoveStack(Vector3 movement){
		AnimalStack targetStack = StackAtTarget (movement);
		
		// Moving onto another stack
		if(targetStack!=null){
			//Debug.Log("Moving onto other stack.");
			TransferAnimals(targetStack, movement);
		}
		// Move onto ground
		else{
			// If bottom animal is the one moving, there is no need to make new stack
			if(animalIndex==0){
				//	Debug.Log("No need for new stack.");
				MoveAnimals(currentStack.animals, movement);
			}
			// Otherwise, create a new stack from the animals leaving previous stack
			else {
				//Debug.Log("Making new stack.");
				AnimalStack newStack = new AnimalStack();
				stacks.Add(newStack);
				TransferAnimals(newStack, movement);
			}
		}
		
		stepCount++;
	}

	// Transfers animals from one stack to another
	// Note: "Target" is a newly created List if animals are moving off a stack back to the ground
	public void TransferAnimals(AnimalStack target, Vector3 movement){
		List<GameObject> sourceStack = currentStack.animals;
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
				// If just shepherd in stack, animals go to floor beneath it
				if(targetStack.Count==1){
					movingHeight = -animalHeight/2;
				}
				// Otherwise move on top of topmost non-shepherd animal
				else{
					movingHeight = targetStack[targetStack.Count-2].transform.localPosition.y;
				}
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
		// If moving onto stack with shepherd on top, remove shepherd before adding animals
		if((targetStack.Count > 0) && targetStack[targetStack.Count-1].name.Equals("Shepherd"))
		{
			shepherdTemp = targetStack[targetStack.Count-1];
			targetStack.RemoveAt(targetStack.Count-1);
		}
		// Add animals to the target stack
		targetStack.AddRange (tempStack);

		// If shepherd was temporarily removed, add it back to the top of the stack
		if(shepherdTemp != null)
		{	
			Vector3 shepherdHeight = targetStack[targetStack.Count-1].transform.localPosition;
			shepherdHeight.y += animalHeight; 
			shepherdTemp.transform.localPosition = shepherdHeight;
			targetStack.Add(shepherdTemp);
		}
		// Remove currentStack from stacks list if all animals from it have moved
		if (animalIndex == 0) {
			stacks.Remove(currentStack);
		}
		
		// If only shepherd was thrown, give control back to animal which did the throwing
		if (currentAnimal.name.Equals ("Shepherd")) {
			currentAnimal = lastAnimal;
			lastAnimal = currentAnimal;
			animalIndex = currentStack.animals.IndexOf(currentAnimal);
		} else {
			// Control remains with animal added to stack
			animalIndex = targetStack.Count - tempStack.Count;
			// Remove 1 from index if shepherd present
			if (shepherdTemp != null) {
				animalIndex--;
			}
			stackIndex = stacks.IndexOf (target);
			// Update current stacks and animals
			currentStack = target;
		}

		tempStack.Clear ();
	}

	// Updates all of a stack's animals' position values on the grid
	public void MoveAnimals(List<GameObject> animals, Vector3 movement){
		for (int i=0; i<animals.Count; i++){
			animals[i].transform.localPosition += movement;
		}
	}
	
	public void ClearThrowingBoxes(){
		for(int i=0; i<throwingBoxes.Length; i++){
			for(int j=0; j<throwingBoxes[i].Count; j++){
				Destroy(throwingBoxes[i][j]);
			}
			throwingBoxes[i].Clear();
		}
		
		Destroy(GameObject.Find("Throwing Range Boxes"));
	}
	
	public void LoadThrowingRanges ()
	{
		GameObject throwingRanges = new GameObject ();
		throwingRanges.name = "Throwing Range Boxes";
		
		float startingX = currentAnimal.transform.localPosition.x;
		float startingZ = currentAnimal.transform.localPosition.z;
		
		// Right
		float boxX = startingX;
		for (int i=0; i<3; i++) {
			boxX += planeSideLength;
			if (boxX < gridColumnNumber * planeSideLength) {
				GameObject box = CreateThrowingBox(boxX, startingZ, throwingRanges);
				box.name = "Right " + i;
				throwingBoxes [(int)Directions.Right].Add (box);
			}
		}
		
		// Left
		boxX = startingX;
		for (int i=0; i<3; i++) {
			boxX -= planeSideLength;
			if (boxX >= 0) {
				GameObject box = CreateThrowingBox(boxX, startingZ, throwingRanges);
				box.name = "Left " + i;
				throwingBoxes [(int)Directions.Left].Add (box);
			}
		}
		
		// Up
		float boxZ = startingZ;
		for (int i=0; i<3; i++) {
			boxZ += planeSideLength;
			if (boxZ < gridColumnNumber*planeSideLength) {
				GameObject box = CreateThrowingBox(startingX, boxZ, throwingRanges);
				box.name = "Up " + i;
				throwingBoxes [(int)Directions.Up].Add (box);
			}
		}
		
		// Down
		boxZ = startingZ;
		for (int i=0; i<3; i++) {
			boxZ -= planeSideLength;
			if (boxZ >= 0) {
				GameObject box = CreateThrowingBox(startingX, boxZ, throwingRanges);
				box.name = "Down " + i;
				throwingBoxes [(int)Directions.Down].Add (box);
			}
		}
		
	}
	
	// Throwing boxes represent distances player can throw to
	public GameObject CreateThrowingBox(float boxX, float boxZ, GameObject throwingRanges){
		GameObject box = GameObject.CreatePrimitive (PrimitiveType.Cube);
		box.transform.localScale = new Vector3 (planeSideLength, 0.1f, planeSideLength);
		box.transform.localPosition = new Vector3 (boxX, 0.05f, boxZ);
		box.GetComponent<Renderer> ().material = throwYellowMat;
		
		box.transform.SetParent (throwingRanges.transform);
		
		return box;
	}
	
	// Cycle through targets to throw at
	public void ChooseThrowingDistance (int desiredDirection)
	{
		int currentDirection = -1;
		int currentDistance = -1;
		
		if (throwTarget != null) {
			for (int i=0; i<throwingBoxes.Length; i++) {
				if (throwingBoxes [i].Contains (throwTarget)) {
					currentDirection = i;
					currentDistance = throwingBoxes [i].IndexOf (throwTarget);
					break;
				}
			}
		}
		
		// No direction already chosen by player yet
		if (currentDirection == -1) {
			// Check if player can throw in desired direction
			if (throwingBoxes [desiredDirection].Count > 0) {
				throwTarget = throwingBoxes [desiredDirection] [0];
				throwTarget.GetComponent<Renderer> ().material = throwRedMat;
			}
		} 
		// Direction already chosen by player
		else {
			if (desiredDirection == currentDirection) {
				// If current distance is less than 2, and direction has greater range than selected, then throw range may be increased
				if (currentDistance < 2 && currentDistance < throwingBoxes [currentDirection].Count-1) {
					throwTarget = throwingBoxes [currentDirection] [++currentDistance];
					throwTarget.GetComponent<Renderer>().material = throwRedMat;
				}
			}
			else{
				// Reset blocks in previously chosen direction to yellow
				for(int i=0; i<=currentDistance; i++){
					throwingBoxes[currentDirection][i].GetComponent<Renderer>().material = throwYellowMat;
				}
				// Choose first block in new direction (if there is one)
				if(throwingBoxes[desiredDirection].Count>0){
					throwTarget = throwingBoxes[desiredDirection][0];
					throwTarget.GetComponent<Renderer>().material = throwRedMat;
				}
			}
		}
	}
	
	// Throw animals towards target block
	public void ThrowAnimals (){
		Vector3 movement = throwTarget.transform.localPosition - currentAnimal.transform.localPosition;
		movement.y = 0; // Heigh changes calculated in TransferAnimals()
		AnimalStack targetStack = StackAtTarget (movement);
		
		if (targetStack == null) {
			targetStack = new AnimalStack ();
			stacks.Add (targetStack);
		}

		lastAnimal = currentAnimal;
		
		// Move current animal to be up one in the stack, as stack being thrown starts there
		currentAnimal = currentStack.animals [++animalIndex];
		
		TransferAnimals (targetStack, movement);
		
		throwingMode = false;
		ClearThrowingBoxes ();
		UpdateHighlight ();
	}
	
	void HandleInput()
	{
		// Determine direction and distance to throw animals
		if (throwingMode) {
			if (Input.GetKeyDown (KeyCode.W)) ChooseThrowingDistance((int)Directions.Up);
			if (Input.GetKeyDown (KeyCode.D)) ChooseThrowingDistance((int)Directions.Right);
			if (Input.GetKeyDown (KeyCode.S)) ChooseThrowingDistance((int)Directions.Down);
			if (Input.GetKeyDown (KeyCode.A)) ChooseThrowingDistance((int)Directions.Left);
			
			if(Input.GetKeyDown (KeyCode.E)){
				if(throwTarget!=null){
					ThrowAnimals();
				}
			}
		}
		else{
			// Move current stack right
			if (Input.GetKeyDown (KeyCode.D)) {
				if (currentAnimal.transform.localPosition.x < (gridColumnNumber-1) * planeSideLength) {
					MoveStack (new Vector3 (planeSideLength, 0.0f, 0.0f));
				}
			}
			// Move current stack left
			if (Input.GetKeyDown (KeyCode.A)) {
				if (currentAnimal.transform.localPosition.x > 0) {
					MoveStack (new Vector3 (-planeSideLength, 0.0f, 0.0f));
				}
			}
			// Move current stack up
			if (Input.GetKeyDown (KeyCode.W)) {
				if (currentAnimal.transform.localPosition.z < (gridRowNumber-1) * planeSideLength) {
					MoveStack (new Vector3 (0.0f, 0.0f, planeSideLength));
				}
			}
			// Move current stack down
			if (Input.GetKeyDown (KeyCode.S)) {
				if (currentAnimal.transform.localPosition.z > 0) {
					MoveStack (new Vector3 (0.0f, 0.0f, -planeSideLength));
				}
			}
		}
		
		// Switch down between animals
		if(Input.GetKeyDown(KeyCode.Tab)) {
			SwitchAnimal(false);
		}
		
		// Switch up between animals
		if(Input.GetKeyDown(KeyCode.BackQuote)) {
			SwitchAnimal(true);
		}
		
		// Switch between throwing and movement modes
		if (Input.GetKeyDown (KeyCode.Q)) {
			// Check animal isn't at top of stack (and has no animals to throw)
			if(animalIndex<currentStack.animals.Count-1){
				throwingMode = !throwingMode;
				UpdateHighlight();
			}
			if(throwingMode){
				LoadThrowingRanges();
			}
			else{
				ClearThrowingBoxes();
			}
		}
		
		// End game
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
		
		// Restart level
		if(Input.GetKeyDown(KeyCode.Return)) {
			Application.LoadLevel(0);
		}
	}

	void SwitchAnimal (bool goingUp)
	{
		// Switching up
		if (goingUp) {
			do {
				// Check if there are moveable animals remaining in current stack
				if (animalIndex < currentStack.animals.Count - 1) {
					animalIndex++;
				} 
			// Otherwise, choose new stack
			else {
					// Start at beginning of next stack
					animalIndex = 0;
					// If at end of stacks list, return to beginning of list
					if (stackIndex == stacks.Count - 1) {
						stackIndex = 0;
					}
				// Otherwise, choose next stack in list
				else {
						stackIndex++;
					}
					// Update current stack
					currentStack = stacks [stackIndex];
				}
			} while(stacks[stackIndex].animals[animalIndex].name.Equals("Shepherd"));
		} 
		// Switching down
		else {
			do {
				// Check if there are moveable animals remaining in current stack
				if (animalIndex > 0) {
					animalIndex--;
				} 
			// Otherwise, choose new stack
			else {
					// If at beginning of stacks list, return to end of list
					if (stackIndex == 0) {
						stackIndex = stacks.Count - 1;
					}
				// Otherwise, choose next stack in list
				else {
						stackIndex--;
					}
					// Update current stack
					currentStack = stacks [stackIndex];
					// Start at top of stack
					animalIndex = currentStack.animals.Count - 1;
				}
			} while(stacks[stackIndex].animals[animalIndex].name.Equals("Shepherd"));
		}

		lastAnimal = currentAnimal;
		currentAnimal = currentStack.animals [animalIndex];
		
		// Highlight currently selected animal
		UpdateHighlight ();
		
		if(throwingMode){
			// Every animal starts in movement mode
			throwingMode = false;
			ClearThrowingBoxes();
			UpdateHighlight();
		}
	}

	// Gives the current animal selected a highlight, and removes highlight from previously selected animal
	// Also changes highlight colour if throwing/movement modes are switched between
	void UpdateHighlight(){
		Color highlightColor;
		
		if (throwingMode) {
			highlightColor = Color.magenta;
		} else {
			highlightColor = Color.cyan;
		}
		
		// Highlight current animal
		currentAnimal.GetComponent<Renderer> ().material.shader = outlined;
		currentAnimal.GetComponent<Renderer> ().material.SetColor ("_OutlineColor", highlightColor);
		currentAnimal.GetComponent<Renderer> ().material.SetFloat ("_Outline", 0.25f);
		
		// Remove highlight from previously selected animal
		if (lastAnimal != null && lastAnimal!= currentAnimal) {
			lastAnimal.GetComponent<Renderer> ().material.shader = diffuse;
		}
	}

}
