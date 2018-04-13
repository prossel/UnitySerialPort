/**
 * Updates the last received line from Serial port
 * Requires the Serial Script attached to the game object as well
 */ 

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LastLine : MonoBehaviour {

	void OnSerialLine(string line) {
		//GetComponent<GUIText>().text = "Last line:\t" + line;
		GetComponent<Text>().text = "Last line:\t" + line;
	}

}
