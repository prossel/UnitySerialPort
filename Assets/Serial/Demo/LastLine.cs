/**
 * Updates the last received line from Serial port
 * Requires the Serial Script attached to the game object as well
 */ 

using UnityEngine;
using System.Collections;

public class LastLine : MonoBehaviour {

	void OnSerialLine(string line) {
		guiText.text = "Last line:\t" + line;
	}

}
