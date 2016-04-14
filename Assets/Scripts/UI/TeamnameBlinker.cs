using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeamnameBlinker : MonoBehaviour
{
	public float flashPerSecond = 0.1f;
	private Image flashImage;

	void Start ()
	{
		flashImage = GetComponent<Image>();
		StartCoroutine(Flash());
	}

	private IEnumerator Flash()
	{
		while(true)
		{
			flashImage.enabled = !flashImage.isActiveAndEnabled;
			yield return new WaitForSeconds( (60/flashPerSecond) * Time.deltaTime);
			yield return null;
		}
	}
}
