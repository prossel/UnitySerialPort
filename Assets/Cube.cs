using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cube : MonoBehaviour {

	Serial serial;

	// Use this for initialization
	void Start () {
		serial = Camera.main.GetComponent<Serial>();
	}
	
	// Update is called once per frame
	void Update () {

		transform.Rotate(0, 1, 0);

		string line = serial.GetLastLine();
		//print ("got this last line: " + line);
		if (line.Length > 0) {
			string[] values = line.Split('\t');
			int val = int.Parse(values[1]);
			//transform.Rotate(val, 0, 0);
			transform.position = new Vector3((val) / 1000.0f, transform.position.y, transform.position.z);
		}


//		List<string> lines = serial.GetLines();
//		foreach(string line in lines) {
//			print ("got this line: " + line);
//		}
	}
}
