using UnityEngine;
using System;

public static class Utility
{
	//==================================
	// Ensure a value is within a range
	//==================================

	public static bool CheckBounds(int value, int min, int max)
	{
		if( value < min )
			return false;
		else if(value > max)
			return false;
		
		return true;
	}
	public static bool CheckBounds(float value, float min, float max)
	{
		if( value < min )
			return false;
		else if(value > max)
			return false;
		
		return true;
	}

	//=============================
	// Loop a value within a range
	//=============================
	
	public static void Wrap(ref int value, int min, int max)
	{
		if(value < min)
			value = max;
		if(value > max)
			value = min;
	}
	public static void Wrap(ref float value, float min, float max)
	{
		if(value < min)
			value = max;
		if(value > max)
			value = min;
	}

	//=============================
	// Lock a value within a range
	//=============================
	
	public static void Clamp(ref int value, int min, int max)
	{
		if(value < min)
			value = min;
		if(value > max)
			value = max;
	}
	public static void Clamp(ref float value, float min, float max)
	{
		if(value < min)
			value = min;
		if(value > max)
			value = max;
	}

	//====================
	// Component Handling
	//====================

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

    //========
    // Assert
    //========

    public static void Assert<T>(T component, GameObject owner) where T : Component
    {
        if(component)
        {
            return;
        }
        else
        {
            Debug.Log("WARNING! " + owner.name + " is missing component " + typeof(T).ToString());
        }
    }

    public static void Assert(GameObject obj, GameObject owner)
    {
        if(obj)
        {
            return;
        }
        else
        {
            Debug.Log("WARNING! " + owner.name + " is missing gameobject reference.");
        }
    }

    public static void Assert<T>(GameObject obj) where T : Component
    {
        T component;

        // Try and get the component
        component = (T)obj.GetComponent(typeof(T));

        if(component)
        {
            return;
        }
        else
        {
            Debug.Log("WARNING! " + obj.name + " is missing component " + typeof(T).ToString());
        }
    }

    public static T GetComponentFromTag<T>(string tag) where T : Component
    {
        T component;
        GameObject obj;

        // Find the gameobject
        obj = GameObject.FindGameObjectWithTag(tag);

        // Check if we found an object
        if(!obj)
        {
            return null;
        }

        // Get the component of the object
        component = (T)obj.GetComponent(typeof(T));

        return component;
    }
}