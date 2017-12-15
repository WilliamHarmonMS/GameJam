using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MainMenuManager : MonoBehaviour {

	public GameObject myNetworkManager;
	public GameObject[] UIElements;
	public GameObject[] ControllerElements;

	// Use this for initialization
	void Start () {
		foreach(GameObject obj in ControllerElements)
		{
			obj.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void pressHost()
	{
		myNetworkManager.GetComponent<NetworkManager>().StartHost();
		clearUI();
		activateControls();
	}

	public void pressJoin()
	{
		myNetworkManager.GetComponent<NetworkManager>().StartClient();
		clearUI();
		activateControls();
	}

	void clearUI()
	{
		foreach(GameObject obj in UIElements)
		{
			obj.SetActive(false);
		}
	}

	void activateControls()
	{
		foreach (GameObject obj in ControllerElements)
		{
			obj.SetActive(true);
		}
	}
}
