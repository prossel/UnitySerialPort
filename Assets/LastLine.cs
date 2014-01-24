/**
 * Updates the last received line from Serial port
 * Requires the Serial Script attached to the game object as well
 */ 

using UnityEngine;
using System.Collections;

public class LastLine : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnSerialLine(string line) {
		guiText.text = "Last line: " + line;
	}

}
