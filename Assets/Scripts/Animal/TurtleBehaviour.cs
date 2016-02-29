using UnityEngine;
using System.Collections;

public class TurtleBehaviour : MonoBehaviour
{
	public enum TurtleState { ONBACK, NORMAL };

	[Header("Turtle Behaviour")]
	public TurtleState turtleState;

	void Start ()
	{
		turtleState = TurtleState.NORMAL;
	}

	void Update ()
	{
	
	}

	public bool IsOnBack()
	{
		return turtleState.Equals(TurtleState.ONBACK);
	}
}

