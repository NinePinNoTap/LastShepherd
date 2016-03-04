using UnityEngine;
using System;

public static class Utility
{
	// Ensure a value is within a range
	public static bool CheckBounds(int value, int min, int max)
	{
		if( value < min )
			return false;
		else if(value > max)
			return false;
		
		return true;
	}
	
	// Loop a value within a range
	public static void Wrap(ref int value, int min, int max)
	{
		if(value < min)
			value = max;
		if(value > max)
			value = min;
	}

	// Lock a value within a range
	public static void Clamp(ref int value, int min, int max)
	{
		if(value < min)
			value = min;
		if(value > max)
			value = max;
	}

    // Tries to add component to the gameobject
    public static T HandleComponent<T>(GameObject obj) where T : Component
    {
        T component;
        // Try and get the component
        component = (T)obj.GetComponent(typeof(T));

        // Check if we succeeded
        if(component == null)
        {
            // Doesn't exist so add it
            component = (T)obj.AddComponent(typeof(T));
        }

        return component;
    }
}