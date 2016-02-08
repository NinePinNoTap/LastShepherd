using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	
	[Header("Game Time Properties")]
	public RectTransform timerBar;				// Access to the timer bar
	public float roundLength = 100.0f;			// How long a round is
	private Vector2 barSize;					// How big the bar was on start
	private float barMultiplier = 7.0f;			// How much to scale the bar

	[Header("Portaits UI Properties")]
	public StackManager stackManager;			// Access to the stack manager
	public Image[] animalPortaitsBG;			// List of Images for portait background
	public Image[] animalPortaits;				// List of Images for portait foreground

	[Header("To Be Removed Properties")]
	public Material material;							// Access to the timer material
	public Color topColour = new Color(1,1,1,1);		// Top colour of the gradient
	public Color bottomColour = new Color(1,1,1,1);		// Bottom colour of the gradient

	void Start ()
	{
		InitialisePortaits();
		InitialiseTimer();
	}

	void Update ()
	{
		HandleAnimalPortaits();
	}

	// 
	// PORTAITS
	//
	private void InitialisePortaits()
	{
		// Change the colour of the portaits to the same as the animals
		for(int i = 0; i < animalPortaits.Length; i++)
		{
			animalPortaits[i].color = stackManager.gameAnimals[i].GetComponent<ObjectHighlighter>().baseColor;
		}
	}
	
	private void HandleAnimalPortaits()
	{
		for(int i = 0; i < 3; i++)
		{
			if(stackManager.gameAnimals[i].Equals(stackManager.currentAnimal))
			{
				animalPortaitsBG[i].color = Color.yellow;
			}
			else
			{
				animalPortaitsBG[i].color = Color.black;
			}
		}
	}

	//
	// TIMER
	//
	private void InitialiseTimer()
	{
		// Store how big the bar is
		barSize = timerBar.sizeDelta;
		
		// Calculate how much to rescale
		barMultiplier = barSize.x / roundLength;
		
		// Update Gradient - Remove later
		if(material)
		{
			material.SetColor("_Color", topColour); // the left color
			material.SetColor("_Color2", bottomColour); // the right color
		}
		
		// Start Timer
		StartCoroutine(Timer ());
	}
	
	private IEnumerator Timer()
	{
		while(roundLength > 0.0f)
		{
			// Reduce length
			roundLength -= Time.deltaTime;
			
			// Calculate size
			barSize.x = roundLength * barMultiplier;
			timerBar.sizeDelta = barSize;
			
			// Come back because we not finished
			yield return null;
		}
		
		DoGameOver();
		
		yield return new WaitForEndOfFrame();
	}
	
	private void DoGameOver()
	{
		// Do something here
		Debug.Log ("Game Over Bitches!");
		Application.LoadLevel(Application.loadedLevel);
	}
}

