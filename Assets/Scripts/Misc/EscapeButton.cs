using UnityEngine;
using System.Collections;

public class EscapeButton : MonoBehaviour
{
    public string nameMissionSelect = "MissionSelect";

	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(nameMissionSelect);
        }
	}
}
