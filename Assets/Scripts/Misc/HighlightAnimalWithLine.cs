using UnityEngine;
using System.Collections;

public class HighlightAnimalWithLine : MonoBehaviour
{
    [Header("Component")]
    public StackManager stackManager;

    [Header("Highlighting")]
    public Color frogColor = Color.red;
    public Color monkeyColor = Color.green;
    public Color penguinColor = Color.blue;
    public Color turtleColor = Color.black;
    public Material material;

    public LineRenderer lineRenderer;

	void Start ()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.SetWidth(.5f, .5f);
	}
	
	void Update ()
    {
        int index;

        index = stackManager.gameAnimals.IndexOf(stackManager.currentAnimal);

        // Get animal color
        switch(index)
        {
            case 0:
                frogColor.a = 0.5f;
                lineRenderer.SetColors(frogColor, frogColor);
                break;

            case 1:
                monkeyColor.a = 0.5f;
                lineRenderer.SetColors(monkeyColor, monkeyColor);
                break;

            case 2:
                penguinColor.a = 0.5f;
                lineRenderer.SetColors(penguinColor, penguinColor);
                break;

            case 3:
                turtleColor.a = 0.5f;
                lineRenderer.SetColors(turtleColor, turtleColor);
                break;
        }

        Vector3 animalPos = stackManager.currentAnimal.transform.position;
        lineRenderer.SetPosition(0, animalPos);
        animalPos.y = 20.0f;
        lineRenderer.SetPosition(1, animalPos);
        lineRenderer.material = material;
	}
}
