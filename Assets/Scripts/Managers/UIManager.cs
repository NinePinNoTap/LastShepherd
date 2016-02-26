using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[Header("Component")]
	public GameManager gameManager;

	[Header("Game Time Properties")]
	public RectTransform timerBar;				// Access to the timer bar
	public float currentTime = 0.0f;			// How long we have played for
	public float roundLength = 100.0f;			// How long a round is
	private Vector2 barSize;					// How big the bar was on start
	private float barMultiplier = 7.0f;			// How much to scale the bar

	[Header("Portaits UI Properties")]
	public StackManager stackManager;			// Access to the stack manager
	public GameObject[] animalPortaits;			// List of Images for portait foreground
	public Color normalColor = Color.black;		// Outline normal colour
	public Color highlightColor = Color.white;	// Outline highlighted colour

	void Start ()
	{
		if(!gameManager)
		{
			GetComponent<GameManager>();
		}

		InitialiseTimer();
	}

	void Update ()
	{
		//HandleAnimalPortaits();
	}

	//========================================================================================
	// PORTAITS
	//========================================================================================
	
	private void HandleAnimalPortaits()
	{
		for(int i = 0; i < 3; i++)
		{
			if(stackManager.gameAnimals[i].Equals(stackManager.currentAnimal))
			{
				animalPortaits[i].GetComponent<Outline>().effectColor = highlightColor;
			}
			else
			{
				animalPortaits[i].GetComponent<Outline>().effectColor = normalColor;
			}
		}
	}

	//========================================================================================
	// GAME TIMER
	//========================================================================================
	private void InitialiseTimer()
	{
		// Store how big the bar is
		barSize = timerBar.sizeDelta;
		
		// Calculate how much to rescale
		barMultiplier = barSize.x / roundLength;
		
		// Start Timer
		StartCoroutine(Timer ());
	}
	
	private IEnumerator Timer()
	{
		// Keep looping
		while(roundLength > currentTime)
		{
			// Reduce length
			currentTime += Time.deltaTime;
			
			// Calculate size
			barSize.x = currentTime * barMultiplier;
			timerBar.sizeDelta = barSize;
			
			// Come back because we not finished
			yield return null;
		}

		// Reset level?
		gameManager.DoGameOver();
		
		yield return new WaitForEndOfFrame();
	}
}

