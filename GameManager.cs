using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class GameManager : MonoBehaviour 
{
	
	#region Global Variables

	public static GameManager Instance;
	public static CustomThreadPool threads;
	public static MainTerrainSettings tSets = new MainTerrainSettings();
	public static LoadingBarSettings lSets = new LoadingBarSettings();
	public static MainPlayerSettings pSets = new MainPlayerSettings();
	public static bool menuState = false;
	public static bool gameState = false;

	#endregion

	#region Application Build

	// Make sure this GameManager is set up as a singleton
	void Awake () 
	{
		if (Instance == null) 
		{
			Instance = this;
			DontDestroyOnLoad (this.gameObject);
		} 
		else Destroy (this.gameObject);
	}

	// Initialize settings and thread pool
	void Start ()
	{
		threads = CustomThreadPool.Instance;
		initializeTerrainSettings (tSets);
		initializeLoadingBar (lSets);
		initializePlayer (pSets);
		GUIObjects.addEventSystem ();

		startUpScreen ();
	}

	// Update is called once per frame
	void Update () 
	{
	}

	// If the user exits the program, terminate all threads in the thread pool
	void OnApplicationQuit()
	{
		threads.killThreadPool();
		Debug.Log ("Goodbye");
		Thread.Sleep (1000);
		Debug.Log (threads.getPoolCount ()); // Make sure all threads are sucessfully terminated
	}

	#endregion

	#region Menu Functions

	// Create the main menu
	void startUpScreen()
	{
		GameObject canvas = CommonGameObjects.getCanvas();

		// Add the background image
		GUIObjects.addBackgroundImage (canvas, "Main Menu Background", Vector2.zero, new Vector2 (1440, 900), 0);

		// Create the menu buttons
		Button buttonP = GUIObjects.addButton (canvas, "Play Button", new Vector2 ( 0, 120), new Vector2 (40, 15), 0);
		Button buttonN = GUIObjects.addButton (canvas, "New Map", new Vector2 ( 0, 00), new Vector2 (40, 15), 1);
		Button buttonQ = GUIObjects.addButton (canvas, "Exit", new Vector2 ( 0, -120), new Vector2 (40, 15), 2);
		Button buttonS = GUIObjects.addButton (canvas, "Save Map", new Vector2 ( -180, 0), new Vector2 (12, 9), 3);
		Button buttonL = GUIObjects.addButton (canvas, "Load Map", new Vector2 ( 180, 0), new Vector2 (12, 9), 4);

		// If all the maps are set, enable the play button to start the game when clicked
		// Otherwise, output instructions to create maps first when clicked
		if (checkAllMapsSet()) 
		{
			buttonP.onClick.AddListener (() => {
				Debug.Log ("Play Game Button Clicked!");
				loadingBarScreen ();
				StartCoroutine (initializeGameState ());
			});
		} 
		else 
		{
			buttonP.onClick.AddListener (() => {
				Debug.Log ("Maps Need to be Created First!");
			});
		}

		// Enable the new map button to create maps when clicked
		buttonN.onClick.AddListener (() => {
			Debug.Log("New Map Button Clicked!");
			loadingBarScreen();
			StartCoroutine(createAllMaps());
		});

		// Enable the exit button to quit the application
		buttonQ.onClick.AddListener (() => {
			Debug.Log("Exit Button Clicked!");
			//Application.Quit();
			UnityEditor.EditorApplication.isPlaying = false;
		});

		// If all the maps are set and not already saved, enable the save maps button
		// Otherwise, output warning that maps are already saved or instructions to create maps first
		if (!tSets.isSaved) // NOT LOCKED, UNSAFE???
		{
			if (checkAllMapsSet()) 
			{
				buttonS.onClick.AddListener (() => {
					Debug.Log ("Save all Maps Button Clicked!");
					loadingBarScreen ();
					StartCoroutine (saveMaps ());
				});
			} 
			else 
			{
				buttonS.onClick.AddListener (() => {
					Debug.Log ("Maps Need to be Created First!");
				});
			}
		} 
		else 
		{
			buttonS.onClick.AddListener (() => {
				Debug.Log ("All Maps Already Saved!");
			});
		}

		// Enable the load map button to load maps when clicked
		buttonL.onClick.AddListener (() => {
			Debug.Log("Load all Maps Button Clicked!");
			loadingBarScreen();
			StartCoroutine(loadMaps());
		});
	}

	// Make sure the game maps are set correctly
	bool checkAllMapsSet()
	{
		lock(tSets)
			return tSets.isHeightSet && tSets.isZoneSet && tSets.isAlphaSet && tSets.isVeggieSet && tSets.isWaterSet;
		
	}

	// Save the game maps to binary files
	IEnumerator saveMaps()
	{
		float sT = Time.realtimeSinceStartup;
		resetLoadingBar (4, 0);

		float[][] a1;
		int[][] a2;
		float[][][] a3;
		//float[][] a4;

		lock (tSets)
		{
			a1 = (float[][])tSets.globalHeightMap.Clone();
			a2 = (int[][])tSets.globalZoneMap.Clone();
			a3 = (float[][][])tSets.globalAlphaMap.Clone();
			//a4 = (VeggieObject[][])tSets.globalVeggieMap.Clone();
		}
		yield return new WaitForSeconds (0.5f); // Give time for loading screen to update

		InputsOutputs.write2DArraytoBinaryFile ("\\Resources\\Terrain\\MapData\\Heightmap_Binary.dat", a1);
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}
		yield return new WaitForSeconds (0.01f);

		InputsOutputs.write2DArraytoBinaryFile ("\\Resources\\Terrain\\MapData\\Zonemap_Binary.dat", a2);
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}
		yield return new WaitForSeconds (0.01f);

		InputsOutputs.write3DArraytoBinaryFile ("\\Resources\\Terrain\\MapData\\Alphamap_Binary.dat", a3);
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}

		// Need to write a way to convert veggie objects to binary data
		/*InputsOutputs.write2DArraytoBinaryFile ("\\Resources\\Terrain\\MapData\\Veggiemap_Binary.dat", a4);
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}*/
		yield return new WaitForSeconds (0.01f);

		// WATER MAP GOES HERE

		closeLoadingBar ();

		lock (tSets) 
		{
			tSets.isSaved = true;
		}

		yield return new WaitForSeconds (0.01f);

		float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
		Debug.Log("Maps Finished Saving after " + eT + " seconds!");

		startUpScreen ();
	}

	// Load the game maps from binary files in the MapData folder
	IEnumerator loadMaps()
	{
		float sT = Time.realtimeSinceStartup;
		resetLoadingBar (4, 0);

		yield return new WaitForSeconds (0.5f); // Give time for loading screen to update

		float[][] a1 = InputsOutputs.read2DFloatArrayfromBinaryFile ("\\Resources\\Terrain\\MapData\\Heightmap_Binary.dat");
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}
		yield return new WaitForSeconds (0.01f);

		int[][] a2 = InputsOutputs.read2DIntArrayfromBinaryFile ("\\Resources\\Terrain\\MapData\\Zonemap_Binary.dat");
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}
		yield return new WaitForSeconds (0.01f);

		float[][][] a3 = InputsOutputs.read3DFloatArrayfromBinaryFile ("\\Resources\\Terrain\\MapData\\Alphamap_Binary.dat");
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}

		// Need to write a way to convert veggie objects to binary data
		//float[][] a4 = InputsOutputs.read2DFloatArrayfromBinaryFile ("\\Resources\\Terrain\\MapData\\Veggiemap_Binary.dat");
		lock (lSets) 
		{
			lSets.totalFilled += 1;
		}
		yield return new WaitForSeconds (0.01f);

		// WATER MAP GOES HERE

		lock (tSets) 
		{
			tSets.globalHeightMap = (float[][])a1.Clone();
			tSets.globalZoneMap = (int[][])a2.Clone();
			tSets.globalAlphaMap = (float[][][])a3.Clone();
			//tSets.globalVeggieMap = (VeggieObject[][])a4.Clone();
			tSets.isHeightSet = true;
			tSets.isAlphaSet = true;
			tSets.isZoneSet = true;
			tSets.isVeggieSet = true;
			tSets.isSaved = true;
		}

		closeLoadingBar ();

		float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
		Debug.Log("Maps Finished Loading after " + eT + " seconds!");

		startUpScreen ();

	}

	// Schedules the creation of all game maps
	IEnumerator createAllMaps()
	{
		threads.resetTotalAddedToQueue ();
		threads.setToCountQueueAdding (true);

		lock (tSets) 
		{
			tSets.isSaved = false;
		}

		// Maps will be created in two chunks: 
		//		- First chunk will be maps that do not depend on any other map for creation (height, zones, water)
		//		- Second chunk will be textures and vegetation
		// After a map has been started, the completion flags will be checked periodically to confirm that the maps are done

		//Start first chunk
		StartCoroutine(ProceduralGeneration.createHeightMap2());
		StartCoroutine(ProceduralGeneration.createZoneMap2());
		StartCoroutine (ProceduralGeneration.createWaterMap ());

		yield return new WaitForSeconds (0.5f); // Give time for loading screen to update

		bool moveNext = false;
		int timeLimitSec = 60;
		float sT = Time.realtimeSinceStartup;

		while(!moveNext)
		{
			yield return new WaitForSeconds (0.2f);
			lock (tSets) 
			{
				// Check first chunk completion flags
				if (tSets.isHeightSet && tSets.isZoneSet && tSets.isWaterSet)
					moveNext = true;
			}

			// Check time limit
			float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
			if (eT > timeLimitSec)
				break;
		}

		// Reset for second chunk
		resetLoadingBar (0, 0);
		sT = Time.realtimeSinceStartup;

		// Make sure that first chunk fully loaded, otherwise output timeout error
		if (moveNext)
		{
			moveNext = false;
			// Start second chunk
			StartCoroutine(ProceduralGeneration.createAlphamap());
			StartCoroutine (ProceduralGeneration.createVeggieMap2 ());

			while (!moveNext) 
			{
				yield return new WaitForSeconds (0.2f);
				lock (tSets) 
				{
					// Check second chunk competion flags
					if (tSets.isAlphaSet && tSets.isVeggieSet)
						moveNext = true;
				}

				// Check time limit
				float eT = Mathf.Round (100f * (Time.realtimeSinceStartup - sT)) / 100f;
				if (eT > timeLimitSec)
					break;
			}

			if (!moveNext) // output timeout error if maps are not complete
			{
				Debug.Log ("Alphamap and Veggiemap creation timeout (>" + timeLimitSec + "s)");
				yield return new WaitForSeconds (1f);
				startUpScreen ();
			}
				
			closeLoadingBar();

			threads.setToCountQueueAdding (false);
			threads.resetTotalAddedToQueue ();
			yield return null;
		}
		else 
		{
			Debug.Log ("Zonemap, Heightmap, and Watermap creation timeout (>" + timeLimitSec + "s)");
			yield return new WaitForSeconds (1f);
			startUpScreen ();
		}
			
		startUpScreen ();
	}

	// Transitions into the game state
	IEnumerator initializeGameState()
	{		
		if (checkAllMapsSet()) 
		{
			menuState = false;
			gameState = true;

			int xTiles;
			int zTiles;
			lock (tSets)
			{
				xTiles = tSets.tileMap.GetLength (0);
				zTiles = tSets.tileMap.GetLength (1);
			}

			// Temporarily hard-coded until final revisions of the vegetation algorithm
			int numVeggieTypes = 15;
			int numVeggies = 200;

			resetLoadingBar (xTiles * zTiles + numVeggies * numVeggieTypes, 0);

			yield return new WaitForSeconds (0.5f); // Give time for loading screen to update

			// Create all the terrain tiles for the pool
			for (int i = 0; i < xTiles; i++)
			{
				for (int j = 0; j < zTiles; j++)
				{
					lock (tSets) 
					{
						int tileID = i * xTiles + j;
						TerrainData tDat = CommonGameObjects.createTerrainData ("Tile ID -" + tileID, tSets.heightMapResolution, new Vector3 (tSets.tileSizeX, tSets.tileSizeY, tSets.tileSizeZ), tSets.alphaMapResolution, tSets.detailMapResolution);
						GameObject thisTile = CommonGameObjects.createTerrain ("Tile ID -" + tileID +": Empty Terrain", tSets.outOfBoundsPos, tDat,tSets.tileParent.transform).gameObject;
						tSets.tileMap [i, j] = CommonGameObjects.createTerrainTile(thisTile, tileID);
					}
				}

				incrementLoadingBar(zTiles);
				yield return new WaitForSeconds (0.01f);
			}

			Debug.Log ("Tiles initialized!");

			// Create all the vegetation objects for the pool
			// Current implementation load all types of veggie...needs to be rewritten to load/pre-load types only in nearby zones
			for (int i = 0; i < numVeggieTypes; i++)
			{
				for (int k = 0; k < numVeggies; k++)
				{
					lock (tSets) 
					{
						VeggieObject tr = new VeggieObject();
						tr.plantID = i;
						tr.vr = CommonGameObjects.createVeggie ("Empty Veggie", tSets.outOfBoundsPos, Quaternion.identity, i,tSets.veggieParent.transform);
						tr.used = false;
						tr.vr.SetActive (false);
						tr.tileNum = -1;

						tSets.veggiePool.Add (tr);
					}
				}

				incrementLoadingBar (numVeggies);
				yield return new WaitForSeconds (0.01f);
			}
			Debug.Log ("Veggies initialized!");

			// Update the world models, using objects from the pools
			// Wait until the objects are done loading to continue
			// Please note, this update occurs before the player is placed to avoid having the update visible to the player camera
			StartCoroutine (checkIfNeedsUpdate ());
			bool keepWaiting = true;
			while (keepWaiting) 
			{
				yield return new WaitForSeconds (0.5f);
				lock (tSets) 
				{
					keepWaiting = tSets.currentlyUpdating;
				}
			}

			closeLoadingBar ();

			// Remove the menu items and create a player/camera/light for the game
			CommonGameObjects.removeAllTaggedObjects (new string[]{ "MainCamera", "Camera", "Canvas", "Button", "Image", "Light" });
			lock (pSets) 
			{
				pSets.mainPlayer = CommonGameObjects.createPlayer (pSets.name, pSets.startPosition, pSets.rotation, 0,0);
				pSets.isPlayerSet = true;
				pSets.theSun = CommonGameObjects.createLight ("THE SUN", pSets.sunStartPosition, pSets.sunRotation, pSets.sunType, pSets.sunShadows, pSets.sunColor);
			}

			// Initialize the player/light/camera settings
			updatePlayerVisuals ();
		} 
		else
		{
			Debug.Log ("Maps need to be set in order to play!");
			startUpScreen ();
		}
		yield return new WaitForSeconds (0.01f);
	}
		
	#endregion

	#region Loading Bar

	// Transition to the loading bar screen and initializes the loading bar
	// Current implementation will remove the player camera if called during gamestate, needs to be revised
	void loadingBarScreen()
	{
		GameObject canvas = CommonGameObjects.getCanvas ();
		GUIObjects.addBackgroundImage(canvas, "Loading Bar Background", Vector2.zero, new Vector2 (1440, 900), 1);
		lock (lSets) 
		{
			lSets.image = GUIObjects.addImage (canvas, "Loader Border", Vector2.zero, new Vector2 (77, 12), 0);
			lSets.image = GUIObjects.addLoadingBar (canvas, "Menu to Game Loader", Vector2.zero, new Vector2 (80, 30), 0);
			lSets.isSet = true;
			lSets.isActive = true;
		}
		StartCoroutine (updateLoadingBar ());
	}

	// Coroutine loop that checks the fill amounts every 0.2 seconds and updates the loading image
	IEnumerator updateLoadingBar()
	{
		Debug.Log ("Loading Bar Active");
		while (lSets.isActive) // NOT LOCKED, UNSAFE???
		{
			yield return new WaitForSeconds (0.2f);
			lock (lSets)
			{
				if (lSets.isActive && lSets.isSet && lSets.totalToFill > 0)
					lSets.image.fillAmount = ((float)lSets.totalFilled) / ((float)lSets.totalToFill);
			}
		}
		Debug.Log ("Loading bar inactive");
	}

	// Resets the values of the fill amounts and disables the loading bar and loading state
	void closeLoadingBar()
	{
		lock (lSets)
		{
			if (lSets.isActive) 
			{
				lSets.totalToFill = 0;
				lSets.totalFilled = 0;
				lSets.isActive = false;
			}
		}
	}

	// Just resets the values of the fill amounts
	void resetLoadingBar(int total, int current)
	{
		lock (lSets)
		{
			if (lSets.isActive) 
			{
				lSets.totalToFill = total;
				lSets.totalFilled = current;
			}
		}
	}

	// Adds the input to the current level of fill for the loading bar
	void incrementLoadingBar(int inc)
	{
		lock (lSets)
		{
			if (lSets.isActive) 
			{
				lSets.totalFilled += inc;
			}
		}
	}

	#endregion

	#region Updates

	// Coroutine that runs continuously during gamestate, checking to see if the tiles or objects need updates
	// Checks player location every loop, and if player has moved significantly it cycles through the tiles to find ones that need updates
	// Applies updates to the identified tiles, then modifies available gameobjects to be placed on the tile
	IEnumerator checkIfNeedsUpdate()
	{
		// Stores the last known position of the player as the Tile ID number they were on
		Vector3 lastTile;

		lock (tSets) 
		{
			lastTile = tSets.outOfBoundsTileNum;
			tSets.currentlyUpdating = false;
		}

		threads.resetTotalAddedToQueue ();
		threads.setToCountQueueAdding (true);

		// Loop continuously during gamestate
		while (gameState) 
		{
			// Try to store the player's current position by getting the current Tile ID number they are on
			// Try/catch was added because the first update (from initializeGameState function) triggers before the player has been created
			Vector3 currentTile;
			lock (pSets) 
			{
				try
				{
					pSets.currentPosition = pSets.mainPlayer.transform.position;
				}
				catch 
				{
					pSets.currentPosition = pSets.startPosition;
				}
				currentTile = worldToGamemap(pSets.currentPosition,0);
			}

			yield return new WaitForSeconds (0.01f);

			// Check to see if the player is on a new tile and there is no ongoing update
			if (lastTile != currentTile && !tSets.currentlyUpdating) // NO LOCK, UNSAFE???
			{
				// Create a flag to check when the tile positions are all fully updated
				bool waitForPos = true;
				int xTiles,zTiles;

				// Set the flag to mark the ongoing update
				lock (tSets) 
				{
					tSets.currentlyUpdating = true;
					xTiles = tSets.tileMap.GetLength (0);
					zTiles = tSets.tileMap.GetLength (1);
				}

				// Calculate the new tile positions
				// Perhaps needs to be revised for some loading bar functionality, however seems to currently finish quickly
				CustomThreadPool.Instance.QueueUserTask (() => {updateTilePositions ();},(ts) => {
					Debug.Log("Tile Positions Recalculated");
					waitForPos=false;
				});

				// Wait until the tile positions have been calculated
				while (waitForPos)
					yield return new WaitForSeconds (0.1f);

				// If the update is beign performed with a loading bar
				resetLoadingBar (2 * xTiles * zTiles, 0);

				// Create counters to check when side threads are completed
				int updateCount = 0;
				int heightCount = 0;
				int alphaCount = 0;
				int detailCount = 0;

				// Check each tile to see if needs to be updated, then update the position/heights/textures/objects of each of those tiles
				for(int i = 0; i < xTiles; i++)
				{
					for(int j = 0; j < zTiles; j++)
					{
						// Check this tile to see if it needs updating
						bool updated;
						lock (tSets) 
						{
							updated = tSets.tileMap [i, j].needsUpdate;
						}

						if (updated)
						{
							//Store the tile
							TerrainTile tTile;
							lock (tSets)
							{
								tTile = tSets.tileMap [i, j];
							}
							GameObject t = tTile.tileObject;
							Terrain tTer = t.GetComponent<Terrain> ();

							// Move the tile to the correct position
							t.transform.position = tTile.tempPosition;

							// Rename the tile
							int tileID = tTile.idNum;
							t.name = "Tile ID -" + tileID + ": " + t.transform.position.x + "(" + i + "), " + t.transform.position.z + "(" + j + ")";

							// Show/hide the tile
							tTer.enabled = tTile.showTile;

							// If the tile is visible update the height/texture/detail
							if (tTile.showTile)
							{
								// Main counter collects the total number of tiles that need updates
								updateCount++;

								// Heightmap must be set here (not with ther maps) due to issues with the player falling through the map??????
								// Must calculate new heightmaps in main thread, in order to apply heightmap data here
								// Needs to be revised to allow side thread to compute new heightmap (see comments below)
								updateTileHeightMap (tTile);
								heightCount++;
								tTer.terrainData.SetHeights (0, 0, tTile.tempHeightmap);

								// Find the new heightmap for the tile and store it in the temp array, then increments the height counter on completion
								/*CustomThreadPool.Instance.QueueUserTask (() => {updateTileHeightMap (tTile);},(ts) => {
									heightCount++;
								});*/

								// Find the new alphamap for the tile and store it in the temp array, then increments the alpha counter on completion
								CustomThreadPool.Instance.QueueUserTask (() => {updateTileAlphaMap (tTile);},(ts) => {
									alphaCount++;
								});

								// Find the new detailmap for the tile and store it in the temp array, then increments the detail counter on completion
								CustomThreadPool.Instance.QueueUserTask (() => {updateTileDetailMap (tTile);},(ts) => {
									detailCount++;
								});
							}

							yield return new WaitForSeconds (0.01f);
						}
					}
					incrementLoadingBar (zTiles);

				}

				// Wait until all the side threads are done by checking the height/alpha/detail counters against the main counter
				bool moveNext = false;
				while (!moveNext) 
				{
					if (updateCount == heightCount && updateCount == alphaCount && updateCount == detailCount)
						break;
					yield return new WaitForSeconds (0.1f);
				}

				// Check each tile to see if needs to be updated, then apply the temp height/texture/detail arrays
				for (int i = 0; i < xTiles; i++)
				{
					for (int j = 0; j < zTiles; j++) 
					{
						// Check this tile to see if it needs updating
						bool updated;
						lock (tSets) 
						{
							updated = tSets.tileMap [i, j].needsUpdate;
						}

						if (updated)
						{
							// Store this terrain tile
							TerrainTile tTile;
							lock (tSets)
							{
								tTile = tSets.tileMap [i, j];
							}

							// Get the terrain object
							Terrain tTer = tTile.tileObject.GetComponent<Terrain> ();

							// Apply the heightmap
							/*tTer.terrainData.SetHeights (0, 0, tTile.tempHeightmap); Executing this command here causes the player to fall through the map???????????
							yield return new WaitForSeconds (0.01f);
							*/

							// Apply the alhpamap 
							tTer.terrainData.SetAlphamaps (0, 0, tTile.tempAlphamap);
							yield return new WaitForSeconds (0.01f);

							// Apply the detailmap
							for (int m = 0; m < tTile.detailmapLayers; m++) 
							{
								tTer.terrainData.SetDetailLayer (0, 0, m, tTile.tempDetailmap [m]);
								yield return new WaitForSeconds (0.01f);
							}
								
							// Place trees on the new tile, currently requires the heightmap to be fully applied before calling
							// Needs to be revised to place objects based upon the global heightmap and not the terrain data height
							updateVeggies (tTile);

							// Mark the tile as fully up to date
							tTile.needsUpdate = false;
							yield return new WaitForSeconds (0.01f);
						}
					}

					// If the update is beign performed with load bar, increase the fill accordingly
					incrementLoadingBar (zTiles);
				}
					
				// Remove seams between adjacent tiles
				setTerrainNeighbors ();
				setTerrainFlush ();

				// Change the last know position to the current position and complete the update
				lastTile = currentTile;
				lock(tSets)
				{
					tSets.currentlyUpdating = false;
				}
			}
		}

		threads.resetTotalAddedToQueue ();
		threads.setToCountQueueAdding (false);

		menuState = true;
		startUpScreen ();
		yield return new WaitForSeconds (0.5f);
	}

	// Recalculate tile positions so that the tile under the player becomes the center tile 
	// However, the actual terrain tile is not moved yet because this function will be used in a side thread and Unity classes cannot be accessed
	// Also, rearrange the tiles in the array so that they match their layout in physical space
	void updateTilePositions()
	{
		// Get the tile number that the player is currently standing on
		Vector3 tileNum;
		lock (pSets) 
		{
			tileNum = worldToGamemap(pSets.currentPosition, 0); 
		}

		// Get some of the settings from the terrain settings
		int zTiles, xTiles, halfLoadedTilesX, halfLoadedTilesZ;
		lock (tSets) 
		{
			zTiles = tSets.tileMap.GetLength (1);
			xTiles = tSets.tileMap.GetLength (0);
			halfLoadedTilesX = tSets.halfLoadedTilesX;
			halfLoadedTilesZ = tSets.halfLoadedTilesZ;
		}

		// Initialize a temporary game object array to store the rearranged tile objects
		TerrainTile[,] tempMap = new TerrainTile[xTiles, zTiles];

		// Initialize an array that marks locations where a new tile is needed
		// This array references positions in the new tile array
		bool[,] needTile = new bool[xTiles, zTiles];

		// Initialize the marker arrays default as true
		for (int i = 0; i < xTiles; i++) 
		{
			for (int j = 0; j < zTiles; j++)
			{
				needTile [i, j] = true;
				tSets.tileMap [i, j].availableForUpdate = true;
			}
		}

		// Iterate throught each position in the new tile array, checking to see if any old tiles can be carried over
		for(int i = 0; i < xTiles; i++)
		{
			for (int j = 0; j < zTiles; j++)
			{
				// Find the tile number for this position in the new array and convert it to world coordinates
				Vector3 currentTile = new Vector3(tileNum.x + i - halfLoadedTilesX, 0, tileNum.z + j - halfLoadedTilesZ);
				Vector3 currentPos = gamemapToWorld	(currentTile, 0);

				// Check all the old tiles to see if a tile already exists in this position
				for (int x = 0; x < xTiles; x++)
				{
					for (int z = 0; z < zTiles; z++)
					{
						// Check to make sure a tile is still needed for this position in the new array
						if (needTile[i,j]) 
						{
							lock (tSets) 
							{								
								// Check this position in the old array for a match
								if (tSets.tileMap[x, z].tempPosition == currentPos) 
								{
									// Get the old tile object and move it to its new position in the temp array
									tempMap[i,j] = tSets.tileMap [x, z];

									// Tell the tile to keep the same position
									tempMap [i, j].tempPosition = currentPos;

									// Set the flag so that the tile does not receive any updates
									tempMap [i, j].needsUpdate = false;

									// Mark the position in the new array as filled
									needTile [i, j] = false;

									// Mark the position in the old array as used
									tSets.tileMap [x, z].availableForUpdate = false;
								}
							}
						}
					}
				}
			}
		}


		// Iterate thorugh each position in the new array, checking to see if there are gaps where none of the old tiles could fill
		for (int i = 0; i < xTiles; i++)
		{
			for (int j = 0; j < zTiles; j++)
			{
				// Skip this position unless a new tile is still needed for this spot
				if (needTile [i, j]) 
				{
					// Find the tile number for this position in the new array and convert it to world coordinates
					Vector3 currentTile = new Vector3 (tileNum.x + i - halfLoadedTilesX, 0, tileNum.z + j - halfLoadedTilesZ);
					Vector3 currentPos = gamemapToWorld (currentTile, 0);

					// Check all the old tiles to see if a tile is available for updating into this new slot
					for (int x = 0; x < xTiles; x++) 
					{
						for (int z = 0; z < zTiles; z++) 
						{
							
							// Make sure a new tile is still needed in the position in the new array
							// Also, make sure the old tile is available for updating in the position of the old array
							if (needTile[i, j] && tSets.tileMap[x, z].availableForUpdate) // NO LOCK, UNSAFE???
							{
								lock (tSets) 
								{
									// Get the old tile object and move it to its new position in the temp array
									tempMap [i, j] = tSets.tileMap [x, z];

									// Tell the old tile that it should move to this position during update
									tempMap [i, j].tempPosition = currentPos;

									// Set the flag so that the tile will receive updates
									tempMap [i, j].needsUpdate = true;

									// Check to see if the new position for this tile is outside the game map
									// If it is out of bounds, set the flag to make it invisible
									// Otherwise, make sure it is visible
									if (currentTile.x < 0 || currentTile.x >= tSets.halfGlobalTilesX * 2 || currentTile.z < 0 || currentTile.z >= tSets.halfGlobalTilesZ * 2)
									{
										tempMap [i, j].showTile = false;
									}
									else
									{
										tempMap [i, j].showTile = true;
									}
								}

								// Mark the position in the new array as filled
								needTile [i, j] = false;

								// Mark the tile in the old array as used
								tSets.tileMap [x ,z].availableForUpdate = false;
							}
						}
					}
				}
			}
		}

		// Update the terrain settings with the new tile map/positions/visibility
		lock (tSets) 
		{
			tSets.tileMap = tempMap;
		}
	}

	// Updates the height map for a given tile
	void updateTileHeightMap(TerrainTile tile)
	{
		// Get the resolution of height data for the tile
		int hmr = tile.heightmapResolution;

		// Get the global height map index that corresponds to the origin of the tile
		Vector3 tileNum = worldToGamemap (tile.tempPosition, 0);
		Vector3 globalHeightMapOrigin = new Vector3 (tileNum.x * (hmr - 1), 0, tileNum.z * (hmr - 1));

		// Iterate through the resolution of the tile and update the temp array with the global heightmap data
		for (int x = 0; x < hmr; x++)
		{
			for (int z = 0; z < hmr; z++) 
			{
				Vector3 globalHeightPos = new Vector3 (globalHeightMapOrigin.x + x, 0, globalHeightMapOrigin.z + z);
				lock(tSets)
				{
					tile.tempHeightmap [z, x] = tSets.globalHeightMap [(int)globalHeightPos.x] [(int)globalHeightPos.z]; // Switched x and z axis??????
				}
			}
		}
	}

	// Updates the alpha map for a given tile
	void updateTileAlphaMap(TerrainTile tile)
	{
		// Get the resolution of texture data for the tile
		int amr = tile.alphamapResolution;
		int amt = tile.alphamapLayers;

		// Get the global alpha map index that corresponds to the origin of the tile
		Vector3 tileNum = worldToGamemap(tile.tempPosition, 0);
		Vector3 globalAlphaMapOrigin = new Vector3 (tileNum.x * (amr), 0, tileNum.z * (amr));

		// Iterate through the resolution of the tile and update the temp array with the global alphamap data
		for (int x = 0; x < amr; x++)
		{
			for (int z = 0; z < amr; z++) 
			{
				Vector3 globalAlphaPos = new Vector3 (globalAlphaMapOrigin.x + x, 0, globalAlphaMapOrigin.z + z);

				for (int y = 0; y < amt; y++)
				{
					lock(tSets)
					{
						tile.tempAlphamap [z, x, y] = tSets.globalAlphaMap [(int)globalAlphaPos.x] [(int)globalAlphaPos.z] [y]; // No Switched axis??????;
					}
				}
			}
		}
	}

	// Updates the detail map for a given tile
	void updateTileDetailMap(TerrainTile tile)
	{
		// Get the resolution of detail data for the tile
		int dmr = tile.detailmapResolution;
		int dmt = tile.detailmapLayers;

		// Get the global detail map index that corresponds to the origin of the tile
		Vector3 tileNum = worldToGamemap (tile.tempPosition, 0);
		Vector3 globalDetailMapOrigin = new Vector3 (tileNum.x * (dmr), 0, tileNum.z * (dmr));

		// Iterate through the resolution of the tile and update the temp array with the detail data
		for (int x = 0; x < dmr; x++)
		{
			for (int z = 0; z < dmr; z++) 
			{
				Vector3 globalDetailPos = new Vector3 (globalDetailMapOrigin.x + x, 0, globalDetailMapOrigin.z + z);

				// Global detail map has not yet been implemented, so use the water content and zone to decide if/which detaile exists here

				// Get water content and zone number
				Vector3 waterPos = gamemapToGamemap(globalDetailPos, 4, 5);
				Vector3 zonePos = gamemapToGamemap(globalDetailPos, 4, 2);
				float waterVal;
				int zoneNum;
				lock (tSets)
				{
					waterVal = tSets.globalWaterMap [(int)waterPos.x] [(int)waterPos.z];
					zoneNum = tSets.globalZoneMap [(int)zonePos.x] [(int)zonePos.z];
				}

				// If water is abundand (>0.7) then a detail should be placed here
				int detail = 0.7f < waterVal ? 1 : 0;

				/* Possibility to change the distribution of detail placement in future revisions
				float thisGrassChance = Interpolations.cubicBezierEaseCurve(0f, 0f, 1f,1f,waterVal);
				int detail = 0.9f < thisGrassChance ? 1 : 0;
				*/

				// Iterate through the detail layers to place the detail corresponding to the current zone
				// CURRENT IMPLEMENTATION REQUIRES THE NUMBER OF DETAILS TO BE EQUAL TO THE NUMBER OF ZONES, NEEDS TO BE REVISED
				for(int i = 0; i < dmt; i++)
				{
					if(zoneNum == i) tile.tempDetailmap [i] [z, x] = detail;
					else tile.tempDetailmap [i] [z, x] = 0;
				}
			}
		}
	}

	// Updates the vegetation objects on a given tile
	void updateVeggies(TerrainTile tile)
	{
		// Get the tile object, position, id number, and vegetation resolution
		GameObject tO = tile.tileObject;
		Vector3 pos = tile.tempPosition;
		int tileID = tile.idNum;
		int vmr = tile.veggiemapResolution;

		// Get the global vegetation map index that corresponds to the origin of the tile
		Vector3 globalVeggieOrigin = worldToGamemap (pos, 6);

		// Populate a list with all of the vegetation objects on the tile from the previous update
		List<VeggieObject> oldVeggies;
		lock (tSets) 
		{
			oldVeggies = tSets.veggiePool.Where (x => x.tileNum == tileID).ToList ();
		}

		// Reset the values of the old vegetation objects, making them available in the pool for reuse
		foreach(VeggieObject vrO in oldVeggies)
		{
			vrO.tileNum = -1;
			vrO.used = false;
			vrO.vr.SetActive (false);
			vrO.vr.name = "Empty Veggie";
			vrO.bioMass = 0;
			updatePlantInfo (vrO);
		}

		// If the tile is going to be visible, find new vegetation objects to populate the surface
		bool showTile = tile.showTile;
		if (showTile) 
		{
			// Iterate through the resolution, looking at the vegetation objects in the global map for non-null values
			for (int i = 0; i < vmr; i++) 
			{
				for (int j = 0; j < vmr; j++) 
				{
					// Get the vegetaion object at this position
					VeggieObject thisVeggie = tSets.globalVeggieMap [(int)globalVeggieOrigin.x + i] [(int)globalVeggieOrigin.z + j];

					// If the object is not null, get a vegetation object (old or from the pool) and update it to the new position
					if (thisVeggie != null) 
					{
						// Get the plant type at the new location
						int idNum = thisVeggie.plantID;

						// Look for a match in available vegetation objects in the pool
						VeggieObject temp = tSets.veggiePool.FirstOrDefault (x => x.used == false && x.plantID == idNum);

						// If a match is found update the vegetation object information, otherwise the pool is too small
						if (temp != null)
						{
							// Get the height of the terrain and combine it with the x,z location to create the origin for the vegetation object position
							Vector3 vegPos = new Vector3 (pos.x + i, tO.GetComponent<Terrain> ().SampleHeight (new Vector3 (pos.x + i, 0, pos.z + j)), pos.z + j);
							temp.vr.transform.position = new Vector3 (vegPos.x, vegPos.y, vegPos.z);

							// Rename the vegetaion object and save the tile ID number
							temp.vr.name = "Veggie on Tile " + tileID;
							temp.tileNum = tileID;

							// Create a volumetric conversion constant using the base height/width of the model for converting the biomass value into actual height/width
							float convConst = 3 / Mathf.PI / ((1/CommonGameObjects.veggieProperties [temp.plantID, 2]) / (1/CommonGameObjects.veggieProperties [temp.plantID, 0]));
							float mult = 1000f;

							// Use the conversion constant to create a scale factor, then apply the new scale to the model
							Vector3 newScale = Vector3.one * Mathf.Pow( thisVeggie.bioMass * convConst*mult, 1f/3f)/Mathf.Pow(mult,1f/3f);
							temp.vr.transform.localScale = newScale * CommonGameObjects.veggieProperties [temp.plantID, 0];

							// Rotate the model according to the global veggie map
							temp.vr.transform.rotation = thisVeggie.rot;

							// Update the properties of the revised vegetation object
							temp.bioMass = thisVeggie.bioMass;
							temp.height = thisVeggie.height;
							temp.width = thisVeggie.width;
							temp.used = true;
							temp.vr.SetActive (true);

							// Send this information to a script on the vegetation object that will display these properties in editor window
							updatePlantInfo (temp);
						} 
						else
							Debug.Log ("POOL TOO SMALL");
					}
				}
			}
		}
	}

	// Send the properties of a vegetation object to a script for display in the editor window (debugging tool)
	void updatePlantInfo(VeggieObject thisVeggie)
	{
		thisVeggie.vr.GetComponent<SeePlantProperties>().bioMass = thisVeggie.bioMass;
		thisVeggie.vr.GetComponent<SeePlantProperties> ().idNum = thisVeggie.plantID;
		thisVeggie.vr.GetComponent<SeePlantProperties> ().updateYSize ();
		thisVeggie.vr.GetComponent<SeePlantProperties> ().ySizeCalc = thisVeggie.height;
		thisVeggie.vr.GetComponent<SeePlantProperties> ().updateXSize ();
		thisVeggie.vr.GetComponent<SeePlantProperties> ().xSizeCalc = thisVeggie.width;
	}

	// Iterate through all the tile objects and assign neighbors (left right up down) to each tile
	// This must be done in order to set these tiles "flush" and remove visible seams in the terrain
	void setTerrainNeighbors()
	{
		lock (tSets) 
		{
			int xTiles = tSets.tileMap.GetLength (0);
			int zTiles = tSets.tileMap.GetLength (1);

			// Iterate through all tiles
			for (int x = 0; x < xTiles; x++)
			{
				for (int z = 0; z < zTiles; z++)
				{
					// Initialize the four neighbors and the central terrain
					Terrain tLeft = null, tRight = null, tTop = null, tBottom = null;
					Terrain current = tSets.tileMap [x, z].tileObject.GetComponent<Terrain> ();

					// Assign the neighbor tiles while simultaneously checking for edge tiles
					// If the central terrain is an edge/corner tile do not assign neighbors beyond the edges
					tLeft = x == 0 ? null : tSets.tileMap [x - 1, z].tileObject.GetComponent<Terrain> ();
					tRight = x == xTiles - 1 ? null : tSets.tileMap [x + 1, z].tileObject.GetComponent<Terrain> ();
					tTop = z == zTiles - 1 ? null : tSets.tileMap [x, z + 1].tileObject.GetComponent<Terrain> ();
					tBottom = z == 0 ? null : tSets.tileMap [x, z - 1].tileObject.GetComponent<Terrain> ();

					//Set the terrain neighbors
					current.SetNeighbors (tLeft, tTop, tRight, tBottom);
				}
			}
		}
	}

	// Iterate through all the tile objects to set these tiles "flush" and remove visible seams in the terrain
	void setTerrainFlush()
	{
		lock (tSets) 
		{
			foreach (TerrainTile tile in tSets.tileMap)
				tile.tileObject.GetComponent<Terrain> ().Flush ();
		}
	}

	// Set up the player camera settings to add fog and change the sky color
	void updatePlayerVisuals()
	{
		lock (pSets)
		{
			GameObject p1 = pSets.mainPlayer;
			RenderSettings.fog = pSets.fogEnabled;
			RenderSettings.fogColor = pSets.fogColor;
			RenderSettings.fogDensity = pSets.fogDensity;

			p1.GetComponentInChildren<Camera>().clearFlags = pSets.clearFlags;
			p1.GetComponentInChildren<Camera>().backgroundColor = pSets.skyColor;
		}
	}

	#endregion

	#region Map Unit Conversions

	// Converts position in any of the the map arrays to world coordinates
	public static Vector3 gamemapToWorld(Vector3 pos, int mapNum)
	{
		// Get the number of rows and columns in the target map
		Vector3 mapSize = getMapsizes (mapNum);

		// Get the number of tiles in the entire worldmap and their size
		int halfTilesX, halfTilesZ, tileSizeX, tileSizeZ;
		lock (tSets)
		{
			halfTilesX = tSets.halfGlobalTilesX;
			halfTilesZ = tSets.halfGlobalTilesZ;
			tileSizeX = tSets.tileSizeX;
			tileSizeZ = tSets.tileSizeZ;				
		}

		// Create a conversion factor for moving between the map and world coordinates
		float mapToWorldX = ((float)(halfTilesX * 2 + 1) * tileSizeX) / mapSize.x;
		float mapToWorldZ = ((float)(halfTilesZ * 2 + 1) * tileSizeZ) / mapSize.z;

		// Multiply the gamemap position by the conversion factor and shift according to the origin of the global tiles
		float worldX = pos.x * mapToWorldX - halfTilesX * tileSizeX;
		float worldZ = pos.z * mapToWorldZ - halfTilesZ * tileSizeZ;

		return new Vector3 (worldX, 0, worldZ);
	}

	// Converts world coordinates to the corresponding position in any of the map arrays
	public static Vector3 worldToGamemap(Vector3 pos, int mapNum)
	{
		// Get the number of rows and columns in the target map
		Vector3 mapSize = getMapsizes (mapNum);

		// Get the number of tiles in the entire worldmap and their size
		int halfTilesX, halfTilesZ, tileSizeX, tileSizeZ;
		lock (tSets)
		{
			halfTilesX = tSets.halfGlobalTilesX;
			halfTilesZ = tSets.halfGlobalTilesZ;
			tileSizeX = tSets.tileSizeX;
			tileSizeZ = tSets.tileSizeZ;
		}

		// Create a conversion factor for moving between the world coordinates and the map
		float worldToMapX = mapSize.x / ((float)(halfTilesX * 2 + 1) * tileSizeX);
		float worldToMapZ = mapSize.z / ((float)(halfTilesZ * 2 + 1) * tileSizeZ);

		// Shift the world coordinates according to the origin of the global tiles and then multiply by the conversion factor
		int mapX = Mathf.FloorToInt (worldToMapX * (pos.x + halfTilesX * tileSizeX));
		int mapZ = Mathf.FloorToInt (worldToMapZ * (pos.z + halfTilesZ * tileSizeZ));

		return new Vector3 (mapX, 0, mapZ);
	}

	// Converts position in any of the the map arrays to position in another
	public static Vector3 gamemapToGamemap(Vector3 pos, int fromMapNum, int toMapNum)
	{
		// Get the number of rows and columns in the target maps
		Vector3 fromMapSize = getMapsizes (fromMapNum);
		Vector3 toMapSize = getMapsizes (toMapNum);

		// Create a conversion factor for moving between the maps
		float mapToMapX = toMapSize.x/fromMapSize.x;
		float mapToMapZ = toMapSize.z/fromMapSize.z;

		// Calculate the positions in the new map by multiplying by the conversion factor and rounding (down?)
		int toMapX = Mathf.FloorToInt(pos.x*mapToMapX);
		int toMapZ = Mathf.FloorToInt(pos.z*mapToMapZ);

		return new Vector3 (toMapX, 0, toMapZ);
	}

	// Switch statement used for obtaining lengths of different game maps
	public static Vector3 getMapsizes(int mapNum)
	{
		int mapLengthX, mapLengthZ;

		lock (tSets) 
		{
			switch (mapNum)
			{
			case 0:				
				mapLengthX = tSets.halfGlobalTilesX * 2 + 1;
				mapLengthZ = tSets.halfGlobalTilesZ * 2 + 1;
				break;
			case 1:				
				mapLengthX = tSets.globalHeightMap.Length;
				mapLengthZ = tSets.globalHeightMap [0].Length;
				break;
			case 2:				
				mapLengthX = tSets.globalZoneMap.Length;
				mapLengthZ = tSets.globalZoneMap [0].Length;
				break;
			case 3:				
				mapLengthX = tSets.globalAlphaMap.Length;
				mapLengthZ = tSets.globalAlphaMap [0].Length;
				break;
			case 4:				
				mapLengthX = tSets.globalDetailMap.Length;
				mapLengthZ = tSets.globalDetailMap [0].Length;
				break;
			case 5:				
				mapLengthX = tSets.globalWaterMap.Length;
				mapLengthZ = tSets.globalWaterMap [0].Length;
				break;
			case 6:
				mapLengthX = tSets.globalVeggieMap.Length;
				mapLengthZ = tSets.globalVeggieMap [0].Length;
				break;
			default:
				return Vector3.zero;
			}
		}

		return new Vector3 (mapLengthX, 0, mapLengthZ);
	}

	#endregion

	#region Initializing Data Classes

	void initializeLoadingBar(LoadingBarSettings l)
	{
		lock (l) 
		{
			l.isSet = false; //No Image yet
			l.isActive = false;
			l.name = "LoadingBar";
			l.position = Vector2.zero;
			l.size = new Vector2 (80, 30);
			l.image = null;
			l.totalFilled = 0f;
			l.totalToFill = 0f;
		}
	}

	void initializeTerrainSettings(MainTerrainSettings t)
	{
		lock (t) 
		{
			t.tileSizeX = 10;
			t.tileSizeZ = 10;
			t.tileSizeY = 5;
			t.currentlyUpdating = false;
			t.isDefaultSet = true; // No arrays yet
			t.isHeightSet = false;
			t.isAlphaSet = false;
			t.isZoneSet = false;
			t.isWaterSet = false;
			t.isVeggieSet = false;
			t.isSaved = false;
			t.halfGlobalTilesX = 5;
			t.halfGlobalTilesZ = 5;
			t.halfLoadedTilesX = 3;
			t.halfLoadedTilesZ = 3;
			t.numZones = CommonGameObjects.terrainTextureColor.GetLength (0);
			t.veggiePool = new List<VeggieObject> ();
			t.veggieParent = new GameObject ();
			t.veggieParent.tag = "Veggie";
			t.veggieParent.name = "Veggies";
			t.tileParent = new GameObject ();
			t.tileParent.tag = "Terrain";
			t.tileParent.name = "Terrain Tiles";
			t.alphaMapResolution = 128;
			t.detailMapResolution = 256;
			t.heightMapResolution = 129;
			t.veggieMapResolution = 10;

			t.tileMap = new TerrainTile[2 * t.halfLoadedTilesX + 1, 2 * t.halfLoadedTilesZ + 1];
			t.outOfBoundsTileNum = new Vector3(t.halfGlobalTilesX*2+2,0,t.halfGlobalTilesX*2+2);
			t.outOfBoundsPos = gamemapToWorld (t.outOfBoundsTileNum, 0);

			int hmLZ = (tSets.heightMapResolution - 1) * (tSets.halfGlobalTilesZ * 2)+1;
			int hmLX = (tSets.heightMapResolution - 1) * (tSets.halfGlobalTilesX * 2)+1;
			int amLX = tSets.alphaMapResolution * (tSets.halfGlobalTilesX * 2);
			int amLZ = tSets.alphaMapResolution * (tSets.halfGlobalTilesZ * 2);
			int dmLX = tSets.detailMapResolution * (tSets.halfGlobalTilesX * 2);
			int dmLZ = tSets.detailMapResolution * (tSets.halfGlobalTilesZ * 2);
			int vmLX = tSets.veggieMapResolution * (tSets.halfGlobalTilesX * 2);
			int vmLZ = tSets.veggieMapResolution * (tSets.halfGlobalTilesX * 2);

			t.globalHeightMap = new float[hmLX][];
			for (int i = 0; i < hmLX; i++) t.globalHeightMap [i] = new float[hmLZ];

			int numVarText = CommonGameObjects.terrainTextureColor.GetLength (1);
			t.globalAlphaMap = new float[amLX][][];
			for (int i = 0; i < amLX; i++) 
			{
				t.globalAlphaMap [i] = new float[amLZ][];
				for (int j = 0; j < t.numZones; j++) t.globalAlphaMap [i] [j] = new float[t.numZones*numVarText];
			}

			t.globalZoneMap = new int[hmLX][];
			for (int i = 0; i < hmLX; i++) t.globalZoneMap [i] = new int[hmLZ];

			t.globalVeggieMap = new VeggieObject[vmLX][];
			for (int i = 0; i < vmLX; i++) t.globalVeggieMap [i] = new VeggieObject[vmLZ];

			t.globalWaterMap = new float[vmLX][];
			for (int i = 0; i < vmLX; i++) t.globalWaterMap [i] = new float[vmLZ];

			t.globalDetailMap =  new int[dmLX][];
			for (int i = 0; i < dmLX; i++) t.globalDetailMap [i] = new int[dmLZ];
		}
	}

	void initializePlayer(MainPlayerSettings p)
	{
		p.name = "Main Player";
		p.clearFlags = CameraClearFlags.SolidColor;
		//Color sky = new Color (137/255f, 219/255f,219/255f,0/255f);
		Color sky = new Color (185/255f, 197/255f, 186/255f, 97/255f);
		p.skyColor = sky + new Color (0/255f, 0/255f,0/255f, 50/255f);
		p.fogEnabled = true;
		p.fogColor = sky;
		p.fogDensity = 0.07f;
		p.needsUpdate = true;
		p.isDefaultSet = true;
		p.isPlayerSet = false;
		p.mainPlayer = null;
		p.startPosition = new Vector3(5,50,5);
		p.currentPosition = p.startPosition;
		p.rotation = Quaternion.identity;
		p.sunStartPosition = new Vector3 (0, 30, 0);
		p.sunRotation = Quaternion.Euler (90, 0, 0);
		p.sunShadows = LightShadows.Soft;
		p.sunType = LightType.Directional;
		p.sunColor = new Color(255/255f, 248/255f, 188/255f, 255/255f);
	}

	#endregion

}

#region DataClasses

public class MainTerrainSettings
{
	public bool isDefaultSet;
	public bool isHeightSet;
	public bool isAlphaSet;
	public bool isZoneSet;
	public bool isVeggieSet;
	public bool isWaterSet;
	public bool currentlyUpdating;
	public bool isSaved;


	public int tileSizeX;
	public int tileSizeZ;
	public int tileSizeY;
	public int halfGlobalTilesX;
	public int halfGlobalTilesZ;
	public int halfLoadedTilesX;
	public int halfLoadedTilesZ;


	public Vector3 outOfBoundsTileNum;
	public Vector3 outOfBoundsPos;
	public int numZones;

	[SerializeField]
	public float[][] globalHeightMap;
	[SerializeField]
	public float[][][] globalAlphaMap;
	[SerializeField]
	public int[][] globalZoneMap;
	[SerializeField]
	public int[][] globalDetailMap;
	[SerializeField]
	public float[][] globalWaterMap;

	public VeggieObject[][] globalVeggieMap;

	public GameObject veggieParent;
	public GameObject tileParent;
	public TerrainTile[,] tileMap;
	public List<VeggieObject> veggiePool;

	public int heightMapResolution;
	public int alphaMapResolution;
	public int detailMapResolution;
	public int veggieMapResolution;
}

public class LoadingBarSettings
{
	public bool isSet;
	public bool isActive;
	public float totalToFill;
	public float totalFilled;
	public string name;
	public Vector2 position;
	public Vector2 size;
	public Image image;
}

public class MainPlayerSettings
{
	public bool isDefaultSet;
	public bool isPlayerSet;
	public bool needsUpdate;
	public bool fogEnabled;
	public Color fogColor;
	public float fogDensity;
	public Color skyColor;
	public CameraClearFlags clearFlags;
	public Vector3 startPosition;
	public Vector3 currentPosition;
	public string name;
	public Quaternion rotation;
	public GameObject mainPlayer;
	public GameObject theSun;
	public Vector3 sunStartPosition;
	public Quaternion sunRotation;
	public LightShadows sunShadows;
	public LightType sunType;
	public Color sunColor;
}


#endregion
