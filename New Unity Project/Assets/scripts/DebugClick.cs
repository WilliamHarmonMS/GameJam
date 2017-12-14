using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugClick : MonoBehaviour {

	private void OnMouseDown()
	{
		Debug.Log("sort order of clicked is: " + GetComponent<SpriteRenderer>().sortingOrder);
		Debug.Log("position of clicked is: " + transform.position);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
}
