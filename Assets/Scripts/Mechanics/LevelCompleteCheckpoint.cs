using UnityEngine;
using System.Collections;

public class LevelCompleteCheckpoint : MonoBehaviour
{
	[Header("Properties")]
	public bool isActivated = false;

    void Update()
    {
        transform.Rotate (0,1,0);
    }

	void OnTriggerEnter()
	{
		if(isActivated)
			return;

		// Flag its activated so we cant activate multiple times
		isActivated = true;

        GameManager.Instance.DoGameComplete();
	}
}