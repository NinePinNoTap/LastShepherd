using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public struct PortaitDataOLD
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
	public PortaitDataOLD[] animalData;
	public Color normalColor = Color.black;		// Outline normal colour
	public Color highlightColor = Color.white;	// Outline highlighted colour

	void Start ()
	{
		// Access the game manager
		if(!gameManager)
		{
			GetComponent<GameManager>();
		}

		// Initialise data
		for(int i = 0; i < animalData.Length; i++)
		{
			if(animalData[i].animatorControl)
			{
				animalData[i].animatorControl.speed = 0.0f;
			}

			animalData[i].backgroundOutline.effectColor = normalColor;
			animalData[i].portaitLight.enabled = false;
			animalData[i].isSelected = false;
		}

		// Create and start the timer
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
		for(int i = 0; i < animalData.Length; i++)
		{
			if(stackManager.gameAnimals[i].Equals(stackManager.currentAnimal))
			{
				if(!animalData[i].isSelected)
				{
					if(animalData[i].animatorControl)
					{
						animalData[i].animatorControl.Play("idle", 0, 0.0f);
						animalData[i].animatorControl.speed = 1.0f;
					}

					animalData[i].backgroundOutline.effectColor = highlightColor;
					animalData[i].portaitLight.enabled = true;
					animalData[i].isSelected = true;
				}
			}
			else
			{
				if(animalData[i].isSelected)
				{
					if(animalData[i].animatorControl)
					{
						animalData[i].animatorControl.Play("idle", 0, 0.0f);
						animalData[i].animatorControl.speed = 0.0f;
					}

					animalData[i].backgroundOutline.effectColor = normalColor;
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

