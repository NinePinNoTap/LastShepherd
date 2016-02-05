using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameTimer : MonoBehaviour
{
	[Header("Properties")]
	public RectTransform timerBar;
	public float roundLength = 100.0f;
	public float updateTime = 1.0f;

	[Header("Temp Properties")]
	public Material material;
	public Color topColour = new Color(1,1,1,1);
	public Color bottomColour = new Color(1,1,1,1);

	// Bar Properties
	private Vector2 barSize;
	private float barMultiplier;
	
	void Start ()
	{
		barSize = timerBar.sizeDelta;

		// Calculate how much to rescale
		barMultiplier = barSize.x / roundLength;

		// Define how much to reduce the bar
		// Lower = Smoother
		updateTime = 0.1f;

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
			// Update time since last frame
			updateTime = Time.deltaTime;

			// Reduce length
			roundLength -= updateTime;

			// Calculate size
			barSize.x = roundLength * 7.0f;
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
