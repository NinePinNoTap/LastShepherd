using UnityEngine;
using System.Collections;

public class ObjectHighlighter : MonoBehaviour
{
	public Shader baseShader;
	public Shader highlightShader;

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
			material.shader = highlightShader;
		}
		else
		{
			material.shader = baseShader;
		}
	}
}
