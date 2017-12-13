using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour {

	bool delayTillUpdate = true;

	private GameObject Player = null;

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
		if (!this.Player)
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject player in players)
			{
				if (player.GetComponent<PlayerController>().isLocalPlayer)
				{
					this.Player = player;
					break;
				}
			}
		}

		if (this.Player)
		{
			this.Player.GetComponent<PlayerController>().handleButton(gameObject.name);
		}
	}
}
