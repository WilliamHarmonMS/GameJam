using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombBehavior : NetworkBehaviour {
	public float lifetime;
	public GameObject explosion;
	public int threatenedSpaces = 4;
	private float timer = 2.0f;
	private float flashTimer = 0.3f;


	private SpriteRenderer mySprite;
	// Use this for initialization
	void Start () {
		mySprite = GetComponent<SpriteRenderer>();
		timer = lifetime;
	}
	
	int[] getIndex()
	{
		GenerateLevel levelModel = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
		GameObject[,] groundPlane = levelModel.groundPlane;
		float shortest = float.MaxValue;
		int[] index = new int[2];
		for(int i = 0; i < levelModel.rowCount; ++i)
		{
			for(int j = 0; j < levelModel.columnCount; ++j)
			{
				float workingDist = Vector3.Distance(groundPlane[i, j].transform.position, transform.position + new Vector3(0, -0.1f, 0));
				if(workingDist < shortest)
				{
					shortest = workingDist;
					index[0] = i;
					index[1] = j;
				}
			}
		}
		return index;
	}

	void explode()
	{

		GenerateLevel levelModel = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
		GameObject[,] actionPlane = levelModel.actionPlane;
		GameObject[,] groundPlane = levelModel.groundPlane;
		int multiplier = 1;
		int[] index = getIndex();

		//start up
		Vector3 indexVector = new Vector3(0, -1, 0);

		for(int i = 0; i < threatenedSpaces; ++i)
		{
			if (i % 4 == 0 && i != 0)
			{
				++multiplier;
				indexVector *= multiplier;
			}
			if(index[0] + (int)indexVector.x < 0 || index[0] + (int)indexVector.x >= 10 || index[1] + (int)indexVector.y < 0 || index[1] + (int)indexVector.y >= 10)
			{
				//don't
			}
			else
			{
				GameObject actionBlock = actionPlane[index[0] + (int)indexVector.x, index[1] + (int)indexVector.y];
				if (actionBlock == null)
				{
					GameObject workingExplo = GameObject.Instantiate<GameObject>(explosion);
					float x = groundPlane[index[0] + (int)indexVector.x, index[1] + (int)indexVector.y].transform.position.x;
					float y = groundPlane[index[0] + (int)indexVector.x, index[1] + (int)indexVector.y].transform.position.y + 0.4f;

					workingExplo.transform.position = new Vector3(x, y, 0);
					workingExplo.GetComponent<SpriteRenderer>().sortingOrder = 2000;
					workingExplo.GetComponent<PlanePosition>().Set(index);
					NetworkServer.Spawn(workingExplo);
				}
			}



			indexVector = rotateAround(indexVector, 90);
		}
	}

	Vector3 rotateAround(Vector3 vector, float angle)
	{
		float theta = angle * Mathf.PI / 180.0f;

		float cs = Mathf.Cos(theta);
		float sn = Mathf.Sin(theta);

		float px = vector.x * cs - vector.y * sn;
		float py = vector.x * sn + vector.y * cs;

		return new Vector3(px, py, 0);
	}

	// Update is called once per frame
	void Update () {
		if(timer > 0)
		{
			timer -= Time.deltaTime;
			flashTimer -= Time.deltaTime;
			if(flashTimer <= 0)
			{
				if (mySprite.color == Color.white)
				{
					mySprite.color = Color.red;
				}
				else
				{
					mySprite.color = Color.white;
				}

				if(timer > lifetime * 0.66f)
				{
					flashTimer = 0.3f;
				} else if (timer > lifetime * 0.33f)
				{
					flashTimer = 0.2f;
				} else
				{
					flashTimer = 0.1f;
				}
			}
		}
		else
		{
			// Server authoritative explosion
			if (NetworkServer.active)
			{
				GameObject.Destroy(this.gameObject);
				//int[] index = getIndex();
				explode();
			}
		}
	}
}
