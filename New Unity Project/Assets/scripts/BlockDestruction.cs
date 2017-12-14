using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlockDestruction : NetworkBehaviour
{
	public bool go = true;
	public float timer = 0.5f;
	private float timerRatio;
	SpriteRenderer mySprite;

	// Use this for initialization
	void Start () {
		mySprite = GetComponent<SpriteRenderer>();
		timerRatio = timer;
	}
	
	// Update is called once per frame
	void Update () {
		if (go)
		{
			timer -= Time.deltaTime;
			float t = (timerRatio - timer) / timerRatio;
			mySprite.color = Color.Lerp(new Color(1,1,1,1), new Color(1,0,0,0), t);

			if(timer <= 0)
			{
				Destroy(this.gameObject);
				NetworkServer.UnSpawn(this.gameObject);
			}
		}
	}
}
