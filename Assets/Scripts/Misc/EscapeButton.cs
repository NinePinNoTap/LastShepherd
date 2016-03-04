using UnityEngine;
using System.Collections;

public class EscapeButton : MonoBehaviour
{
    public string nameMissionSelect = "MissionSelect";

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(nameMissionSelect);
        }
	}
}
