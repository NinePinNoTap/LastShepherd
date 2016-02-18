using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public string nameMainMenu = "MainMenu";
    public string nameMissionSelect = "MissionSelect";
    public string namePauseMenu = "PauseMenu";
    public string nameLoadingScreen = "LoadingScreen";
    public string[] nameLevels;

    private float loadTime = 10;

    void Start()
    {
        // We want this to be continuous
        DontDestroyOnLoad(this);
    }

    public void GoToLevel(int ID)	
    {
        if(ID > nameLevels.Length)
            return;

        //Application.LoadLevel(nameLevels[ID]);

        StartCoroutine(Test());
    }

    public void GoToMainMenu()
    {
        Application.LoadLevel(nameMainMenu);
    }

    public void GoToLoadScreen()
    {
        Application.LoadLevel(nameLoadingScreen);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    private IEnumerator Test()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        AsyncOperation async = Application.LoadLevelAdditiveAsync(nameLevels[0]);

        yield return async;

        yield return new WaitForSeconds(loadTime);



        Debug.Log("LOADED");
    }
}
