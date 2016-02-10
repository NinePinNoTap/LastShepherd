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
	public GameObject[] animalPortaits;				// List of Images for portait foreground

	void Start ()
	{
		InitialisePortaits();
		InitialiseTimer();
	}

	void Update ()
	{
		HandleAnimalPortaits();
	}

	//========================================================================================
	// PORTAITS
	//========================================================================================
	private void InitialisePortaits()
	{
		// Change the colour of the portaits to the same as the animals
		for(int i = 0; i < animalPortaits.Length; i++)
		{
			animalPortaits[i].GetComponent<Image>().color = stackManager.gameAnimals[i].GetComponent<ObjectHighlighter>().baseColor;
		}
	}
	
	private void HandleAnimalPortaits()
	{
		for(int i = 0; i < 3; i++)
		{
			if(stackManager.gameAnimals[i].Equals(stackManager.currentAnimal))
			{
				animalPortaits[i].GetComponent<Outline>().effectColor = Color.yellow;
			}
			else
			{
				animalPortaits[i].GetComponent<Outline>().effectColor = Color.black;
			}
		}
	}

	//========================================================================================
	// PORTAITS
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

