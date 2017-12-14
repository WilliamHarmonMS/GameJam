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
	public GameObject bomb;
	public GameObject block;
	public GameObject destroyOverlay;
	public float speed = 1.0f;
	public Facing facingDirection = Facing.Down;
	public PowerUp currentPowerUp = PowerUp.None;

	private Vector2Int playerIndex = new Vector2Int();
	private Vector2Int realPlayerIndex = new Vector2Int();

	private bool moving = false;
	private bool busy = false;
	private Vector3 moveDirection = Vector3.zero;
	private float t = 0.0f;
	private Vector3 lerpAnchor = Vector3.zero;
	private int playerNumber = -1;

	public Sprite[] spriteList;
	public RuntimeAnimatorController[] animationList;
	// Use this for initialization
	void Start()
	{
		levelModel = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
	}

	public override void OnStartServer()
	{
		GenerateLevel serverInfo = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
		playerNumber = serverInfo.assignPlayer;
		++serverInfo.assignPlayer;
		SetPlayerIndex(serverInfo.indexToPosition(serverInfo.playerOneIndex));
		GetComponent<SpriteRenderer>().sortingOrder = playerIndex.y + 101;
		transform.position = new Vector3(playerIndex.x, playerIndex.y + 0.5f, 0);
	}

	public override void OnStartLocalPlayer()
	{
		GenerateLevel serverInfo = GameObject.FindGameObjectWithTag("LevelModel").GetComponent<GenerateLevel>();
		if(playerNumber == -1)
		{
			playerNumber = serverInfo.assignPlayer;
			++serverInfo.assignPlayer;

			int workingIndex = 0;
			switch (playerNumber)
			{
				case 2:
					workingIndex = serverInfo.playerTwoIndex;
					break;
				case 3:
					workingIndex = serverInfo.playerThreeIndex;
					break;
				case 4:
					workingIndex = serverInfo.playerFourIndex;
					break;
			}
			int offset = (playerNumber - 1) * 8;
			GetComponent<SpriteRenderer>().sprite = spriteList[0 + offset];
			SetPlayerIndex(serverInfo.indexToPosition(workingIndex));
			GetComponent<SpriteRenderer>().sortingOrder = playerIndex.y + 101;
			transform.position = new Vector3(playerIndex.x, -playerIndex.y + 0.5f, 0);
		}

		Debug.Log("Player number is: " + playerNumber);
		Debug.Log("realPlayerIndex: " + playerIndex);
		Debug.Log("position: " + transform.position);
		Debug.Log("sort order is: " + GetComponent<SpriteRenderer>().sortingOrder);
		base.OnStartLocalPlayer();
		// Initalize things here, maybe grab a spawn location?
	}

	void setMoveDirection(int x, int y)
	{
		if (x != 0 || y != 0)
		{
			GameObject indexOccupant = GetFromPlane(playerIndex[1] - y, playerIndex[0] + x, PlanePosition.PlaneType.Action);
			if (indexOccupant != null)
			{
				if (indexOccupant.tag == "ActionBlock")
				{
					moveDirection = Vector3.zero;
					return;
				}
				if (indexOccupant.tag == "PowerUp")
				{
					assignPower(indexOccupant);
				}
			}
		}

		SetPlayerIndex(playerIndex + new Vector2Int(x, -y));
		updateIndexOnServer(playerNumber);
		moving = true;
		moveDirection = new Vector3(x, y, 0);
		CmdSyncMoveDir(moveDirection);
	}

	[Command]
	void CmdSyncMoveDir(Vector3 dir)
	{
		moveDirection = dir;
		RpcClientSyncMoveDir(dir);
	}

	[ClientRpc]
	void RpcClientSyncMoveDir(Vector3 dir)
	{
		if (!hasAuthority)
			moveDirection = dir;
	}

	void updateIndexOnServer(int myNumber)
	{
		switch (myNumber)
		{
			case 1:
				levelModel.playerOneIndex = levelModel.positionToIndex(playerIndex.x, playerIndex.y);
				break;
			case 2:
				levelModel.playerTwoIndex = levelModel.positionToIndex(playerIndex.x, playerIndex.y);
				break;
			case 3:
				levelModel.playerThreeIndex = levelModel.positionToIndex(playerIndex.x, playerIndex.y);
				break;
			case 4:
				levelModel.playerFourIndex = levelModel.positionToIndex(playerIndex.x, playerIndex.y);
				break;
		}
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
		Vector2Int offset = FaceToIndex(direction);
		Vector2Int pIndex = new Vector2Int(playerIndex[0], playerIndex[1]);
		Vector2Int index = pIndex + offset;
		GameObject actionBlock = GetFromPlane(index, PlanePosition.PlaneType.Action);
		GameObject groundBlock = GetFromPlane(index, PlanePosition.PlaneType.Ground);
		bool isTool = actionBlock && actionBlock.tag != "ActionBlock";
		return (actionBlock != null && !isTool) || groundBlock == null;
	}

	GameObject getBlockAt(Facing direction)
	{
		Vector2Int offset = FaceToIndex(direction);
		Vector2Int index = new Vector2Int(playerIndex[0], playerIndex[1]) + offset;
		GameObject actionBlock = GetFromPlane(index, PlanePosition.PlaneType.Action);
		return actionBlock;
	}

	string getTagAt(Facing direction)
	{
		GameObject obj = getBlockAt(direction);
		return obj ? obj.tag : "";
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
		// Multiplayer players sync themselves
		if (!isLocalPlayer)
			return;
		if (!checkBusy())
		{
			if (moveDirection == Vector3.zero)
			{
				if (button == "left")
				{
					if (!checkConflict(Facing.Left))
					{
						setMoveDirection(-1, 0);
						facingDirection = Facing.Left;
					}
				}
				else if (button == "right")
				{
					if (!checkConflict(Facing.Right))
					{
						setMoveDirection(1, 0);
						facingDirection = Facing.Right;
					}
				}
				else if (button == "up")
				{
					if (!checkConflict(Facing.Up))
					{
						setMoveDirection(0, 1);
						facingDirection = Facing.Up;
					}
				}
				else if (button == "down")
				{
					if (!checkConflict(Facing.Down))
					{
						setMoveDirection(0, -1);
						facingDirection = Facing.Down;
					}
				}
			}

			if(button == "A")
			{
				CmdDestroyBlock();
			}

			if(button == "B")
			{
				Debug.Log("player number is: " + playerNumber);
				CmdPlaceBomb(playerIndex.x, playerIndex.y);
			}
		}

	}

	void Update()
	{
		// Multiplayer players sync themselves
		if (isLocalPlayer)
		{
			UpdateInput();
		}
		UpdateAnimation();
	}

	void UpdateAnimation()
	{
		int offset = (playerNumber - 1) * 8;
		Animator anim = GetComponent<Animator>();
		if (anim.runtimeAnimatorController == null && moveDirection != Vector3.zero)
		{
			Facing facing = VecToFace(moveDirection);
			SetSprite(facing, false);
			anim.runtimeAnimatorController = animationList[(GetSpriteIndex(facing) + 4) + offset];
		}
		else if (anim.runtimeAnimatorController != null && moveDirection == Vector3.zero)
		{
			SetSprite(VecToFace(moveDirection), false);
			anim.runtimeAnimatorController = null;
		}
	}

	void SetSprite(Facing dir, bool moving)
	{
		int spriteIndex = GetSpriteIndex(dir);
		if (moving)
			spriteIndex += 4;
		GetComponent<SpriteRenderer>().sprite = spriteList[spriteIndex];
	}

	int GetSpriteIndex(Facing dir)
	{
		switch (dir)
		{
			case Facing.Down: return 0;
			case Facing.Right: return 1;
			case Facing.Left: return 2;
			case Facing.Up: return 3;
			default: return 0;
		}
	}

	void UpdateInput()
	{
		if (moveDirection == Vector3.zero && !checkBusy())
		{
			if (Input.GetButton("left"))
			{
				if (!checkConflict(Facing.Left))
				{
					setMoveDirection(-1, 0);
					facingDirection = Facing.Left;
				}
			}
			else if (Input.GetButton("right"))
			{
				if (!checkConflict(Facing.Right))
				{
					setMoveDirection(1, 0);
					facingDirection = Facing.Right;
				}
			}
			else if (Input.GetButton("up"))
			{
				if (!checkConflict(Facing.Up))
				{
					setMoveDirection(0, 1);
					facingDirection = Facing.Up;
					GetComponent<SpriteRenderer>().sortingOrder -= 10;
				}
			}
			else if (Input.GetButton("down"))
			{
				if (!checkConflict(Facing.Down))
				{
					setMoveDirection(0, -1);
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

			transform.position = Vector3.Lerp(lerpAnchor, lerpAnchor + moveDirection * 1.05f, t);
			t += Time.deltaTime * speed;

			if (t >= 1)
			{
				setMoveDirection(0, 0);
				lerpAnchor = Vector3.zero;
				t = 0;
				moving = false;
			}
		}

		if (Input.GetButtonDown("shoot") && !checkBusy())
		{
			// Shoot on the server
			CmdPlaceBomb(playerIndex.x, playerIndex.y);
		}

		if (Input.GetButtonDown("place") && !checkBusy())
		{
			// TODO: command to make this server side, who knows about the action plane and will put it there.
			/*GameObject placedBlock = GameObject.Instantiate<GameObject>(block);
			placedBlock.transform.position = new Vector3(playerIndex[0], -playerIndex[1] + 0.5f, 0);
			Vector3 posOffset = Vector3.zero;
			Vector2Int index = new Vector2Int(playerIndex[0], playerIndex[1]);
			switch (facingDirection)
			{
				case Facing.Up:
					posOffset = Vector3.up;
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
			}*/
		}

		if (Input.GetButtonDown("destroy") && !checkBusy())
		{
			GameObject actionBlock = null;
			Vector2Int index = new Vector2Int(playerIndex[0], playerIndex[1]);
			index += FaceToIndex(facingDirection);
			actionBlock = GetFromPlane(index.y, index.x, PlanePosition.PlaneType.Action);
			if (actionBlock != null)
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
	void CmdPlaceBomb(int x, int y)
	{
		Vector2Int activeIndex = new Vector2Int(x, y);
		GameObject activeBomb = GameObject.Instantiate<GameObject>(bomb);
		activeBomb.transform.position = new Vector3(activeIndex[0], -activeIndex[1] + 0.5f, 0);
		activeBomb.transform.position += FaceToVec(facingDirection);
		activeBomb.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + FaceToSort(facingDirection);
		activeBomb.GetComponent<PlanePosition>().Set(activeIndex, PlanePosition.PlaneType.Action);
		NetworkServer.Spawn(activeBomb);
	}

	[Command]
	void CmdDestroyBlock()
	{
		GameObject actionBlock = null;
		Vector2Int index = new Vector2Int(playerIndex[0], playerIndex[1]);
		index += FaceToIndex(facingDirection);
		actionBlock = GetFromPlane(index.y, index.x, PlanePosition.PlaneType.Action);
		if (actionBlock != null)
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

	public static Facing VecToFace(Vector3 v)
	{
		if (v == Vector3.up)
			return Facing.Up;
		if (v == Vector3.right)
			return Facing.Right;
		if (v == Vector3.down)
			return Facing.Down;
		if (v == Vector3.left)
			return Facing.Left;
		return Facing.Down;
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

	public static Vector2Int FaceToIndex(Facing facing)
	{
		switch (facing)
		{
			case Facing.Up: return Vector2Int.down;
			case Facing.Right: return Vector2Int.right;
			case Facing.Down: return Vector2Int.up;
			case Facing.Left: return Vector2Int.left;
			default: return Vector2Int.zero;
		}
	}

	private Vector3 CoordsToVec(int row, int col)
	{
		return new Vector3(col, -row + 0.5f, 0.0f);
	}

	private Vector2 CoordsToVec2(int row, int col)
	{
		return new Vector2(col, -row + 0.5f);
	}

	private GameObject GetFromPlane(Vector2Int index, PlanePosition.PlaneType type)
	{
		return GetFromPlane(index.y, index.x, type);
	}

	private GameObject GetFromPlane(int row, int col, PlanePosition.PlaneType type)
	{
		Vector2 start = CoordsToVec2(row, col);
		Collider2D[] hits = Physics2D.OverlapBoxAll(start, Vector2.one * 0.1f, 0.0f);
		foreach (Collider2D hit in hits)
		{
			GameObject obj = hit.gameObject;
			PlanePosition p = obj.GetComponent<PlanePosition>();
			if (p && p.Matches(row, col, type))
				return obj;
		}
		return null;
	}

	private void SetPlayerIndex(Vector2Int index)
	{
		playerIndex = index;
		GetComponent<PlanePosition>().Set(index, PlanePosition.PlaneType.Player);
	}
}

