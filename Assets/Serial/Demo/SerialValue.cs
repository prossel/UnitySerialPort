using UnityEngine;
using System.Collections;

public class SerialValue : MonoBehaviour {

	/// <summary>
	/// The column from which to show the value.
	/// </summary>
	public int Column = 0;

	void OnSerialValue(string value, int col) {
		if (col == Column) {
			guiText.text = "Last value [" + Column + "]: " + value;
		}
	}
}
