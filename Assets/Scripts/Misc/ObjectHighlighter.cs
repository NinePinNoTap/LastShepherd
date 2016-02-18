using UnityEngine;
using System.Collections;

public class ObjectHighlighter : MonoBehaviour
{
	public Color baseColor = new Color(1,1,1,1);
	public Color highlightColor = new Color(1,1,1,1);

	private Material material;

	void Awake ()
	{
		// Get material
		material = GetComponent<Renderer>().material;
		Toggle(false);
	}

	public void Toggle(bool Flag)
	{
		if(Flag)
		{
			material.SetColor("_Color", highlightColor);
		}
		else
		{
			material.SetColor("_Color", baseColor);
		}
	}
}
