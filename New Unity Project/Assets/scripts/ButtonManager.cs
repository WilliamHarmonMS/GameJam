using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour {

	bool delayTillUpdate = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (delayTillUpdate)
		{
			GetComponent<Button>().onClick.AddListener(f_ptr);
			delayTillUpdate = false;
		}
	}

	void f_ptr()
	{
		GameObject Player = GameObject.FindGameObjectWithTag("Player");
		Player.GetComponent<PlayerController>().handleButton(gameObject.name);
	}
}
