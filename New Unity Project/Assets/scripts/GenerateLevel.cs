using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GenerateLevel : NetworkBehaviour
{
	private bool makeRandom = false;

	public GameObject grass;
	public GameObject bricks;
	public GameObject steve;

	public int rowCount;
	public int columnCount;
	public int[] playerStartPosition = new int[2];
	public GameObject[,] groundPlane;
	public GameObject[,] actionPlane;
	public GameObject[] powerUps;

	[SyncVar]
	public int assignPlayer = 1;

	[SyncVar]
	public int playerOneIndex = 0;
	[SyncVar]
	public int playerTwoIndex = 9;
	[SyncVar]
	public int playerThreeIndex = 90;
	[SyncVar]
	public int playerFourIndex = 99;



	[SerializeField]
	public SortedDictionary<GameObject, string> blockList;

	// Use this for initialization
	private string[] levelData = new string[]
	{ "", "", "", "", "log", "", "", "", "", "",
	  "", "", "", "", "log", "", "stone", "", "", "",
		"", "", "stone", "", "stone", "stone", "stone", "", "", "",
		"", "log", "log", "", "log", "", "", "", "", "",
		"", "log", "stone", "", "stone", "", "stone", "", "", "",
		"", "", "log", "log", "", "", "", "log", "", "",
		"", "stone", "", "stone", "", "stone", "", "stone", "", "",
		"", "", "", "", "", "", "", "stone", "", "",
		"", "", "stone", "stone", "", "", "stone", "stone", "", "",
		"flint", "", "", "", "log", "", "", "", "", "",};


	void Start()
	{
		// Only server generates level
		if (!NetworkServer.active)
			return;
		groundPlane = new GameObject[rowCount, columnCount];
		actionPlane = new GameObject[rowCount, columnCount];

		//make ground plane
		for (int i = 0; i < rowCount; ++i)
		{
			for (int j = 0; j < columnCount; ++j)
			{
				GameObject block = GameObject.Instantiate<GameObject>(grass);
				block.transform.position = new Vector3(i, -j, 0);
				block.GetComponent<SpriteRenderer>().sortingOrder = j;
				float randomTint = Random.Range(0.8f, 1.0f);
				// TODO: This needs to be synced, won't do anything here
				block.GetComponent<SpriteRenderer>().color = new Color(randomTint, randomTint, randomTint, 1);
				SpawnInPlane(block, i, j, groundPlane);
			}
		}

		//make action plane
		if (makeRandom)
		{
			for (int i = 0; i < rowCount; ++i)
			{
				for (int j = 0; j < columnCount; ++j)
				{
					int doPopulate = Random.Range(0, 5);
					if (doPopulate == 0)
					{
						GameObject block = GameObject.Instantiate<GameObject>(bricks);
						block.GetComponent<SpriteRenderer>().sortingOrder = (j * 10) + 100;
						//float randomTint = Random.Range(0.8f, 1.0f);
						//block.GetComponent<SpriteRenderer>().color = new Color(randomTint, randomTint, randomTint, 1);
						SpawnInPlane(block, i, j, actionPlane);
					}
					else
					{
						int spawnPowerUp = Random.Range(0, 31);
						if (spawnPowerUp == 0)
						{
							int powerUpType = Random.Range(0, 4);
							GameObject pUp = GameObject.Instantiate<GameObject>(powerUps[powerUpType]);
							pUp.tag = "PowerUp";
							SpawnInPlane(pUp, i, j, actionPlane);
						}
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < rowCount; ++i)
			{
				for (int j = 0; j < columnCount; ++j)
				{
					int index = j + (i * columnCount);
					string blockType = levelData[index];
					if (blockType != "")
					{
						GameObject prefab = (GameObject)Resources.Load(blockType);
						GameObject block = GameObject.Instantiate(prefab);
						SpawnInPlane(block, i, j, actionPlane);
					}
				}
			}
		}

		//Can't generate them now, players should look for a spawn position in OnLocalPlayerStart
		//if(actionPlane[playerStartPosition[0], playerStartPosition[1]] == null)
		//{
		//	GameObject player = GameObject.Instantiate<GameObject>(steve);
		//	player.transform.position = new Vector3(playerStartPosition[0], playerStartPosition[1] + 0.5f, 0);
		//}
	}


	public int positionToIndex(int x, int y)
	{
		return (y * columnCount) + x;
	}

	public Vector2Int indexToPosition(int index)
	{
		Vector2Int returnValue = new Vector2Int();
		returnValue.y = (int)(index / columnCount);
		returnValue.x = index - (returnValue.y * columnCount);
		return returnValue;
	}
	// Update is called once per frame
	void Update()
	{
	}

	private void SpawnInPlane(GameObject obj, int row, int col, GameObject[,] plane)
	{
		// TODO: this is silly but I don't want to think about it right now
		PlanePosition.PlaneType t = PlanePosition.PlaneType.Ground;
		float yOffset = 0.0f;
		if (plane == actionPlane)
		{
			t = PlanePosition.PlaneType.Action;
			yOffset = 0.5f;
		}

		obj.transform.position = new Vector3(col, -row + yOffset, 0);
		plane[col, row] = obj;
		PlanePosition pos = obj.GetComponent<PlanePosition>();
		if (pos)
			pos.Set(row, col, t);
		NetworkServer.Spawn(obj);
	}
}
