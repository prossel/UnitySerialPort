using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SendCommand : MonoBehaviour
{

	public InputField txtCommand;
	public Toggle chkSendNewLine;

	public void OnSendCommand ()
	{
		Serial.Write (txtCommand.text + (chkSendNewLine.isOn ? "\n" : ""));
		txtCommand.ActivateInputField ();
	}
}
