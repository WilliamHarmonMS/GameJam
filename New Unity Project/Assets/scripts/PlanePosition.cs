using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlanePosition : NetworkBehaviour {
	[SyncVar]
	private int Row;
	[SyncVar]
	private int Column;

	public void Set(int row, int column)
	{
		Row = row;
		Column = column;
	}

	public void Set(int[] index)
	{
		Set(index[1], index[0]);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Overkill to do this every frame, event handler would be better
		// I'm not sure what the actual layering here is, but this should update the order appropriately given the row
		GetComponent<SpriteRenderer>().sortingOrder = Row * 10;
	}
}
