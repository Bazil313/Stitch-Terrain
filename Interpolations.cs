using UnityEngine;
using System.Collections;

public class Interpolations
{
	// Interpolation function that returns the height of a sin curve for a given distance along the curve
	public static float sinInterpolation (float percentile, float numWaves, float start, float end, float height, float blendLimit)
	{
		// Default return will be a linear interpolation between the start and end values
		float finalInterpolation = Mathf.Lerp (start, end, percentile);
		// If the requested ehight value is 0, return the linear interpolation to avoid calulation issues in the final result
		if(height == 0)
			return finalInterpolation;
		else
		{
			// Make sure the percentile is between 0 and 1
			percentile = Mathf.Clamp (percentile, 0, 1);

			// Calculate a sin curve that begins and ends at the 'start' height and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			float startSin = ((height-start)*Mathf.Sin (numWaves * percentile * 2f * Mathf.PI + 3 * Mathf.PI / 2f) + (height-start)) / 2f+start;
			// Calculate a sin curve that begins and ends at the 'end' height and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			float endSin = ((height-end)*Mathf.Sin (numWaves * percentile * 2f * Mathf.PI + 3 * Mathf.PI / 2f) + (height-end)) / 2f+end;
			// Calculate a sin curve that begins and ends at 0 and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			float regSin =  (height*Mathf.Sin (numWaves * percentile * 2f * Mathf.PI + 3 * Mathf.PI / 2f) + height) / 2f;

			// If the distance along the curve is less than the blend limit, interpolate between the sin curve that starts at 'start' height and the sin curve that starts at 0
			if(percentile < blendLimit) finalInterpolation = Mathf.Lerp (startSin,regSin,percentile/blendLimit);
			// If the distance along the curve is between the blend limits, use ony the values provided by the sin curve that starts at 0
			else if (percentile >= blendLimit && percentile <= 1f - blendLimit) finalInterpolation = regSin;
			// If the distance along the curve has reached the blend limit at the end, interpolate between the sin curve that end at 'end' height and the sin curve that starts at 0
			else finalInterpolation = Mathf.Lerp (endSin,regSin,(1f-percentile)/blendLimit);

			// Return the interpolated sin curves
			return finalInterpolation;
		}
	}

	// Interpolation function that returns the height of a triangle wave for a given distance along the curve
	public static float triangleInterpolation (float percentile, float numWaves, float start, float end, float height, float rounded)
	{
		// Default return will be a linear interpolation between the start and end values
		float finalInterpolation = Mathf.Lerp (start, end, percentile);
		// If the requested ehight value is 0, return the linear interpolation to avoid calulation issues in the final result
		if (height == 0)
			return finalInterpolation;
		else {
			// Make sure the percentile is between 0 and 1
			percentile = Mathf.Clamp (percentile, 0, 1);
			rounded = Mathf.Clamp (rounded, 0, 1);

			float secondHalf = (Mathf.Acos (rounded - 1) - Mathf.Acos ((1 - rounded) * Mathf.Sin (2 * Mathf.PI * numWaves * percentile + 3 * Mathf.PI / 2))) / (Mathf.Acos (rounded - 1) - Mathf.Acos (1 - rounded));

			// Calculate a triangle curve that begins and ends at the 'start' height and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			//float startTri = start + (start-height)*secondHalf;
			// Calculate a triangle curve that begins and ends at the 'end' height and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			//float endTri = end + (end-height)*secondHalf;
			// Calculate a sin curve that begins and ends at 0 and reaches a maximum of 'height', stretched to fit 'numWaves' of peaks into the 0-1 range
			float regTri = height * secondHalf;

			/*
		// If the distance along the curve is less than the blend limit, interpolate between the sin curve that starts at 'start' height and the sin curve that starts at 0
			if (percentile <= blendLimit)
				finalInterpolation = Mathf.Lerp (startTri, regTri, percentile / blendLimit);
			// If the distance along the curve is between the blend limits, use ony the values provided by the sin curve that starts at 0
			else if (percentile > blendLimit && percentile < 1f - blendLimit)
				*/
				finalInterpolation = regTri;

			/*
			// If the distance along the curve has reached the blend limit at the end, interpolate between the sin curve that end at 'end' height and the sin curve that starts at 0
			else
				finalInterpolation = Mathf.Lerp (endTri, regTri, (1f - percentile) / blendLimit);
			*/

			// Return the interpolated sin curves
			return finalInterpolation;
		}
	}

	public static float perlinEaseCurve (float percentile)
	{
		return 6 * Mathf.Pow (percentile, 5) - 15 * Mathf.Pow (percentile, 4) + 10 * Mathf.Pow (percentile, 3);
	}

	public static float cubicBezierEaseCurve (float x2, float y2, float x3, float y3,float xVal)
	{
		//float t1 = Mathf.Clamp (xVal, 0, 1);
		//float t2 = 1 - xVal;
		//float x1 = 0;
		float y1 = 0;
		//float x4 = 1;
		float y4 = 1;

		//float ease1 = 1f * t2 * t2 * t2 * x2;
		//float ease2 = 3f * t1 * t2 * t2 * y2;
		//float ease3 = 3f * t1 * t1 * t2 * x3;
		//float ease4 = 1f * t1 * t1 * t1 * y3;

		//float easeY1 = 1f * t2 * t2 * t2 * y1;
		//float easeY2 = 3f * t1 * t2 * t2 * y2;
		//float easeY3 = 3f * t1 * t1 * t2 * y3;
		//float easeY4 = 1f * t1 * t1 * t1 * y4;

		//float mid1x = Mathf.Lerp (x1, x2, xVal);
		//float mid1y = Mathf.Lerp (y1, y2, xVal);
		//float mid2x = Mathf.Lerp (x2, x3, xVal);
		//float mid2y = Mathf.Lerp (y2, y3, xVal);
		//float mid3x = Mathf.Lerp (x3, x4, xVal);
		//float mid3y = Mathf.Lerp (y3, y4, xVal);

		//float mid12y = Mathf.Lerp (mid1y, mid2y, xVal);
		//float mid12x = Mathf.Lerp (mid1x, mid2x, xVal);
		//float mid23y = Mathf.Lerp (mid2y, mid3y, xVal);
		//float mid23x = Mathf.Lerp (mid2x, mid3x, xVal);

		//float eased = Mathf.Lerp (mid12y, mid23y, xVal);
		float eased = lerpDown(new float[] {y1,y2,y3,y4},xVal);


		//float eased =  easeY1+easeY2+easeY3+easeY4;

		return Mathf.Clamp (eased, 0, 1);
	}

	public static float lerpDown (float[] x, float val)
	{
		if (x.Length == 2)
			return Mathf.Lerp (x [0], x [1],val);
		else if (x.Length < 2)
			return 0;
		else
		{
			float[] temp = new float[x.Length - 1];
			for(int i = 0; i < temp.Length; i++)
				temp [i] = Mathf.Lerp (x [i], x [i + 1], val);
			return lerpDown (temp, val);
		}
	}


	// Returns the distance between two points (x1,y1) (x2, y2) using the formula: |x1 - x2| + |y1 - y2|
	public static float manhattanDistance(Vector2 point1, Vector2 point2)
	{
		float dist1 = point2.x - point1.x;
		float dist2 = point2.y - point1.y;
		float combo = Mathf.Abs (dist1) + Mathf.Abs (dist2);
		return combo;
	}

	// Returns the distance between two points (x1,y1) (x2, y2) using the formula: SQRT((x1-x2)^2 + (y1-y2)^2)
	public static float euclideanDistance(Vector2 point1, Vector2 point2)
	{
		float dist1 = point2.x - point1.x;
		float dist2 = point2.y - point1.y;
		float combo = Mathf.Sqrt (Mathf.Pow (dist1, 2) + Mathf.Pow (dist2, 2));
		return combo;
	}

}
