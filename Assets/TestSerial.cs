using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;


public class TestSerial : MonoBehaviour
{

	public string portName;
	public int portSpeed = 9600;

	SerialPort serial;

	// Use this for initialization
	void Start ()
	{
		OpenPort();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (serial.IsOpen) {
			
			Debug.Log ("BytesToRead: " + serial.BytesToRead + "\n" +
				"ReadBufferSize:  " + serial.ReadBufferSize + "\n" +
				"Parity:  " + serial.Parity + "\n"+
				"DtrEnable:  " + serial.DtrEnable + "\n"+
				"RtsEnable:  " + serial.RtsEnable + "\n");

			if (serial.BytesToRead >= 0) {
				try {
					serial.ReadTimeout = 1000;

	//				int iByte = serial.ReadByte ();
	//				Debug.Log (iByte);

	//				int iChar = serial.ReadChar ();
	//				Debug.Log (iChar);

					string data = serial.ReadExisting();
					Debug.Log(data);

	//				char[] buffer = new char[10];
	//				serial.Read(buffer, 0, 1);
	//				Debug.Log(buffer);


				} catch (System.Exception e) {
					Debug.LogError(e);
				}
			}
			else if (serial.BytesToRead == -1) {
				// Happens when leonardo is reset, device disapears in system.
				Debug.Log("is open: " + serial.IsOpen);

				serial.Close();
			}
		}
		else {
			// try to open
			//serial.Open();
			OpenPort();
		}
	}

	void OpenPort() {
		if (serial == null) {
			//serial = new SerialPort (portName, portSpeed, Parity.None, 8, StopBits.One);
			serial = new SerialPort ();
		}

		if (serial.IsOpen) {
			serial.Close();
		}

		// Get a list of available ports
		List<string> portNames = new List<string> ();
		portNames.AddRange (System.IO.Ports.SerialPort.GetPortNames ());
		portNames.AddRange (System.IO.Directory.GetFiles ("/dev/", "cu.*"));
		Debug.Log (portNames.Count + "available ports: \n" + string.Join ("\n", portNames.ToArray ()));

		if (portName == "") {
			// try with last port of the list
			portName = portNames[portNames.Count - 1];
		}

		serial.PortName = portName;
		serial.BaudRate = portSpeed;

		try {
			serial.Open ();
			serial.DtrEnable = true; // Won't read from Leonardo without this
		} catch (System.Exception e) {
			Debug.LogError (e);
		}

		Debug.Log ("Port is open: " + serial.IsOpen);
	}
}
