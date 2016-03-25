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

    void Start()
    {
        // Get access to button
        button = GetComponent<Button>();

        // Starting state
        transitionState = TransitionState.Down;

        // Get colour
        baseColor = button.colors.normalColor;
        highlightedColor =  button.colors.highlightedColor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("Selected");
        isSelected = true;
        animationFrame = 0;
        StartCoroutine(Flash());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        StopCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        while(true)
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
