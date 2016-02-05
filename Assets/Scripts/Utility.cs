using UnityEngine;
using System;

namespace Helper
{
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

		public static float GridRanking(Vector3 pos)
		{
			return pos.x + (pos.z * 100);
		}
	}
}