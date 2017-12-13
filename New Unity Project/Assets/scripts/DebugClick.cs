using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugClick : MonoBehaviour {

	private void OnMouseDown()
	{
		Debug.Log("sorting order of this block is: " + GetComponent<SpriteRenderer>().sortingOrder);
		Debug.Log("tag of this object is: " + gameObject.tag);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
}
