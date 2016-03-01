using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public struct PortaitData
{
	public Outline backgroundOutline;
	public Animator animatorControl;
	public Light portaitLight;
	public bool isSelected;
}

public class UIManager : MonoBehaviour
{
	[Header("Component")]
	public GameManager gameManager;
	public StackManager stackManager;			// Access to the stack manager

	[Header("Game Time Properties")]
	public RectTransform timerBar;				// Access to the timer bar
	public float currentTime = 0.0f;			// How long we have played for
	public float roundLength = 100.0f;			// How long a round is
	private Vector2 barSize;					// How big the bar was on start
	private float barMultiplier = 7.0f;			// How much to scale the bar

	[Header("Portaits UI Properties")]
	public PortaitData[] animalData;
	public Color normalColor = Color.black;		// Outline normal colour
	public Color highlightColor = Color.white;	// Outline highlighted colour

	void Start ()
	{
		if(!gameManager)
		{
			GetComponent<GameManager>();
		}

		// Initialise data
		for(int i = 0; i < animalData.Length; i++)
		{
			animalData[i].backgroundOutline.effectColor = normalColor;
			animalData[i].animatorControl.speed = 0.0f;
			animalData[i].portaitLight.enabled = false;
			animalData[i].isSelected = false;
		}

		InitialiseTimer();
	}

	void Update ()
	{
		HandleAnimalPortaits();
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
				if(!animalData[i].isSelected)
				{
					animalData[i].backgroundOutline.effectColor = highlightColor;
					animalData[i].animatorControl.speed = 1.0f;
					animalData[i].portaitLight.enabled = true;
					animalData[i].isSelected = true;
				}
			}
			else
			{
				if(animalData[i].isSelected)
				{
					animalData[i].backgroundOutline.effectColor = normalColor;
					animalData[i].animatorControl.speed = 0.0f;
					animalData[i].portaitLight.enabled = false;
					animalData[i].isSelected = false;
				}
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

