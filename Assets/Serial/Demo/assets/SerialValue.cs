
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SerialValue : MonoBehaviour {

	/// <summary>
	/// The column (0 based) from which to show the value. 
	/// </summary>
	public int Column = 0;

	void OnSerialValues(string[] values) {
		if (Column < values.Length) {
			//GetComponent<GUIText>().text = "Last value [" + Column + "]: " + values[Column];
			GetComponent<Text>().text = "Last value [" + Column + "]: " + values[Column];
		}
	}
}
