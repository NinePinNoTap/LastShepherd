using UnityEngine;
using System.Collections;

public class HighlightAnimalWithDisk : MonoBehaviour
{
    [Header("Component")]
    public StackManager stackManager;

    [Header("Highlighting")]
    public GameObject highlightDisk;
    public Color frogColor = Color.red;
    public Color monkeyColor = Color.green;
    public Color penguinColor = Color.blue;
    public Color turtleColor = Color.black;

    void Start ()
    {
    }

    void Update ()
    {
        int index;

        index = stackManager.gameAnimals.IndexOf(stackManager.currentAnimal);

        // Get animal color
        switch(index)
        {
            case 0:
                highlightDisk.GetComponent<SpriteRenderer>().color = frogColor;
                frogColor.a = 0.5f;
                break;

            case 1:
                highlightDisk.GetComponent<SpriteRenderer>().color = monkeyColor;
                monkeyColor.a = 0.5f;
                break;

            case 2:
                highlightDisk.GetComponent<SpriteRenderer>().color = penguinColor;
                penguinColor.a = 0.5f;
                break;

            case 3:
                highlightDisk.GetComponent<SpriteRenderer>().color = turtleColor;
                turtleColor.a = 0.5f;
                break;
        }

        highlightDisk.transform.position = stackManager.currentStack.Get(0).transform.position - 
                                            new Vector3(0.0f, (stackManager.animalHeight / 2), 0.0f) +
                                            new Vector3(0.0f, 0.01f, 0.0f);
    }
}
