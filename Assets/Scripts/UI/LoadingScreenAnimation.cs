using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreenAnimation : MonoBehaviour
{
	public enum FadeState { IN, OUT };

	[Header("Components")]
	public Image animationImage;

	[Header("Properties")]
	public List<Sprite> animationSprites;
	public float fadeDuration = 0.1f;
	private int animationIndex;
	private FadeState fadeState;

	void Start ()
	{
		// Initialise animation
		animationIndex = 0;
		animationImage.sprite = animationSprites[animationIndex];
		animationImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		// Start fade in
		fadeState = FadeState.IN;
		StartCoroutine(FadeImage());
	}

	private IEnumerator FadeImage()
	{
		while(true)
		{
			// Get current alpha value
			float curAlpha = animationImage.color.a;

			// Check what state we are in
			if(fadeState == FadeState.IN)
			{
				curAlpha += (1/fadeDuration) * Time.deltaTime;
			}
			else
			{
				curAlpha -= (1/fadeDuration) * Time.deltaTime;
			}

			// Apply the alpha back to the sprite
			animationImage.color = new Color(1.0f, 1.0f, 1.0f, curAlpha);

			// Check if we have finished
			if(Utility.CheckBounds(curAlpha, 0.0f, 1.0f))
			{
				// Haven't finished, return next frame
				yield return null;
			}
			else
			{
				// Reached a limit
				break;
			}
		}

		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		// Switch the state flade
		fadeState = (fadeState == FadeState.IN) ? FadeState.OUT : FadeState.IN;

		// Check if we are fading in
		if(fadeState == FadeState.IN)
		{
			// Update image index
			animationIndex++;
			Utility.Wrap(ref animationIndex, 0, animationSprites.Count - 1);
			animationImage.sprite = animationSprites[animationIndex];
		}

		// Start the animation again
		StartCoroutine(FadeImage());
	}
}
