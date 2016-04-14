using UnityEngine;
using System.Collections;

public class GameSaveData : MonoBehaviour
{
    public static void SaveLevelData(string levelName, int levelPoints)
    {
        if(PlayerPrefs.HasKey(levelName))
        {
            PlayerPrefs.SetInt(levelName, levelPoints);
        }
        else
        {
            PlayerPrefs.SetInt(levelName, levelPoints);
        }   
    }

    public static int GetPoints(string levelName)
    {
        return PlayerPrefs.GetInt(levelName);
    }
}
