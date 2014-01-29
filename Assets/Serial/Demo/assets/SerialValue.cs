using UnityEngine;
using System.Collections;

public class SerialValue : MonoBehaviour {

	/// <summary>
	/// The column (0 based) from which to show the value. 
	/// </summary>
	public int Column = 0;

	void OnSerialValues(string[] values) {
		if (Column < values.Length) {
			guiText.text = "Last value [" + Column + "]: " + values[Column];
		}
	}
}
