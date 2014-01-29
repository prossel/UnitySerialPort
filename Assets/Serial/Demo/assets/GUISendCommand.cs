using UnityEngine;
using System.Collections;

public class GUISendCommand : MonoBehaviour {

	private string txtCommand = "";
	private bool chkSendNewLine = true;

	void OnGUI() {

		// Command text field with send button
		GUILayout.BeginArea (new Rect(10, Screen.height - 30, Screen.width - 20, 30));
		GUILayout.BeginHorizontal();
		GUILayout.Label ("Send to serial");
		txtCommand = GUILayout.TextField(txtCommand);
		chkSendNewLine = GUILayout.Toggle(chkSendNewLine, "add \\n");
		if (GUILayout.Button("Send")) {
			print("button");
			Serial.Write(txtCommand);
			if (chkSendNewLine)
				Serial.Write("\n");
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
