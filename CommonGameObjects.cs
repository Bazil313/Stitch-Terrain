using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;
using System.Collections.Generic;

public class CommonGameObjects
{
	#region File Paths

	private static string[] playerPrefabs = 
	{
		//"Characters/RigidBodyFPSController"
		"Characters/FPSController"
	};

	private static string[] postProcessingProfiles = 
	{
		"Post Processing/Stiched Terrain Profile 20170724"
	};

	public static string[,] terrainTextureColor = 
	{
		{"Terrain/Textures/ground-CrackedEarthCloser_Color-unity", "Terrain/Textures/ground-DryDirt-SmallROcks_Color-unity"},
		{"Terrain/Textures/ground-WinterOakLeaves_Color-unity","Terrain/Textures/ground-ShortGrass_Color-unity"},
		//{"Terrain/Textures/ground-SpringSnow_Color-unity", "Terrain/Textures/ground-GreenShoots_Color-unity"}
		{"Terrain/Textures/ground-SpringSnowGrass_Color-unity","Terrain/Textures/ground-SpringSnow_Color-unity"}
	};

	public static string[,] terrainTextureNormal = 
	{
		{"Terrain/Textures/ground-CrackedEarthCloser_Normal-unity", "Terrain/Textures/ground-DryDirt-SmallROcks_Normal-unity"},
		{"Terrain/Textures/ground-WinterOakLeaves_Normal-unity","Terrain/Textures/ground-ShortGrass_Normal-unity"},
		//{"Terrain/Textures/ground-SpringSnow_Normal-unity", "Terrain/Textures/ground-GreenShoots_Color-unity"}
		{"Terrain/Textures/ground-SpringSnowGrass_Normal-unity","Terrain/Textures/ground-SpringSnow_Normal-unity"}
	};

	public static float[,,] textureProperties = 
	{
		// 0-Tile Size, 1-Smoothness, 2-Metallic
		{{ 2,0.1f, 0.5f},{ 2,0.1f, 0.5f}}, //0
		{{ 2,0.1f, 0.5f},{ 2,0.1f, 0.8f}}, //1
		//{{ 2,0.1f, 0.8f},{ 2,0.1f, 0.5f}} //2
		{{ 2,0.1f, 0.7f},{ 2,0.1f, 0.7f}} //2
	};

	public static string[,] detailFiles = 
	{
		{"Terrain/DetailTextures/grass03"},
		{"Terrain/DetailTextures/grass02"},
		{"Terrain/DetailTextures/3td_SwordGrass_01"}
	};
		
	public static string[,] vegetationFiles = 
	{
		{"Vegetation/Free_SpeedTrees/Palm_Desktop"}, //0
		{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/tropicalPlant_01"}, //1
		{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/tropicalPlant_04"}, //2
		{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/grass_Gamba"}, //3
		{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/tree_Banana_01"}, //4
		{"Vegetation/Tree9/Tree9_4 WithCollider"}, //5
		{"Vegetation/Meshes/Bushes/Bush 04/Bush 04 prefab"}, //6
		{"Vegetation/Free_SpeedTrees/Broadleaf_Desktop"}, //7
		{"Vegetation/Meshes/Bushes/Bush 01/Bush 01 prefab"}, //8
		{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/plant_Philodendron"}, //9
		{"Vegetation/Meshes/Bushes/Bush 05/Bush 05 prefab"}, //10
		{"Vegetation/Meshes/Bushes/Bush 02/Bush 02 prefab"}, //11
		{"Vegetation/Foliage Free/models/pine1aWithCollider"}, //12
		{"Vegetation/Free_SpeedTrees/Conifer_Desktop"}, //13
		{"Vegetation/Meshes/Bushes/Bush 03/Bush 03 prefab Revised Collider"} //14
		//{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/groundCover_02_Mixed"}, //15
		//{"Vegetation/Odds_N_Ends Series - Tropical Foliage v1.0/PreFabs/groundCover_01_Epimedium"}, //16
		//{"Vegetation/Olive_Tree/Olive_Prefab/Olive_Tree_Prefab"}, //17
	};

	public static float[,] veggieProperties = 
	{
		// 0-Vertical Scale to Size, 1-Max Vertical Scale, 2-Horizontal Scale to Size, 3-Max Horizontal Scale, 4-Water Affinity, 
		// 5-Soil Affinity, 6-Light Affinity, 7-Above Ground Coeff, 8-Below Ground Coeff, 9-Starting Biomass, 10-collision enabled, 
		// 11-placement chance, 12-biomass conversion, 13-above RoI multiplier, 14-below RoI multiplier
		{ 1f/ 3.4f,5, 1f/ 2.7f,5,0.5f,0.33f,0.5f,  7,  5,0.1f,1,0.040f,0.05f,1f,1.2f}, //0
		{ 1f/ 1.8f,5, 1f/ 1.9f,5,0.5f,0.33f,0.5f,  2,  2,0.1f,1,0.100f,0.15f,1f,1.2f}, //1
		{ 1f/ 0.4f,5, 1f/ 1.1f,5,0.5f,0.33f,0.5f,  1,  1,0.1f,0,0.200f,0.85f,1f,1.2f}, //2
		{ 1f/ 2.2f,5, 1f/ 2.7f,5,0.5f,0.33f,0.5f,  1,  1,0.1f,0,0.200f,0.30f,1f,1.2f}, //3
		{ 1f/ 2.3f,5, 1f/ 3.5f,5,0.5f,0.33f,0.5f,  8,  8,0.1f,0,0.040f,0.10f,1f,1.2f}, //4
		{ 1f/11.4f,5, 1f/10.6f,5,0.5f,0.33f,0.5f,  7,  7,0.1f,0,0.050f,0.18f,1f,1.2f}, //5
		{ 1f/ 6.3f,5, 1f/ 8.1f,5,0.5f,0.33f,0.5f,  2,  2,0.1f,0,0.200f,0.30f,1f,1.2f}, //6
		{ 1f/10.5f,5, 1f/11.6f,5,0.5f,0.33f,0.5f, 10, 15,0.1f,1,0.020f,0.05f,1f,1.2f}, //7
		{ 1f/ 7.9f,5, 1f/ 7.0f,5,0.5f,0.33f,0.5f,  3,  3,0.1f,0,0.200f,0.30f,1f,1.2f}, //8
		{ 1f/ 0.6f,5, 1f/ 1.3f,5,0.5f,0.33f,0.5f,  1,  1,0.1f,0,0.200f,0.60f,1f,1.2f}, //9
		{ 1f/ 5.4f,5, 1f/ 9.2f,5,0.5f,0.33f,0.5f,  2,  2,0.1f,0,0.200f,0.35f,1f,1.2f}, //10
		{ 1f/ 6.5f,5, 1f/ 8.9f,5,0.5f,0.33f,0.5f,  2,  2,0.1f,0,0.200f,0.30f,1f,1.2f}, //11
		{ 1f/10.8f,5, 1f/ 9.8f,5,0.5f,0.33f,0.5f,  5,  5,0.1f,1,0.060f,0.12f,1f,1.2f}, //12
		{ 1f/14.9f,5, 1f/ 8.1f,5,0.5f,0.33f,0.5f,  8, 12,0.1f,1,0.150f,0.02f,1f,1.2f}, //13
		{ 1f/ 7.7f,5, 1f/10.2f,5,0.5f,0.33f,0.5f,  2,  2,0.1f,1,0.100f,0.15f,1f,1.2f}  //14
		//{ 1f/ 0.3f,5, 1f/ 1.3f,5,0.1f,0.1f,0.1f,  1,  1,0.1f,0,0.100f,0.90f,1f,0.5f}, //15
		//{ 1f/ 0.2f,5, 1f/ 2.0f,5,0.1f,0.1f,0.1f,  1,  1,0.1f,0,0.200f,0.97f,1f,1.2f}, //16
		//{ 1f/   1f,5, 1f/   1f,5,0.1f,0.1f,0.1f,  2,  2,0.1f,0,0.100f,0.30f,1f,1.2f}, //17
	};

	private static string[] musicAudioFiles = 
	{
		"Audio/Music/Back Oboe"
	};

	#endregion

	private static System.Random rand = new System.Random ();

 	// Constructor
	public CommonGameObjects()
	{

	}
		
	public static Camera createCamera(string cameraName, Vector3 pos, Quaternion rot, CameraClearFlags ccf, Color col)
	{
		removeAllTaggedObjects (new string[]{"Camera","MainCamera"});
		GameObject camera = new GameObject();
		camera.transform.position = pos;
		camera.transform.rotation = rot;
		Camera theCam = camera.AddComponent<Camera> ();
		camera.AddComponent<FlareLayer> ();
		camera.AddComponent<GUILayer> ();
		AudioListener al = camera.AddComponent<AudioListener> ();
		theCam.backgroundColor = col;
		theCam.clearFlags = ccf;
		camera.name = cameraName;
		camera.tag = "Camera";

		lock (GameManager.sSets)
		{
			GameManager.sSets.currentListener = al;
		}

		return theCam;
	}

	public static TerrainTile createTerrainTile(GameObject terrainTile, int tileID)
	{
		TerrainTile tTile = new TerrainTile ();
		tTile.tileObject = terrainTile;
		tTile.idNum = tileID;
		Terrain t = terrainTile.GetComponent<Terrain> ();
		tTile.heightmapResolution = t.terrainData.heightmapResolution;
		tTile.heightmapHeight = t.terrainData.heightmapHeight;
		tTile.alphamapResolution = t.terrainData.alphamapResolution;
		tTile.alphamapLayers = t.terrainData.alphamapLayers;
		tTile.detailmapResolution = t.terrainData.detailResolution;
		tTile.detailmapLayers = t.terrainData.detailPrototypes.Length;
		lock (GameManager.tSets) 
		{
			tTile.veggiemapResolution = GameManager.tSets.veggieMapResolution;
		}
		tTile.tempHeightmap = new float[tTile.heightmapResolution, tTile.heightmapResolution];
		tTile.tempAlphamap = new float[tTile.alphamapResolution, tTile.alphamapResolution,t.terrainData.alphamapLayers];
		tTile.tempDetailmap = new int[tTile.detailmapLayers][,];
		for(int i = 0; i < tTile.detailmapLayers; i++)
			tTile.tempDetailmap[i] = new int[tTile.detailmapResolution , tTile.detailmapResolution];
		tTile.tempPosition = terrainTile.transform.position;
		tTile.availableForUpdate = true;
		tTile.showTile = false;
		tTile.needsUpdate = false;

		return tTile;
	}

	public static GameObject createTerrain(string name, Vector3 pos, TerrainData data, Transform parentObject)
	{
		GameObject terrain = new GameObject ();
		terrain.transform.SetParent (parentObject);
		terrain.name = name;
		terrain.tag = "Terrain";
		terrain.transform.position = pos;
		TerrainCollider tc = terrain.AddComponent<TerrainCollider> ();
		Terrain theTerrain = terrain.AddComponent<Terrain>();
		theTerrain.terrainData = data;
		tc.terrainData = data;

		return terrain;
	}

	public static TerrainData createTerrainData(string name, int heightmapR, Vector3 size, int alphamapR, int detailR)
	{
		TerrainData data = new TerrainData();
		data.name = name;
		data.heightmapResolution = heightmapR;
		data.size = size;
		data.alphamapResolution = alphamapR;
		data.SetDetailResolution(detailR,16);
		data.wavingGrassSpeed = 0.1f;
		data.wavingGrassStrength = 0.1f;
		data.wavingGrassAmount = 0.1f;
		int numTexts = terrainTextureColor.GetLength (0);
		int numVarTexts = terrainTextureColor.GetLength (1);

		SplatPrototype[] getTextures = new SplatPrototype[numTexts*numVarTexts];
		DetailPrototype[] getDetails = new DetailPrototype[detailFiles.GetLength (0)];

		for (int i = 0; i < numTexts; i++)
		{
			for (int j = 0; j < numVarTexts; j++)
			{
				SplatPrototype spl = new SplatPrototype ();
				spl.texture = Resources.Load<Texture2D> (terrainTextureColor [i,j]);
				spl.normalMap = Resources.Load<Texture2D> (terrainTextureNormal[i,j]);
				spl.metallic = textureProperties[i,j,2];
				spl.smoothness = textureProperties[i,j,1];
				spl.tileSize = new Vector2 (textureProperties[i,j,0], textureProperties[i,j,0]);
				getTextures [i*numVarTexts+j] = spl;
			}
		}

		for (int i = 0; i < detailFiles.GetLength (0); i++)
		{
			DetailPrototype j = new DetailPrototype ();
			j.prototypeTexture = Resources.Load<Texture2D> (detailFiles [i,0]);
			j.renderMode = DetailRenderMode.Grass;
			j.healthyColor = new Color (230f/255f,255f/255f,212f/255f,255f/255f);
			j.dryColor = new Color (253f/255f,255f/255f,218f/255f,255f/255f);
			j.noiseSpread = .1f;
			j.minHeight = 0.2f;
			j.maxHeight = 0.3f;
			j.minWidth = 0.2f;
			j.maxWidth = 0.3f;
			getDetails [i] = j;
		}

		data.splatPrototypes = getTextures;
		data.detailPrototypes = getDetails;
		data.RefreshPrototypes ();
		return data;
	}

	public static GameObject createPlayer(string name, Vector3 pos, Quaternion rot, int selector, int processingProfile)
	{
		removeAllTaggedObjects (new string[]{"Camera","MainCamera"});

		GameObject player = GameObject.Instantiate (Resources.Load<GameObject> (playerPrefabs [selector]));
		GameObject playerChild = player.transform.GetChild(0).gameObject;
		playerChild.AddComponent<PostProcessingBehaviour> ();
		PostProcessingProfile thisProf = Resources.Load<PostProcessingProfile> (postProcessingProfiles[processingProfile]);
		playerChild.GetComponent<PostProcessingBehaviour> ().profile = thisProf;

		player.name = name;
		player.tag = "Player";

		return player;
	}

	public static GameObject createVeggie(string veggieName, Vector3 pos, Quaternion rot, int idNum, Transform parentObject)
	{
		GameObject vegetable = GameObject.Instantiate (Resources.Load<GameObject> (vegetationFiles [idNum,0]));
		vegetable.transform.SetParent (parentObject);
		vegetable.AddComponent<SeePlantProperties> ();
		vegetable.transform.position = pos;
		vegetable.transform.rotation = rot;
		bool col = veggieProperties [idNum, 10] == 1 ? true : false;
		try
		{
			vegetable.GetComponent<MeshCollider> ().enabled = col;
		}
		catch {}

		vegetable.name = veggieName;
		vegetable.tag = "Veggie";

		return vegetable;
	}

	public static VeggieObject assignVeggieProps(int idNum, int zone, Vector3 pos)
	{
		int theIDNum = idNum + zone * 5;
		float isPlaced = (float)rand.NextDouble ();
		if (isPlaced < veggieProperties[theIDNum,11]) 
		{
			VeggieObject thisVeggie = new VeggieObject ();
			thisVeggie.plantID = theIDNum;
			int yRot = rand.Next (0, 360);
			thisVeggie.rot = Quaternion.Euler(new Vector3(0,yRot,0));
			thisVeggie.zoneNum = zone;
			thisVeggie.bioMass = 1f;
			thisVeggie.pos = pos;
			return thisVeggie;
		} 
		else
			return null;
	}

	public static GameObject createLight(string lightName, Vector3 pos, Quaternion rot, LightType theType, LightShadows theShadows, Color col)
	{
		GameObject lights = new GameObject ();
		lights.transform.position = pos;
		lights.transform.rotation = rot;
		lights.tag = "Light";
		Light theLight = lights.AddComponent<Light> ();
		theLight.name = lightName;
		theLight.shadows = theShadows;
		theLight.type = theType;
		theLight.color = col;
		return lights;
	}

	public static GameObject getNewCanvas()
	{
		GameObject canvasGO = GameObject.FindGameObjectWithTag ("Canvas");

		if (canvasGO == null)
			canvasGO = GUIObjects.createCanvas (getCamera (), "Menu Canvas");
		else 
		{	 
			removeAllTaggedObjects (new string[]{ "Button", "Text", "Image"});
			GUIObjects.addBackgroundImage (canvasGO, "Empty Image", Vector2.zero, new Vector2 (1440, 900), 0);
		}

		return canvasGO;
	}

	public static Camera getCamera()
	{
		GameObject cameraGO = GameObject.FindGameObjectWithTag ("MainCamera");
		Camera camera;

		if (cameraGO == null)
			cameraGO = GameObject.FindGameObjectWithTag ("Camera");

		if (cameraGO == null)
			camera = createCamera ("Camera", new Vector3 (0, -500, 0), Quaternion.identity, CameraClearFlags.Color, Color.black);
		else
			camera = cameraGO.GetComponent<Camera> ();

		return camera;
	}

	public static AudioSource newAudioSource(string name, GameObject go, int selector, bool looping, float vol, bool is3D)
	{
		AudioSource aS = go.AddComponent<AudioSource> ();
		aS.name = name;
		aS.tag = "Music Source";
		aS.clip = Resources.Load<AudioClip> (musicAudioFiles [selector]);
		aS.loop = looping;
		aS.volume = vol;
		if (!is3D)
			aS.spatialBlend = 0;

		return aS;
	}

	public static void removeAllTaggedObjects(string[] tags)
	{
		foreach(string tag in tags)
		{
			GameObject[] obs = GameObject.FindGameObjectsWithTag (tag);
			foreach (GameObject o in obs)
			{
				//Transform[] chils = o.GetComponentsInChildren<Transform> ();
				//foreach (Transform t in chils) 
				//{
				//	GameObject.DestroyImmediate (t.gameObject);
				//}
				GameObject.DestroyImmediate (o);
			}
		}
	}
		
}

public class TreeObject
{
	public GameObject tr;
	public int tileNum;
	public int zoneNum;
	public bool used;
}

public class BushObject
{
	public GameObject br;
	public int tileNum;
	public int zoneNum;
	public bool used;
}

public class GrassObject
{
	public GameObject gr;
	public int tileNum;
	public int zoneNum;
	public bool used;
}

public class VeggieObject
{
	public GameObject vr;
	public int plantID;
	public float bioMass;
	public Quaternion rot;
	public int tileNum;
	public int zoneNum;
	public bool used;
	public Vector3 pos;
	public float height;
	public float width;
	public float lightEnergy;
	public float soilEnergy;
	public float waterEnergy;
}

public class TerrainTile
{
	public GameObject tileObject;
	public int idNum;
	public Vector3 actualPosition;
	public Vector3 tempPosition;
	public bool availableForUpdate;
	public bool showTile;
	public bool needsUpdate;
	public int heightmapResolution;
	public float heightmapHeight;
	public int alphamapResolution;
	public int alphamapLayers;
	public int detailmapResolution;
	public int detailmapLayers;
	public int veggiemapResolution;
	public float[,] tempHeightmap;
	public float[,,] tempAlphamap;
	public int[][,] tempDetailmap;
}