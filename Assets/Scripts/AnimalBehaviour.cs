using UnityEngine;
using System.Collections;

public class AnimalBehaviour : MonoBehaviour 
{
	
	private bool active;
	
	public GameManager gm;
	
	private AnimalStack owner;
	private int ownIndex;
	
	public float animalHeight = 1.0f;
	
	public Vector3 currentVelocity;
	
	
	// Use this for initialization
	void Start () 
	{
		active = false;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		currentVelocity = new Vector3(0.0f,0.0f,0.0f);
		if(active)
		{
			HandleInput();
			HandleCollision();
		}
		else
		{
			
			
		}
	
	}
	
	void FixedUpdate()
	{
	
		Vector3 newVelocity = this.gameObject.GetComponent<Rigidbody>().velocity;
		newVelocity.x = currentVelocity.x;
		newVelocity.z = currentVelocity.z;
		this.gameObject.GetComponent<Rigidbody>().velocity = newVelocity;
		
		if(ownIndex > 0)
		{
			Vector3 correctedPosition = owner.animals[0].gameObject.transform.position;
		
			correctedPosition.y += ownIndex*animalHeight;
		
			this.gameObject.transform.position = correctedPosition;
		}
	}
	
	void HandleCollision()
	{
		RaycastHit hit;
		RaycastHit hit2;
		RaycastHit bottom;
		for(int i = 0; i< 13; i++)
		{
			float x = animalHeight*0.5f * Mathf.Cos(((360.0f/13.0f)*i)*(Mathf.PI /180.0f));
			float z = animalHeight*0.5f * Mathf.Sin(((360.0f/13.0f)*i)*(Mathf.PI /180.0f));
			
			if(Physics.Raycast(this.gameObject.transform.position, new Vector3(x,0.0f,z) , out hit, animalHeight*0.55f))
			{
				if( (ownIndex==0) && hit.transform.gameObject.tag.Equals("Animal"))
				{
					AnimalStack oldStack = owner;
					AnimalStack newStack = hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetOwner();
					int newAnimalIndex = newStack.GetSize();
					Vector3 newPos = newStack.Get(newStack.GetSize()-1).transform.position;
					
					for(int k=0; k<oldStack.GetSize();k++)
					{
						Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(k+1), 0.0f);
						oldStack.animals[k].transform.position = temp;
						oldStack.animals[k].GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
						newStack.Add(oldStack.animals[k]);
						oldStack.animals[k].GetComponent<Rigidbody>().useGravity = false;
						oldStack.animals[k].GetComponent<AnimalBehaviour>().SetOwner(newStack,(newStack.GetSize()-1));
						
					}
					
					gm.levelStacks.Remove(oldStack);
					gm.currentStack = newStack;
					gm.stackIndex = gm.levelStacks.IndexOf(newStack);
					gm.animalIndex = newAnimalIndex;
					//gm.levelStacks[gm.stackIndex].animals[gm.animalIndex].GetComponent<AnimalBehaviour>().Activate();
					
				}
				else if( (ownIndex==0) && hit.transform.gameObject.tag.Equals("Tile"))
				{
					//Physics.Raycast(collision.gameObject.transform.position, Vector3.up, out hit, animalHeight);
					
					Vector3 start = hit.transform.position;
					start.y += animalHeight*0.5f;
					
					if(Physics.Raycast(start, Vector3.up, out hit2, animalHeight) )
					{
						if((hit.transform.gameObject!=hit2.transform.gameObject) && (hit2.transform.gameObject.tag.Equals("Tile")))
						{
							return;
						}
					}
					
					for(int j=0; j<owner.GetSize();j++)
					{
						owner.animals[j].transform.position = (hit.transform.position + new Vector3(0.0f, animalHeight*(j+1),0.0f));
					}
					//Physics.Raycast(this.gameObject.transform.position, collision.gameObject.transform.position-this.gameObject.transform.position, out hit, animalHeight);
					//if(!collision.gameObject.Equals(hit.transform.gameObject))
					/*Vector3 distance = hit.transform.gameObject.transform.position-this.gameObject.transform.position;
				if(true)
				{
					distance = Vector3.Normalize(distance);
					if((distance.y < 0.4f) && (distance.y > -0.4f))
					{
						
						for(int i=0; i<owner.GetSize();i++)
						{
							owner.animals[i].transform.position = (hit.transform.gameObject.transform.position + new Vector3(0.0f, animalHeight*(i+1),0.0f));
						}
					}
				}			
				*/
				}
				
				
				return;
			}
			else if(Physics.Raycast(this.gameObject.transform.position, new Vector3(x,z,0.0f) , out bottom, animalHeight*0.55f))
			{
				if((ownIndex==0) && bottom.transform.gameObject.tag.Equals("Animal") && !(owner.Equals(bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetOwner())))
				{
					AnimalStack oldStack = owner;
					AnimalStack newStack = bottom.transform.gameObject.GetComponent<AnimalBehaviour>().GetOwner();
					int newAnimalIndex = newStack.GetSize();
					Vector3 newPos = newStack.Get(newStack.GetSize()-1).transform.position;
					
					for(int k=0; k<oldStack.GetSize();k++)
					{
						Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(k+1), 0.0f);
						oldStack.animals[k].transform.position = temp;
						oldStack.animals[k].GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
						newStack.Add(oldStack.animals[k]);
						oldStack.animals[k].GetComponent<Rigidbody>().useGravity = false;
						oldStack.animals[k].GetComponent<AnimalBehaviour>().SetOwner(newStack,(newStack.GetSize()-1));
						
					}
					
					gm.levelStacks.Remove(oldStack);
					gm.currentStack = newStack;
					gm.stackIndex = gm.levelStacks.IndexOf(newStack);
					gm.animalIndex = newAnimalIndex;
				}	
				
			}
		}
		
		
	}
	
	/*void OnCollisionEnter(Collision collision)
	{
		RaycastHit hit;
		Debug.Log("Collison detected");
		if(active && (ownIndex==0) && collision.gameObject.tag.Equals("Animal"))
		{
			AnimalStack oldStack = owner;
			AnimalStack newStack = collision.gameObject.GetComponent<AnimalBehaviour>().GetOwner();
			int newAnimalIndex = newStack.GetSize();
			Vector3 newPos = newStack.Get(newStack.GetSize()-1).transform.position;
			
			for(int i=0; i<oldStack.GetSize();i++)
			{
				Vector3 temp = newPos + new Vector3(0.0f, animalHeight*(i+1), 0.0f);
				oldStack.animals[i].transform.position = temp;
				oldStack.animals[i].GetComponent<AnimalBehaviour>().currentVelocity = new Vector3(0.0f,0.0f,0.0f);
				newStack.Add(oldStack.animals[i]);
				oldStack.animals[i].GetComponent<Rigidbody>().useGravity = false;
				oldStack.animals[i].GetComponent<AnimalBehaviour>().SetOwner(newStack,(newStack.GetSize()-1));
				
			}
			
			gm.levelStacks.Remove(oldStack);
			gm.currentStack = newStack;
			gm.stackIndex = gm.levelStacks.IndexOf(newStack);
			gm.animalIndex = newAnimalIndex;
			//gm.levelStacks[gm.stackIndex].animals[gm.animalIndex].GetComponent<AnimalBehaviour>().Activate();
			
		}
		else if(active && (ownIndex==0) && collision.gameObject.tag.Equals("Tile"))
		{
			//Physics.Raycast(collision.gameObject.transform.position, Vector3.up, out hit, animalHeight);
			
			if(Physics.Raycast(collision.gameObject.transform.position, Vector3.up, out hit, animalHeight) )
			{
				if((hit.transform.gameObject!=collision.gameObject) && (hit.transform.gameObject.tag.Equals("Tile")))
				{
					return;
				}
			}
			
			//Physics.Raycast(this.gameObject.transform.position, collision.gameObject.transform.position-this.gameObject.transform.position, out hit, animalHeight);
			//if(!collision.gameObject.Equals(hit.transform.gameObject))
			Vector3 distance = collision.gameObject.transform.position-this.gameObject.transform.position;
			if(true)
			{
				distance = Vector3.Normalize(distance);
				if((distance.y < 0.4f) && (distance.y > -0.4f))
				{
					
					for(int i=0; i<owner.GetSize();i++)
					{
						owner.animals[i].transform.position = (collision.gameObject.transform.position + new Vector3(0.0f, animalHeight*(i+1),0.0f));
					}
				}
			}			
			
		}
		
		
	}*/
	
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
			Vector3 newPos = oldStack.animals[i].transform.position + v*1.1f*animalHeight + new Vector3(0.0f, -animalHeight*gm.animalIndex,0.0f);
			oldStack.animals[i].transform.position = newPos;
			newStack.Add(oldStack.animals[i]);
			oldStack.animals[i].GetComponent<AnimalBehaviour>().SetOwner(newStack, (i-gm.animalIndex));
		}
		newStack.animals[0].GetComponent<Rigidbody>().useGravity = true;
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
		/*Vector3 currentPosition = this.gameObject.transform.position;
		
		currentPosition += v*Time.deltaTime;
		
		this.gameObject.transform.position = currentPosition;
		*/
		currentVelocity = v;
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
	
	public void SetOwner(AnimalStack a, int i)
	{
		owner = a;
		ownIndex =i;	
	}
	
	public void SetGameManager(GameManager g)
	{	Debug.Log("Set GM for" + this.gameObject.name);
		gm = g;	
	}
	
}
