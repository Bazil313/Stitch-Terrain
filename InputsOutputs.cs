using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class InputsOutputs
{

	public static void write1DArraytoBinaryFile(string resourcePath, float[] serializedArray)
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.dataPath + resourcePath,FileMode.Open);
		bf.Serialize (file, serializedArray);
		file.Close ();

	}
		
	public static float[] read1DArrayfromBinaryFile(string resourcePath)
	{
		if (File.Exists (Application.dataPath + resourcePath))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + resourcePath, FileMode.Open);
			float[] data = (float[])bf.Deserialize (file);
			file.Close ();
			return data;
		} 
		else
		{
			return new float[]{0f};
		}
	}

	public static void write2DArraytoBinaryFile(string resourcePath, float[][] serializedArray)
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.dataPath + resourcePath,FileMode.Open);
		bf.Serialize (file, serializedArray);
		file.Close ();

	}

	public static void write2DArraytoBinaryFile(string resourcePath, int[][] serializedArray)
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.dataPath + resourcePath,FileMode.Open);
		bf.Serialize (file, serializedArray);
		file.Close ();

	}

	public static float[][] read2DFloatArrayfromBinaryFile(string resourcePath)
	{
		if (File.Exists (Application.dataPath + resourcePath))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + resourcePath, FileMode.Open);
			float[][] data = (float[][])bf.Deserialize (file);
			file.Close ();
			return data;
		} 
		else
		{
			return new float[][]{new float[]{0f},new float[]{0f}};
		}
	}

	public static int[][] read2DIntArrayfromBinaryFile(string resourcePath)
	{
		if (File.Exists (Application.dataPath + resourcePath))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + resourcePath, FileMode.Open);
			int[][] data = (int[][])bf.Deserialize (file);
			file.Close ();
			return data;
		} 
		else
		{
			return new int[][]{new int[]{0},new int[]{0}};
		}
	}

	public static void write3DArraytoBinaryFile(string resourcePath, float[][][] serializedArray)
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.dataPath + resourcePath,FileMode.Open);
		bf.Serialize (file, serializedArray);
		file.Close ();

	}

	public static void write3DArraytoBinaryFile(string resourcePath, int[][][] serializedArray)
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.dataPath + resourcePath,FileMode.Open);
		bf.Serialize (file, serializedArray);
		file.Close ();

	}

	public static float[][][] read3DFloatArrayfromBinaryFile(string resourcePath)
	{
		if (File.Exists (Application.dataPath + resourcePath))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + resourcePath, FileMode.Open);
			float[][][] data = (float[][][])bf.Deserialize (file);
			file.Close ();
			return data;
		} 
		else
		{
			return new float[][][]{new float[][]{new float[]{0f},new float[]{0f}},new float[][]{new float[]{0f},new float[]{0f}}};
		}
	}

	public static int[][][] read3DIntArrayfromBinaryFile(string resourcePath)
	{
		if (File.Exists (Application.dataPath + resourcePath))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + resourcePath, FileMode.Open);
			int[][][] data = (int[][][])bf.Deserialize (file);
			file.Close ();
			return data;
		} 
		else
		{
			return new int[][][]{new int[][]{new int[]{0},new int[]{0}},new int[][]{new int[]{0},new int[]{0}}};
		}
	}

}
