using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonFlashWhenSelected : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private enum TransitionState { Down, Up };
    private Button button;

    [Header("Properties")]
    public float flashSpeed = 0.75f;
    public bool isSelected = false;
    private TransitionState transitionState;
    private Color baseColor;
    private Color highlightedColor;
    private float animationFrame = 0;

	[Header("Text Scaling")]
	public int textSelect = 70;
	public int textDeselect = 50;

    void Awake()
    {
		// Access button component
		button = GetComponent<Button>();

        // Starting state
        transitionState = TransitionState.Down;

        // Get colour
        baseColor = button.colors.normalColor;
        highlightedColor =  button.colors.highlightedColor;
    }

    public void OnSelect(BaseEventData eventData)
	{
        isSelected = true;
        animationFrame = 0;
        StartCoroutine(Flash());

		GetComponentInChildren<Text>().fontSize = textSelect;
    }

    public void OnDeselect(BaseEventData eventData)
    {
		isSelected = false;
		GetComponentInChildren<Text>().fontSize = textDeselect;
    }

    private IEnumerator Flash()
    {
		while(isSelected)
        {
            // Increase alpha
            animationFrame += Time.deltaTime;

            // Get colour
            ColorBlock block = button.colors;

            // Update structs
            if(transitionState.Equals(TransitionState.Down))
            {
                // Lerp towards base
                block.highlightedColor = Color.Lerp(highlightedColor, baseColor, animationFrame / flashSpeed);
                if(block.highlightedColor.Equals(baseColor))
                {
                    transitionState = TransitionState.Up;
                    animationFrame = 0;
                }
            }
            else
            {
                // Lerp towards highlighted
                block.highlightedColor = Color.Lerp(baseColor, highlightedColor, animationFrame / flashSpeed);
                if(block.highlightedColor.Equals(highlightedColor))
                {
                    transitionState = TransitionState.Down;
                    animationFrame = 0;
                }
            }

            // Apply to button
            button.colors = block;

            yield return null;
        }
    }
}
