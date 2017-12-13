using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugClick : MonoBehaviour {

	private void OnMouseDown()
	{
		Debug.Log(GetComponent<SpriteRenderer>().sortingOrder);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
}
