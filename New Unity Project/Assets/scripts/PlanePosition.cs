﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlanePosition : NetworkBehaviour {
	public enum PlaneType
	{
		Ground,
		Action,
		Player
	}

	[SyncVar]
	public int Row;
	[SyncVar]
	public int Column;
	[SyncVar]
	public PlaneType Type;

	public Vector2Int getPos()
	{
		return new Vector2Int(GetColumn(), GetRow());
	}

	public int GetRow()
	{
		return Row;
	}

	public int GetColumn()
	{
		return Column;
	}

	public PlaneType GetPlaneType()
	{
		return Type;
	}

	public bool Matches(int row, int column, PlaneType type)
	{
		return Row == row && Column == column && Type == type;
	}

	public void Set(int row, int column, PlaneType type)
	{
		Row = row;
		Column = column;
		Type = type;
	}

	public void Set(int[] index, PlaneType type)
	{
		Set(index[1], index[0], type);
	}

	public void Set(Vector2Int index, PlaneType type)
	{
		Set(index[1], index[0], type);
	}

	// Update is called once per frame
	void Update () {
		// Overkill to do this every frame, event handler would be better
		// I'm not sure what the actual layering here is, but this should update the order appropriately given the row
		int bias = 0;
		int mul = 1;
		// TODO: is this correct? It seems goune plane sort is just row, action is row*10 + 100, and player is row*10 + 101
		switch (Type)
		{
			case PlaneType.Action:
				mul = 10;
				bias = 110;
				break;
			case PlaneType.Player:
				mul = 10;
				bias = 110;
				break;
		}
		GetComponent<SpriteRenderer>().sortingOrder = Row * mul + bias;
	}
}
