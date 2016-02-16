using UnityEngine;
using System.Collections;

public enum AnimalColour { WHITE, GREEN, BLUE };

public class AnimalBehaviour : MonoBehaviour 
{
	public bool isControllable;
	public float moveSpeed = 1.5f;
	public StackManager stacksManager;
	
	public AnimalStack parentStack;
	public int stackIndex;
	
	public float animalHeight = 1.0f;
	
	public Vector3 currentVelocity;
	public ObjectHighlighter objHighlighter;

	public AnimalColour animalColour = AnimalColour.WHITE;

	public bool beingThrown;

	public LayerMask layerMask;

	void Awake () 
	{
		isControllable = false;
		beingThrown = false;
	}

	void Update () 
	{

	}
	
	void FixedUpdate()
	{

		if(isControllable && !beingThrown)
		{
			HandleCollision();
		}
		
		if (beingThrown) {
			if(IsGrounded()){
				beingThrown = false;
			}
		}

		if (!beingThrown) {
			// Update velocity
			GetComponent<Rigidbody> ().velocity = new Vector3 (currentVelocity.x, GetComponent<Rigidbody> ().velocity.y, currentVelocity.z);
		}

		
		if(stackIndex > 0)
		{
			Vector3 correctedPosition = parentStack.Get(0).gameObject.transform.position;
			correctedPosition.y += stackIndex*animalHeight;
			transform.position = correctedPosition;
		}
	}
	
	private void HandleCollision()
	{
		RaycastHit hit;
		RaycastHit hit2;
		RaycastHit bottom;
		for(int i = 0; i< 13; i++)
		{
			float x = animalHeight*0.5f * Mathf.Cos(((360.0f/13.0f)*i)*(Mathf.PI /180.0f));
			float z = animalHeight*0.5f * Mathf.Sin(((360.0f/13.0f)*i)*(Mathf.PI /180.0f));

			if(Physics.Raycast(this.gameObject.transform.position, new Vector3(x,0.0f,z), out hit, animalHeight * 0.55f))
			{
				if( (stackIndex==0) && hit.transform.gameObject.tag.Equals("Animal"))
				{
					AnimalStack newStack = parentStack;
					AnimalStack oldStack = hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();
					int newAnimalIndex = 0;
					Vector3 newPos = oldStack.Get(0).transform.position;
					
					for(int k=0; k<newStack.GetSize();k++)
					{
						Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(k), 0.0f);
						newStack.Get(k).transform.position = temp;
						newStack.Get(k).GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
						//newStack.Add(oldStack.Get(k));
						//oldStack.Get(k).GetComponent<Rigidbody>().useGravity = false;
						//oldStack.Get(k).GetComponent<AnimalBehaviour>().SetParentStack(newStack,(newStack.GetSize()-1));
					}

					newPos = newStack.Get(newStack.GetSize()-1).transform.position;

					for(int k=0; k<oldStack.GetSize();k++)
					{
						Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(k+1), 0.0f);
						oldStack.Get(k).transform.position = temp;
						oldStack.Get(k).GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
						newStack.Add(oldStack.Get(k));
						oldStack.Get(k).GetComponent<Rigidbody>().useGravity = false;
						oldStack.Get(k).GetComponent<AnimalBehaviour>().SetParentStack(newStack,(newStack.GetSize()-1));
					}
					
					stacksManager.levelStacks.Remove(oldStack);
					stacksManager.currentStack = newStack;
					stacksManager.stackIndex = stacksManager.levelStacks.IndexOf(newStack);
					stacksManager.animalIndex = newAnimalIndex;
					
				}
				else if( (stackIndex==0) && hit.transform.gameObject.tag.Equals("Tile"))
				{
					Vector3 start = hit.transform.position;
					//start.y += animalHeight*0.5f;
					
					if(Physics.Raycast(start, Vector3.up, out hit2, animalHeight) )
					{
						if((hit.transform.gameObject!=hit2.transform.gameObject) && (hit2.transform.gameObject.tag.Equals("Tile")))
						{
							Debug.Log ("HI");
							continue;
						}
					}
					
					for(int j=0; j<parentStack.GetSize();j++)
					{
						parentStack.Get(j).transform.position = (hit.transform.position + new Vector3(0.0f, animalHeight*(j+1),0.0f));
					}
				}
				
				return;
			}
			else if(Physics.Raycast(this.gameObject.transform.position, new Vector3(x,z,0.0f) , out bottom, animalHeight*0.55f))
			{
				if((stackIndex==0) && bottom.transform.gameObject.tag.Equals("Animal") && !(parentStack.Equals(bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack())))
				{
					AnimalStack oldStack = parentStack;
					AnimalStack newStack = bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetParentStack();
					int newAnimalIndex = newStack.GetSize();
					Vector3 newPos = newStack.Get(newStack.GetSize()-1).transform.position;
					
					for(int k=0; k<oldStack.GetSize();k++)
					{
						Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(k+1), 0.0f);
						oldStack.Get(k).transform.position = temp;
						oldStack.Get(k).GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
						newStack.Add(oldStack.Get(k));
						oldStack.Get(k).GetComponent<Rigidbody>().useGravity = false;
						oldStack.Get(k).GetComponent<AnimalBehaviour>().SetParentStack(newStack,(newStack.GetSize()-1));
						
					}
					
					stacksManager.levelStacks.Remove(oldStack);
					stacksManager.currentStack = newStack;
					stacksManager.stackIndex = stacksManager.levelStacks.IndexOf(newStack);
					stacksManager.animalIndex = newAnimalIndex;
				}
			}
		}		
	}
	
	public void HopOffStack(Vector3 v)
	{
		AnimalStack oldStack = parentStack;
		AnimalStack newStack = new AnimalStack();
		
		for(int i = stacksManager.animalIndex; i < oldStack.GetSize(); i++)
		{
			Vector3 newPos = oldStack.Get(i).transform.position + v*1.1f*animalHeight + new Vector3(0.0f, -animalHeight*stacksManager.animalIndex,0.0f);
			oldStack.Get(i).transform.position = newPos;
			newStack.Add(oldStack.Get(i));
			oldStack.Get(i).GetComponent<AnimalBehaviour>().SetParentStack(newStack, (i-stacksManager.animalIndex));
		}
		newStack.Get(0).GetComponent<Rigidbody>().useGravity = true;
		oldStack.GetList ().RemoveRange(stacksManager.animalIndex, oldStack.GetSize()-stacksManager.animalIndex);
		stacksManager.levelStacks.Add(newStack);
		stacksManager.currentStack = newStack;
		stacksManager.stackIndex = stacksManager.levelStacks.IndexOf(newStack);
		stacksManager.animalIndex = 0;
	}
	
	public void MoveStack(Vector3 v)
	{
		for(int i=0; i<parentStack.GetSize();i++)
		{
			parentStack.Get(i).GetComponent<AnimalBehaviour>().Move(v);
		}
	}

	public bool IsGrounded(){
		RaycastHit bottom;
		
		if (Physics.Raycast(this.gameObject.transform.position, -Vector3.up, out bottom, animalHeight*0.51f, layerMask)) {
			if(GetComponent<Rigidbody>().velocity.y == 0){
				return true;
			}
		}
		
		return false;
	}
	
	void Move(Vector3 v)
	{
		currentVelocity = v;
	}
	
	public void Activate()
	{
		isControllable = true;
		objHighlighter.Toggle(true);
	}
	
	public void Deactivate()
	{
		isControllable = false;
		objHighlighter.Toggle(false);
	}
	
	public AnimalStack GetParentStack()
	{
		return parentStack;
	}	
	
	public void SetParentStack(AnimalStack a, int i)
	{
		parentStack = a;
		stackIndex = i;	
	}

	// returns true if the animal has others above it in the stack
	public bool CanThrow(){
		int stackSize = parentStack.GetSize ();
		
		if (stackSize <= 1)
			return false;
		
		if (stacksManager.animalIndex < stackSize - 1) {
			return true;
		} else {
			return false;
		}
	}
	
	public GameObject GetAnimalAbove(){
		if (CanThrow ()) {
			return parentStack.Get (stacksManager.animalIndex + 1);
		} else {
			return null;
		}
	}
}