using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class PlayerController : NetworkBehaviour
{
	public enum Facing
	{
		Up,
		Right,
		Down,
		Left
	};

	public enum PowerUp
	{
		None,
		Pick,
		Axe,
		Shovel,
		Hoe,
		Flint
	};

	private GenerateLevel levelModel;
	public GameObject[,] groundPlane;
	public GameObject[,] actionPlane;
	public GameObject bomb;
	public GameObject block;
	public GameObject destroyOverlay;
	public float speed = 1.0f;
	public Facing facingDirection = Facing.Down;
	public PowerUp currentPowerUp = PowerUp.None;

	private int[] playerIndex = new int[2];
	private bool moving = false;
	private bool busy = false;
	private Vector3 moveDirection = Vector3.zero;
	private float t = 0.0f;
	private Vector3 lerpAnchor = Vector3.zero;
	

	// Use this for initialization
	void Start()
	{
		levelModel = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
		groundPlane = levelModel.groundPlane;
		actionPlane = levelModel.actionPlane;
		playerIndex = levelModel.playerStartPosition;
		GetComponent<SpriteRenderer>().sortingOrder = playerIndex[1] + 101;
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		// Initalize things here, maybe grab a spawn location?
	}

	Vector3 setMoveDirection(int x, int y)
	{
		GameObject indexOccupant = actionPlane[playerIndex[0] + x, playerIndex[1] - y];
		if (indexOccupant != null)
		{
			if (indexOccupant.tag == "ActionBlock")
			{
				return Vector3.zero;
			}
			if (indexOccupant.tag == "PowerUp")
			{
				assignPower(indexOccupant);
			}
		}

		playerIndex[0] = playerIndex[0] + x;
		playerIndex[1] = playerIndex[1] - y;
		moving = true;
		return new Vector3(x, y, 0);
	}

	

	void assignPower(GameObject pUp)
	{
		if (pUp.name.StartsWith("pick"))
		{
			currentPowerUp = PowerUp.Pick;
		} else if (pUp.name.StartsWith("axe"))
		{
			currentPowerUp = PowerUp.Axe;
		}	else if (pUp.name.StartsWith("hoe"))
		{
			currentPowerUp = PowerUp.Hoe;
		} else if (pUp.name.StartsWith("flint"))
		{
			currentPowerUp = PowerUp.Flint;
		} else if (pUp.name.StartsWith("shovel"))
		{
			currentPowerUp = PowerUp.Shovel;
		}
		Destroy(pUp);
	}

	bool checkConflict(Facing direction)
	{
		int[] offset = new int[] { 0, 0 };
		switch (direction)
		{
			case Facing.Up:
				offset[1] = -1;
				break;
			case Facing.Right:
				offset[0] = 1;
				break;
			case Facing.Down:
				offset[1] = 1;
				break;
			case Facing.Left:
				offset[0] = -1;
				break;
		}

		GameObject actionBlock = actionPlane[playerIndex[0] + offset[0], playerIndex[1] + offset[1]];
		return actionBlock != null;
	}

	GameObject getBlockAt(Facing direction)
	{
		int[] offset = new int[] { 0, 0 };
		switch (direction)
		{
			case Facing.Up:
				offset[1] = -1;
				break;
			case Facing.Right:
				offset[0] = 1;
				break;
			case Facing.Down:
				offset[1] = 1;
				break;
			case Facing.Left:
				offset[0] = -1;
				break;
		}

		GameObject actionBlock = actionPlane[playerIndex[0] + offset[0], playerIndex[1] + offset[1]];
		return actionBlock;
	}

	bool checkBusy()
	{
		return moving || busy;
	}

	// Update is called once per frame

	IEnumerator busyDelay(int seconds)
	{
		yield return new WaitForSeconds(seconds);
		busy = false;
	}

	public void handleButton(string button)
	{
		if (moveDirection == Vector3.zero && !checkBusy())
		{
			if (button == "left")
			{
				if (!checkConflict(Facing.Left) || getBlockAt(Facing.Left).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(-1, 0);
					facingDirection = Facing.Left;
				}
			}
			else if (button == "right")
			{
				if (!checkConflict(Facing.Right) || getBlockAt(Facing.Right).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(1, 0);
					facingDirection = Facing.Right;
				}
			}
			else if (button == "up")
			{
				if (!checkConflict(Facing.Up) || getBlockAt(Facing.Up).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(0, 1);
					facingDirection = Facing.Up;
					GetComponent<SpriteRenderer>().sortingOrder -= 10;
				}
			}
			else if (button == "down")
			{
				if (!checkConflict(Facing.Down) || getBlockAt(Facing.Down).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(0, -1);
					facingDirection = Facing.Down;
					GetComponent<SpriteRenderer>().sortingOrder += 10;
				}
			}
		}
	}

	void Update()
	{
		// Multiplayer players sync themselves
		if (!isLocalPlayer)
			return;

		if (moveDirection == Vector3.zero && !checkBusy())
		{
			if (Input.GetButton("left"))
			{
				if(!checkConflict(Facing.Left) || getBlockAt(Facing.Left).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(-1, 0);
					facingDirection = Facing.Left;
				}
			} else if (Input.GetButton("right"))
			{
				if (!checkConflict(Facing.Right) || getBlockAt(Facing.Right).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(1, 0);
					facingDirection = Facing.Right;
				}
			} else if (Input.GetButton("up"))
			{
				if (!checkConflict(Facing.Up) || getBlockAt(Facing.Up).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(0, 1);
					facingDirection = Facing.Up;
					GetComponent<SpriteRenderer>().sortingOrder -= 10;
				}
			} else if (Input.GetButton("down"))
			{
				if (!checkConflict(Facing.Down) || getBlockAt(Facing.Down).tag != "ActionBlock")
				{
					moveDirection = setMoveDirection(0, -1);
					facingDirection = Facing.Down;
					GetComponent<SpriteRenderer>().sortingOrder += 10;
				}
			}
		}

		if (moving)
		{
			if (lerpAnchor == Vector3.zero)
			{
				lerpAnchor = transform.position;
			}

			transform.position = Vector3.Lerp(lerpAnchor, lerpAnchor + moveDirection, t);
			t += Time.deltaTime * speed;

			if (t >= 1)
			{
				moveDirection = Vector3.zero;
				lerpAnchor = Vector3.zero;
				t = 0;
				moving = false;
			}
		}

		if (Input.GetButtonDown("shoot") && !checkBusy())
		{
			// Shoot on the server
			CmdPlaceBomb();

			/*GameObject activeBomb = GameObject.Instantiate<GameObject>(bomb);
			activeBomb.transform.position = new Vector3(playerIndex[0], -playerIndex[1] + 0.5f, 0);
			switch (facingDirection)
			{
				case Facing.Up:
					activeBomb.transform.position += new Vector3(0, 1, 0);
					activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 10;
					break;
				case Facing.Right:
					activeBomb.transform.position  += new Vector3(1, 0, 0);
					activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
					break;
				case Facing.Down:
					activeBomb.transform.position  += new Vector3(0, -1, 0);
					activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 10;
					break;
				case Facing.Left:
					activeBomb.transform.position  += new Vector3(-1, 0, 0);
					activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
					break;
			}*/


		}

		if (Input.GetButtonDown("place") && !checkBusy())
		{
			GameObject placedBlock = GameObject.Instantiate<GameObject>(block);
			placedBlock.transform.position = new Vector3(playerIndex[0], -playerIndex[1] + 0.5f, 0);
			switch (facingDirection)
			{
				case Facing.Up:
					placedBlock.transform.position += new Vector3(0, 1, 0);
					actionPlane[playerIndex[0], playerIndex[1] - 1] = placedBlock;
					placedBlock.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 10;
					break;
				case Facing.Right:
					placedBlock.transform.position += new Vector3(1, 0, 0);
					actionPlane[playerIndex[0] + 1, playerIndex[1]] = placedBlock;
					placedBlock.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
					break;
				case Facing.Down:
					placedBlock.transform.position += new Vector3(0, -1, 0);
					actionPlane[playerIndex[0], playerIndex[1] + 1] = placedBlock;
					placedBlock.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 10;
					break;
				case Facing.Left:
					placedBlock.transform.position += new Vector3(-1, 0, 0);
					actionPlane[playerIndex[0] - 1, playerIndex[1]] = placedBlock;
					placedBlock.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
					break;
			}
		}

		if(Input.GetButtonDown("destroy") && !checkBusy())
		{
			GameObject actionBlock = null;
			switch (facingDirection)
			{
				case Facing.Up:
					actionBlock = actionPlane[playerIndex[0], playerIndex[1] - 1];
					break;
				case Facing.Right:
					actionBlock = actionPlane[playerIndex[0] + 1, playerIndex[1]];
					break;
				case Facing.Down:
					actionBlock = actionPlane[playerIndex[0], playerIndex[1] + 1];
					break;
				case Facing.Left:
					actionBlock = actionPlane[playerIndex[0] - 1, playerIndex[1]];
					break;
			}

			if(actionBlock != null)
			{
				busy = true;
				GameObject overlay = GameObject.Instantiate<GameObject>(destroyOverlay);
				overlay.GetComponent<SpriteRenderer>().sortingOrder = actionBlock.GetComponent<SpriteRenderer>().sortingOrder + 1;
				overlay.transform.position = actionBlock.transform.position;
				TimedDeath td = actionBlock.AddComponent<TimedDeath>();
				td.Life = 1.0f;
				StartCoroutine(busyDelay(1));
			}
		}
	}

	[Command]
	void CmdPlaceBomb()
	{
		GameObject activeBomb = GameObject.Instantiate<GameObject>(bomb);
		activeBomb.transform.position = new Vector3(playerIndex[0], -playerIndex[1] + 0.5f, 0);
		activeBomb.transform.position += FaceToVec(facingDirection);
		activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + FaceToSort(facingDirection);
		activeBomb.GetComponent<PlanePosition>().Set(playerIndex);
		NetworkServer.Spawn(activeBomb);
	}

	public static Vector3 FaceToVec(Facing facing)
	{
		switch (facing)
		{
			case Facing.Up: return Vector3.up;
			case Facing.Right: return Vector3.right;
			case Facing.Down: return Vector3.down;
			case Facing.Left: return Vector3.left;
			default: return Vector3.zero;
		}
	}

	public static int FaceToSort(Facing facing)
	{
		switch (facing)
		{
			case Facing.Up: return -10;
			case Facing.Down: return 10;
			default: return 0;
		}
	}
}

