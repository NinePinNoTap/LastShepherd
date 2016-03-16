using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//
// keyName specifies the key we save to at the end of the game
// In our level manager, we need to call 'PlayerPrefs.SaveInt(keyName, winState)
// Then this script loads that value and sets text accordingly
// The text elements will probably be replaced by images
//

public class GameOverManager : MonoBehaviour
{
    [Header("Components")]
    public Text instructionsText;

    [Header("Player Prefs")]
    public string keyName;

    [Header("Debuggin")]
    public int gameWin = 0;

	void Start ()
    {
        if(PlayerPrefs.HasKey(keyName))
        {
            int winState = PlayerPrefs.GetInt(keyName);

            HandleWinScreen(winState);
        }
        else
        {
            HandleWinScreen(gameWin);
        }
	}

    private void HandleWinScreen(int winState)
    {
        switch(winState)
        {
            // Lose
            case 0:
                instructionsText.text = "You lose!";
                break;

                // Win
            case 1:
                instructionsText.text = "You win!";
                break;
        }
    }
}