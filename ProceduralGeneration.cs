using UnityEngine;
using System.Collections;
using System.Threading;
using System;
using System.Collections.Generic;

public class ProceduralGeneration 
{
	//static float initHeight = 0f;
	static float initDetailScale = 300f;
	//static float initHeightScale = 100f;
	static int boundary = 50;
	static int ageOfWorld = 50;
	static float bioMassMinimum = 0.1f;

	public static IEnumerator createHeightMap2()
	{
		bool fin = true;
		float sT = Time.realtimeSinceStartup;
		float[][] hm;

		lock (GameManager.tSets) 
		{
			hm = (float[][])GameManager.tSets.globalHeightMap.Clone();
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += hm.Length * hm.Length;
		}



		CustomThreadPool.Instance.QueueUserTask (() => 
			{
				GenerationMethods thisGen = new GenerationMethods();
				Debug.Log("Heightmap " + hm.Length + "x" + hm.Length + " started");
				for (int x = 0; x < hm.Length; x++)
				{
					for (int z = 0; z < hm[x].Length; z++) 
					{
						//hm[x][z] = initHeight + Mathf.PerlinNoise (x / initDetailScale, z / initDetailScale) * initHeightScale;
						hm[x][z] = thisGen.OctavePerlin((double)x / initDetailScale, (double)z / initDetailScale,0.0,3,2f);
					}

					lock (GameManager.lSets) 
					{
						GameManager.lSets.totalFilled += hm.Length;
					}
					Thread.Sleep (0);
				}
			},
			(ts) => 
			{
				fin = false;
			});

		while(fin) yield return new WaitForSeconds(0.5f);

		lock(GameManager.tSets)
		{
			GameManager.tSets.globalHeightMap = (float[][])hm.Clone();
			GameManager.tSets.isHeightSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log("Heightmap finished after " + eT + " seconds");
		}
	}

	public static IEnumerator createAlphamap()
	{
		float sT = Time.realtimeSinceStartup;
		bool fin = true;

		int amLength1 = 0;
		int amLength2 = 0;
		int amLength3 = 0;
		int zmLength1 = 0;
		int zmLength2 = 0;
		int numZones = 0;

		int[][] zm;
		float[][] wm;
		int textVars = CommonGameObjects.terrainTextureColor.GetLength (1);


		lock (GameManager.tSets) 
		{
			amLength1 = GameManager.tSets.globalAlphaMap.Length;
			amLength2 = GameManager.tSets.globalAlphaMap[1].Length;
			amLength3 = GameManager.tSets.globalAlphaMap[1][1].Length;
			zm = (int[][])GameManager.tSets.globalZoneMap.Clone();
			wm = (float[][])GameManager.tSets.globalWaterMap.Clone();
			zmLength1 = GameManager.tSets.globalZoneMap.Length;
			zmLength2 = GameManager.tSets.globalZoneMap[1].Length;
			numZones = GameManager.tSets.numZones;

		}

		float[][][] am = new float[amLength1][][];
		for (int i = 0; i < amLength1; i++) 
		{
			am[i] = new float[amLength2][];
			for (int j = 0; j < amLength2; j++)	am [i] [j] = new float[amLength3];
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += amLength1 * amLength2;
		}

		yield return new WaitForSeconds (0.01f);

		CustomThreadPool.Instance.QueueUserTask (() => 
			{
				Debug.Log("Alphamap " + amLength1 + "x" + amLength2 + "x" + amLength3 + " started");
				for (int x = 0; x < amLength1; x++) 
				{
					for (int z = 0; z < amLength2; z++) 
					{
						Vector3 zonemapIndex = GameManager.gamemapToGamemap(new Vector3(x,0,z), 3, 2);
						Vector3 watermapIndex = GameManager.gamemapToGamemap(new Vector3(x,0,z), 3, 5);

						int zoneMapInt = zm[(int)zonemapIndex.x][(int)zonemapIndex.z];
						float waterNum = zoneMapInt * textVars + (textVars-1) * wm[(int)watermapIndex.x][(int)watermapIndex.z];

						for(int i = 0; i < textVars * numZones; i++)
						{
							am[x][z][i] = textureDistribution(waterNum, i);
						}

					}
					lock (GameManager.lSets) 
					{
						GameManager.lSets.totalFilled += amLength1;
					}
					Thread.Sleep(0);
				}
			}, 
			(ts) =>
			{
				fin = false;
			});

		while (fin)	yield return new WaitForSeconds (0.5f);

		lock (GameManager.tSets) 
		{
			GameManager.tSets.globalAlphaMap = (float[][][])am.Clone ();
			GameManager.tSets.isAlphaSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log ("Alphamap finished after " + eT + " seconds");
		}
	}

	public static IEnumerator createZoneMap2()
	{
		bool fin = true;
		float sT = Time.realtimeSinceStartup;
		int numZones = 0;
		int length1 = 0;

		lock (GameManager.tSets)
		{
			numZones = GameManager.tSets.numZones;
			length1 = GameManager.tSets.globalHeightMap.Length;
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += length1 * length1;
		}

		int[][] zm = new int[length1][];
		for (int i = 0; i < length1; i++) zm [i] = new int[length1];

		Vector2[] voronoiPoints = new Vector2[numZones];

		for (int x = 0; x < numZones; x++) 
		{
			System.Random rnd = new System.Random ();

			int voronoiX = rnd.Next (boundary, length1 - boundary);
			int voronoiY = rnd.Next (boundary, length1 - boundary);

			voronoiPoints [x] = new Vector2 (voronoiX, voronoiY);

			for (int j = 0; j < x; j++)	if (voronoiPoints [x] == voronoiPoints [j])	x--;
		}



		CustomThreadPool.Instance.QueueUserTask (() => 
			{
				Debug.Log ("Zonemap " + length1 + "x" + length1 + " started");
				for (int x = 0; x < zm.Length; x++)
				{
					for (int z = 0; z < zm [x].Length; z++)
					{
						float shortestDistance = length1;
						for (int y = 0; y < numZones; y++)
						{
							float newDistance = Interpolations.manhattanDistance (new Vector2 (x, z), voronoiPoints [y]);

							if (newDistance <= shortestDistance)
							{
								zm [x] [z] = y;
								shortestDistance = newDistance;

							}
						}
					}

					lock (GameManager.lSets)
					{
						GameManager.lSets.totalFilled += length1;
					}
					Thread.Sleep (0);
				}
			},
			(ts) => 
			{
				fin = false;
			});

		while (fin) yield return new WaitForSeconds (0.5f);

		lock (GameManager.tSets) 
		{
			GameManager.tSets.globalZoneMap = (int[][])zm.Clone ();
			GameManager.tSets.isZoneSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log ("Zonemap finished after " + eT + " seconds");
		}
	}

	public static IEnumerator createWaterMap()
	{
		bool fin = true;
		float sT = Time.realtimeSinceStartup;
		int length1 = 0;
		int length2 = 0;

		lock (GameManager.tSets)
		{
			length1 = GameManager.tSets.globalWaterMap.Length;
			length2 = GameManager.tSets.globalWaterMap [0].Length;
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += length1 * length2;
		}

		float[][] wm = new float[length1][];
		for (int i = 0; i < length1; i++) wm [i] = new float[length1];

		CustomThreadPool.Instance.QueueUserTask (() => 
			{
				GenerationMethods thisGen = new GenerationMethods();
				Debug.Log ("Watermap " + length1 + "x" + length2 + " started");
				for (int x = 0; x < length1; x++)
				{
					for (int z = 0; z < length2; z++)
					{
						float jk = thisGen.OctavePerlin((double)x / length1 * 2f, (double)z / length2 * 2f,0.0,3,2f);
						jk = Interpolations.cubicBezierEaseCurve(3f,0f,-2f,1f,jk);

						wm [x][z] = jk;
					}

					lock (GameManager.lSets)
					{
						GameManager.lSets.totalFilled += length1;
					}
					Thread.Sleep (0);
				}
			},
			(ts) => 
			{
				fin = false;
			});

		while (fin) yield return new WaitForSeconds (0.5f);

		lock (GameManager.tSets) 
		{
			GameManager.tSets.globalWaterMap = (float[][])wm.Clone ();
			GameManager.tSets.isWaterSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log ("Watermap finished after " + eT + " seconds");
		}
	}

	public static IEnumerator createVeggieMap2()
	{
		Debug.Log("Creating Veggiemap!");
		yield return new WaitForSeconds(0.01f);
		bool fin = true;
		float sT = Time.realtimeSinceStartup;
		VeggieObject[][] vm1;
		int[][] zm;
		float[][] wm;//, soilMap;
		List<VeggieObject>[][] aboveGroundComp;
		List<VeggieObject>[][] belowGroundComp;

		lock (GameManager.tSets) 
		{
			vm1 = (VeggieObject[][])GameManager.tSets.globalVeggieMap.Clone();
			zm = (int[][])GameManager.tSets.globalZoneMap.Clone();
			wm = (float[][])GameManager.tSets.globalWaterMap.Clone ();

		}

		VeggieObject[][] seedMap = new VeggieObject[vm1.Length][];
		//soilMap = new float[vm1.Length][];
		for (int i = 0; i < vm1.Length; i++)
		{
			seedMap [i] = new VeggieObject[vm1 [i].Length];
			//soilMap [i] = new float[vm1 [i].Length];
		}

		aboveGroundComp = new List<VeggieObject>[vm1.Length][];
		belowGroundComp = new List<VeggieObject>[vm1.Length][];

		for (int i = 0; i < vm1.Length; i++)
		{
			aboveGroundComp [i] = new List<VeggieObject>[vm1 [i].Length];
			for (int j = 0; j < aboveGroundComp [i].Length; j++)
				aboveGroundComp [i] [j] = new List<VeggieObject> ();

			belowGroundComp [i] = new List<VeggieObject>[vm1 [i].Length];
			for (int j = 0; j < belowGroundComp [i].Length; j++)
				belowGroundComp [i] [j] = new List<VeggieObject> ();
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += vm1.Length * vm1.Length * (ageOfWorld*3+1);
		}

		yield return new WaitForSeconds(1f);

		CustomThreadPool.Instance.QueueUserTask (() => 
			{
				System.Random rand = new System.Random();

				Debug.Log("Veggiemap " + vm1.Length + "x" + vm1.Length + " started");
				for (int x = 0; x < vm1.Length; x++)
				{
					for (int z = 0; z < vm1[x].Length; z++) 
					{
						Vector3 zoneIndex = GameManager.gamemapToGamemap(new Vector3(x,0,z),6,2);
						int zoneNum = zm[(int)zoneIndex.x][(int)zoneIndex.z];

						seedMap[x][z] = CommonGameObjects.assignVeggieProps(rand.Next(0,5),zoneNum, GameManager.gamemapToWorld(new Vector3(x,0,z),6));
						//soilMap[x][z] =  (float)rand.NextDouble();
					}

					lock (GameManager.lSets) 
					{
						GameManager.lSets.totalFilled += vm1.Length;
					}
					Thread.Sleep (0);
				}

				for(int a = 0; a < ageOfWorld; a++)
				{

					for (int x = 0; x < vm1.Length; x++)
					{
						for (int z = 0; z < vm1[x].Length; z++) 
						{
							VeggieObject currentV = seedMap[x][z];
							if(currentV != null)
							{
								float bioM = currentV.bioMass;
								float convConst = (1/CommonGameObjects.veggieProperties [currentV.plantID,0]) / (1/CommonGameObjects.veggieProperties [currentV.plantID, 2]);
								float roi = Mathf.Pow( 6 * bioM/Mathf.PI/convConst, 1f/3f)/2;
								float aboveRoI=CommonGameObjects.veggieProperties [currentV.plantID,13]*roi;
								float belowRoI=CommonGameObjects.veggieProperties [currentV.plantID,14]*roi;
								currentV.width = roi*2;

								float thisHeight = Mathf.Pow( 6 *currentV.bioMass/Mathf.PI/Mathf.Pow(1/convConst,2), 1f/3f);
								currentV.height = thisHeight;

								int smallerAI = (int)Mathf.Min(x,Mathf.Ceil(aboveRoI));
								int smallerAJ = (int)Mathf.Min(z,Mathf.Ceil(aboveRoI));
								int largerAI = (int)Mathf.Min(vm1.Length - x - 1, Mathf.Ceil(aboveRoI));
								int largerAJ = (int)Mathf.Min(vm1.Length - z - 1, Mathf.Ceil(aboveRoI));

								int smallerBI = (int)Mathf.Min(x,Mathf.Ceil(belowRoI));
								int smallerBJ = (int)Mathf.Min(z,Mathf.Ceil(belowRoI));
								int largerBI = (int)Mathf.Min(vm1.Length - x - 1, Mathf.Ceil(belowRoI));
								int largerBJ = (int)Mathf.Min(vm1.Length - z - 1, Mathf.Ceil(belowRoI));

								for(int i = -smallerAI; i <= largerAI; i++)
								{
									for(int j = -smallerAJ; j <= largerAJ; j++)
									{
										float dist = calcPlantDistance (x, z, x + i, z + j);
										if(dist <= aboveRoI) aboveGroundComp[x+i][z+j].Add(currentV);
									}
								}

								for(int i = -smallerBI; i <= largerBI; i++)
								{
									for(int j = -smallerBJ; j <= largerBJ; j++)
									{
										float dist = calcPlantDistance (x, z, x + i, z + j);
										if(dist <= belowRoI) belowGroundComp[x+i][z+j].Add(currentV);
									}
								}
							}
						}

						lock (GameManager.lSets) 
						{
							GameManager.lSets.totalFilled += vm1.Length;
						}
						Thread.Sleep (0);
					}

					for (int x = 0; x < vm1.Length; x++)
					{
						for (int z = 0; z < vm1[x].Length; z++) 
						{
							float totalSoilComp = 0;
							float totalWaterComp = 0;
							float totalLightComp = 0;

							foreach(VeggieObject vO in aboveGroundComp[x][z])
							{
								totalLightComp += Mathf.Pow(vO.height+1,6);
							}

							foreach(VeggieObject vO in belowGroundComp[x][z])
							{
								totalWaterComp += CommonGameObjects.veggieProperties[vO.plantID,4];
								totalSoilComp += CommonGameObjects.veggieProperties[vO.plantID,5];
							}

							foreach(VeggieObject vO in aboveGroundComp[x][z])
							{
								float thisLightComp = Mathf.Pow(vO.height+1,6);
								float lightForGrowth = thisLightComp/totalLightComp;

								Vector3 compIndex = GameManager.worldToGamemap(vO.pos, 6);
								float distance = calcPlantDistance(x,z,(int)compIndex.x,(int)compIndex.z);

								vO.lightEnergy += lightForGrowth*resourceEfficiencyFromDistance(distance,vO.plantID,true);
							}

							foreach(VeggieObject vO in belowGroundComp[x][z])
							{
								float thisWaterComp = CommonGameObjects.veggieProperties[vO.plantID,4];
								float waterForGrowth = thisWaterComp/totalWaterComp*wm[x][z];
								//float thisSoilComp = CommonGameObjects.veggieProperties[vO.plantID,5];
								//float soilForGrowth = thisSoilComp/totalSoilComp*soilMap[x][z];

								Vector3 compIndex = GameManager.worldToGamemap(vO.pos, 6);
								float distance = calcPlantDistance(x,z,(int)compIndex.x,(int)compIndex.z);
									
								//vO.bioMass += (waterForGrowth+soilForGrowth)*resourceEfficiencyFromDistance(distance,vO.plantID,false);
								vO.waterEnergy += (waterForGrowth)*resourceEfficiencyFromDistance(distance,vO.plantID,false);
							}
						}

						lock (GameManager.lSets) 
						{
							GameManager.lSets.totalFilled += vm1.Length;
						}
						Thread.Sleep(0);
					}

					for (int x = 0; x < vm1.Length; x++)
					{
						for (int z = 0; z < vm1[x].Length; z++) 
						{
							seedMap[x][z] = growPlant(seedMap[x][z]);
						}
						lock (GameManager.lSets) 
						{
							GameManager.lSets.totalFilled += vm1.Length;
						}
						Thread.Sleep(0);
					}
				}

			},
			(ts) => 
			{
				fin = false;
			});

		while(fin) yield return new WaitForSeconds(0.5f);

		lock(GameManager.tSets)
		{
			GameManager.tSets.globalVeggieMap = (VeggieObject[][])seedMap.Clone();
			GameManager.tSets.isVeggieSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log("Veggiemap finished after " + eT + " seconds");
		}
	}

	public static VeggieObject growPlant(VeggieObject thisVeggie)
	{
		if (thisVeggie != null)
		{
			float lightAff = CommonGameObjects.veggieProperties [thisVeggie.plantID, 6];
			float waterAff = CommonGameObjects.veggieProperties [thisVeggie.plantID, 4];
			//float soilAff = CommonGameObjects.veggieProperties [thisVeggie.plantID, 5];
			float affFactor = Mathf.Min(thisVeggie.lightEnergy/lightAff,thisVeggie.waterEnergy/waterAff);
			//float affFactor = Mathf.Min(thisVeggie.lightEnergy/lightAff,thisVeggie.waterEnergy/waterAff, thisVeggie.soilEnergy/soilAff);
 
			float totalEnergy = affFactor*lightAff + affFactor*waterAff;
			//float totalEnergy = affFactor*lightAff + affFactor*waterAff+ affFactor*soilAff;

			thisVeggie.bioMass += totalEnergy;

			float usingEnegry = thisVeggie.bioMass * CommonGameObjects.veggieProperties [thisVeggie.plantID, 12];
			thisVeggie.bioMass = Mathf.Clamp(thisVeggie.bioMass - usingEnegry,0,500);

			thisVeggie.lightEnergy = 0;
			thisVeggie.waterEnergy = 0;
			//thisVeggie.soilEnergy = 0;

			if (thisVeggie.bioMass < bioMassMinimum)
			{
				thisVeggie = null;
			}
		}
		return thisVeggie;
	}

	public static float calcPlantDistance(int x1, int z1, int x2, int z2)
	{
		Vector3 compWorld = GameManager.gamemapToWorld(new Vector3(x1,0,z1), 6);
		Vector3 thisWorld = GameManager.gamemapToWorld(new Vector3(x2,0,z2), 6);
		float mag = Mathf.Pow (compWorld.x - thisWorld.x, 2) + Mathf.Pow (compWorld.z - thisWorld.z, 2);
		float distance = Mathf.Pow(mag,1f/2f);
		return distance;
	}

	public static float resourceEfficiencyFromDistance(float distance, int plantID, bool isAbove)
	{
		float coeff = isAbove ? CommonGameObjects.veggieProperties [plantID, 7] : CommonGameObjects.veggieProperties [plantID, 8];

		float factor = Mathf.Exp (-1f*distance/coeff);

		return factor;
	}

	public static float textureDistribution(float coeff, float xVal)
	{
		float theDist = 0;

		if(xVal > coeff - 1f && xVal < coeff + 1f)
		{
			theDist = Mathf.Cos (Mathf.PI * xVal - coeff * Mathf.PI) / 2f + 0.5f;
		}

		return theDist;
	}

	/* // Revised map creation to divide each map into individual chunks, did not actually take less time in testing
	public static IEnumerator createHeightMap1()
	{
		float sT = Time.realtimeSinceStartup;
		object countLock = new object();
		int chunkCounter = 0;
		int hmLength;
		List<FloatArrayChunk> chunkList = new List<FloatArrayChunk>();
		lock (GameManager.tSets) 
		{
			hmLength = GameManager.tSets.globalHeightMap.Length;
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += hmLength * hmLength;
		}

		Debug.Log("Heightmap " + hmLength + "x" + hmLength + " started");

		for(int x = 0; x < hmLength;x++)
		{
			FloatArrayChunk chunk = new FloatArrayChunk ();
			chunk.num = x;
			chunk.arr = new float[hmLength];
			chunk.task = () => 
			{
				for (int z = 0; z < chunk.arr.Length; z++) 
				{
					chunk.arr [z] = initHeight + Mathf.PerlinNoise (x / initDetailScale, z / initDetailScale) * initHeightScale;
					Thread.Sleep(0);
				}

				lock (GameManager.lSets) 
				{
					GameManager.lSets.totalFilled += hmLength;
				}
			};
			chunk.act = (ts) => 
			{
				lock(countLock)	chunkCounter++;
			};
			chunkList.Add (chunk);
		}

		foreach (FloatArrayChunk chunk in chunkList) CustomThreadPool.Instance.QueueUserTask (chunk.task, chunk.act);

		int safe = 0;
		while (safe < hmLength - 1) 
		{
			yield return new WaitForSeconds (0.5f);
			lock (countLock) safe = chunkCounter;
		}

		lock(GameManager.tSets)
		{
			foreach(FloatArrayChunk chunk in chunkList) GameManager.tSets.globalHeightMap[chunk.num] = (float[])chunk.arr.Clone();
			GameManager.tSets.isHeightSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log("Heightmap finished after " + eT + " seconds");
		}
	}

	public static IEnumerator createZoneMap1()
	{
		float sT = Time.realtimeSinceStartup;
		object countLock = new object();
		int chunkCounter = 0;
		List<IntArrayChunk> chunkList = new List<IntArrayChunk> ();
		int numZones = 0;
		int length1 = 0;
		lock (GameManager.tSets) 
		{
			numZones = GameManager.tSets.numZones;
			length1 = GameManager.tSets.globalHeightMap.Length;
		}

		lock (GameManager.lSets) 
		{
			GameManager.lSets.totalToFill += length1 * length1;
		}

		Vector2[] voronoiPoints = new Vector2[numZones];

		for (int x = 0; x < numZones; x++) 
		{
			System.Random rnd = new System.Random();

			int voronoiX = rnd.Next(boundary, length1 - boundary);
			int voronoiY = rnd.Next(boundary, length1 - boundary);

			voronoiPoints [x] = new Vector2 (voronoiX, voronoiY);

			for (int j = 0; j < x; j++) if (voronoiPoints [x] == voronoiPoints [j]) x--;
		}

		Debug.Log("Zonemap " + length1 + "x" + length1 + " started");

		for (int x = 0; x < length1; x++) 
		{
			IntArrayChunk chunk = new IntArrayChunk ();
			chunk.num = x;
			chunk.arr = new int[length1];
			chunk.task = () =>
			{
				for (int z = 0; z < chunk.arr.Length; z++)
				{
					float shortestDistance = length1;
					for (int y = 0; y < numZones; y++) 
					{
						float newDistance = Interpolations.manhattanDistance (new Vector2 (x, z), voronoiPoints [y]);

						if (newDistance <= shortestDistance)
						{
							chunk.arr [z] = y;
							shortestDistance = newDistance;
							Thread.Sleep (0);
						}
					}
				}

				lock (GameManager.lSets) 
				{
					GameManager.lSets.totalFilled += length1;
				}
			};

			chunk.act = (ts) =>
			{
				lock(countLock)	chunkCounter++;
			};

			chunkList.Add (chunk);
		}

		foreach (IntArrayChunk chunk in chunkList) CustomThreadPool.Instance.QueueUserTask (chunk.task, chunk.act);

		int safe = 0;
		while (safe < length1 - 1)
		{
			yield return new WaitForSeconds (0.5f);
			lock (countLock) safe = chunkCounter;
		}

		lock(GameManager.tSets)
		{
			foreach(IntArrayChunk chunk in chunkList) GameManager.tSets.globalZoneMap[chunk.num] = (int[])chunk.arr.Clone();
			GameManager.tSets.isZoneSet = true;
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			Debug.Log ("Zonemap finished after " + eT + " seconds");
		}
	}

	class FloatArrayChunk
	{
		public int num;
		public float[] arr;
		public UserTask task;
		public Action<TaskStatus> act;
	}

	class IntArrayChunk
	{
		public int num;
		public int[] arr;
		public UserTask task;
		public Action<TaskStatus> act;
	}
	*/
}