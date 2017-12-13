using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDeath : MonoBehaviour {

	public float Life;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Life -= Time.deltaTime;
		if(Life <= 0)
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
