using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public struct PortraitData
{
	public Image sprite;
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
	public float GameManager.Instant.clockTime = 100.0f;			// How long a round is
	private Vector2 barSize;					// How big the bar was on start
	private float barMultiplier = 7.0f;			// How much to scale the bar
	
	[Header("Portaits UI Properties")]
	public PortraitData[] animalData;
	
	void Start ()
	{
		// Make sure we have stack manager
		if(!gameManager)
		{
			gameManager = Utility.GetComponentFromTag<GameManager>("GameManager");
		}

		// Make sure we have stack manager
		if(!stackManager)
		{
			stackManager = Utility.GetComponentFromTag<StackManager>("StackManager");
		}
		
		// Initialise data
		for(int i = 0; i < animalData.Length; i++)
		{
			animalData[i].isSelected = false;
		}
		
		// Create and start the timer
		InitialiseTimer();
	}
	
	void Update ()
	{
		HandleAnimalPortraits();
	}
	
	//========================================================================================
	// PORTAITS
	//========================================================================================
	
	private void HandleAnimalPortraits()
	{
		for(int i = 0; i < animalData.Length; i++)
		{
			if(stackManager.gameAnimals[i].Equals(stackManager.currentAnimal))
			{
				if(!animalData[i].isSelected)
				{
					animalData[i].sprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(170,170);
					animalData[i].isSelected = true;
				}
			}
			else
			{
				if(animalData[i].isSelected)
				{
					animalData[i].sprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100,100);
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
		barMultiplier = barSize.x / GameManager.Instance.levelClock;
		
		// Start Timer
		StartCoroutine(Timer ());
	}
	
	private IEnumerator Timer()
	{
		// Keep looping
		while(GameManager.Instance.levelClock > currentTime)
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

