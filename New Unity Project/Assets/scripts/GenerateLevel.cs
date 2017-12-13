﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenerateLevel : MonoBehaviour {
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

	[SerializeField]
	public SortedDictionary<GameObject, string> blockList;

	// Use this for initialization
	private string[] levelData = new string[] 
	{ "", "", "bricks", "", "", "", "", "", "", "",
	  "", "", "bricks", "", "", "", "bricks", "", "", "",
		"", "", "bricks", "", "", "", "", "", "", "",
		"", "", "bricks", "", "bricks", "", "bricks", "", "", "",
		"", "", "bricks", "", "", "", "", "", "", "",
		"", "", "bricks", "", "", "", "", "", "", "",
		"", "", "bricks", "", "", "bricks", "", "", "", "",
		"", "", "bricks", "", "", "", "", "", "", "",
		"", "", "bricks", "", "", "", "", "bricks", "", "",
		"", "", "bricks", "", "", "", "", "", "", "",};

	void Start () {
		groundPlane = new GameObject[rowCount,columnCount];
		actionPlane = new GameObject[rowCount, columnCount];

		//make ground plane
		for (int i = 0; i < rowCount; ++i)
		{
			for(int j = 0; j < columnCount; ++j)
			{
				GameObject block = GameObject.Instantiate<GameObject>(grass);
				block.transform.position = new Vector3(i, -j, 0);
				block.GetComponent<SpriteRenderer>().sortingOrder = j;
				float randomTint = Random.Range(0.8f, 1.0f);
				block.GetComponent<SpriteRenderer>().color = new Color(randomTint, randomTint, randomTint, 1);
				groundPlane[i, j] = block;
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
						block.transform.position = new Vector3(i, -j + 0.5f, 0);
						//float randomTint = Random.Range(0.8f, 1.0f);
						//block.GetComponent<SpriteRenderer>().color = new Color(randomTint, randomTint, randomTint, 1);
						actionPlane[i, j] = block;
					}
					else
					{
						int spawnPowerUp = Random.Range(0, 31);
						if (spawnPowerUp == 0)
						{
							int powerUpType = Random.Range(0, 4);
							GameObject pUp = GameObject.Instantiate<GameObject>(powerUps[powerUpType]);
							pUp.GetComponent<SpriteRenderer>().sortingOrder = (j * 10) + 100;
							pUp.transform.position = new Vector3(i, -j + 0.5f, 0);
							pUp.tag = "PowerUp";
							actionPlane[i, j] = pUp;
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
						block.GetComponent<SpriteRenderer>().sortingOrder = (i * 10) + 100;
						block.transform.position = new Vector3(j, -i + 0.5f, 0);
						actionPlane[j, i] = block;
					}
				}
			}
		}


		if(actionPlane[playerStartPosition[0], playerStartPosition[1]] == null)
		{
			GameObject player = GameObject.Instantiate<GameObject>(steve);
			player.transform.position = new Vector3(playerStartPosition[0], playerStartPosition[1] + 0.5f, 0);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
