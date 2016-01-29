using UnityEngine;
using System.Collections;

public class AnimalBehaviour : MonoBehaviour 
{
	
	private bool active;
	
	public GameManager gm;
	
	private AnimalStack owner;
	
	public float animalHeight = 1.0f;
	
	
	// Use this for initialization
	void Start () 
	{
		active = false;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(active)
		{
			HandleInput();
		}
	
	}
	
	void OnCollisionEnter(Collision collision)
	{
		RaycastHit hit;
		Debug.Log("Collison detected");
		if(active && owner.animals[0].Equals(this.gameObject) && collision.gameObject.tag.Equals("Animal"))
		{
			AnimalStack oldStack = owner;
			AnimalStack newStack = collision.gameObject.GetComponent<AnimalBehaviour>().GetOwner();
			int newAnimalIndex = newStack.GetSize();
			Vector3 newPos = newStack.Get(newStack.GetSize()-1).transform.position;
			
			for(int i=0; i<oldStack.GetSize();i++)
			{
				Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(i+1), 0.0f);
				oldStack.animals[i].transform.position = temp;
				newStack.Add(oldStack.animals[i]);
				oldStack.animals[i].GetComponent<AnimalBehaviour>().SetOwner(newStack);
				
			}
			
			gm.levelStacks.Remove(oldStack);
			gm.currentStack = newStack;
			gm.stackIndex = gm.levelStacks.IndexOf(newStack);
			gm.animalIndex = newAnimalIndex;
			//gm.levelStacks[gm.stackIndex].animals[gm.animalIndex].GetComponent<AnimalBehaviour>().Activate();
			
		}
		else if(collision.gameObject.tag.Equals("Tile"))
		{
			//Physics.Raycast(this.gameObject.transform.position, collision.gameObject.transform.position-this.gameObject.transform.position, out hit, animalHeight);
			//if(!collision.gameObject.Equals(hit.transform.gameObject))
			//Vector3 distance = collision.gameObject.transform.position-this.gameObject.transform.position;
			//if(!(distance.y < 0.0f))
			//{
				
				for(int i=0; i<owner.GetSize();i++)
				{
					owner.animals[i].transform.position = (collision.gameObject.transform.position + new Vector3(0.0f, animalHeight*(i+1),0.0f));
				}
			//}
		}
	}
	
	void HandleInput()
	{
		if(owner.animals[0].Equals(this.gameObject))
		{
			if(Input.GetKey(KeyCode.W))
			{
				MoveStack(new Vector3(0, 0, 3));
			}
			if(Input.GetKey(KeyCode.A))
			{
				MoveStack(new Vector3(-3, 0, 0));
			}
			if(Input.GetKey(KeyCode.S))
			{
				MoveStack(new Vector3(0, 0, -3));	
			}
			if(Input.GetKey(KeyCode.D))
			{
				MoveStack(new Vector3(3, 0, 0));
			}
		}
		else
		{
			if(Input.GetKeyDown(KeyCode.W))
			{
				HopOffStack(new Vector3(0, 0, 1));
			}
			else if(Input.GetKeyDown(KeyCode.A))
			{
				HopOffStack(new Vector3(-1, 0, 0));
			}
			else if(Input.GetKeyDown(KeyCode.S))
			{
				HopOffStack(new Vector3(0, 0, -1));	
			}
			else if(Input.GetKeyDown(KeyCode.D))
			{
				HopOffStack(new Vector3(1, 0, 0));
			}
		}
		
	}
	
	void HopOffStack(Vector3 v)
	{
		AnimalStack oldStack = owner;
		AnimalStack newStack = new AnimalStack();
		
		for(int i = gm.animalIndex; i < oldStack.GetSize(); i++)
		{
			Vector3 newPos = oldStack.animals[i].transform.position + v*1.2f*animalHeight + new Vector3(0.0f, -animalHeight*gm.animalIndex,0.0f);
			oldStack.animals[i].transform.position = newPos;
			newStack.Add(oldStack.animals[i]);
			oldStack.animals[i].GetComponent<AnimalBehaviour>().SetOwner(newStack);
		}
		
		oldStack.animals.RemoveRange(gm.animalIndex, oldStack.GetSize()-gm.animalIndex);
		gm.levelStacks.Add(newStack);
		gm.currentStack = newStack;
		gm.stackIndex = gm.levelStacks.IndexOf(newStack);
		gm.animalIndex = 0;
	}
	
	
	void MoveStack(Vector3 v)
	{
		for(int i=0; i<owner.GetSize();i++)
		{
			owner.animals[i].GetComponent<AnimalBehaviour>().Move(v);
		}
	}
	
	void Move(Vector3 v)
	{
		Vector3 currentPosition = this.gameObject.transform.position;
		
		currentPosition += v*Time.deltaTime;
		
		this.gameObject.transform.position = currentPosition;
		
	}
	
	public void Activate()
	{
		active = true;
		this.gameObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.black);
	}
	
	public void Deactivate()
	{
		active = false;
		this.gameObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
	}

	
	public AnimalStack GetOwner()
	{
		return owner;
	}	
	
	public void SetOwner(AnimalStack a)
	{
		owner = a;	
	}
	
	public void SetGameManager(GameManager g)
	{	Debug.Log("Set GM for" + this.gameObject.name);
		gm = g;	
	}
	
}
