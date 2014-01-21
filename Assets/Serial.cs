using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO.Ports; 
// System.IO.Ports requires a working Serial Port. On Mac, you will need to purcase the Uniduino plug-in on the Unity Store
// This adds a folder + a file into your local folder at ~/lib/libMonoPosixHelper.dylib
// This file will activate your serial port for C# / .NET
// The functions are the same as the standard C# SerialPort library
// cf. http://msdn.microsoft.com/en-us/library/system.io.ports.serialport(v=vs.110).aspx


public class Serial : MonoBehaviour {

	SerialPort serial;

	//private string serialIn = "";
	//string serialOut = "";
	private List<string> linesIn = new List<string>();

//	public int ReceivedBytesCount { get { return serialIn.Length; } }
//	public string ReceivedBytes { get { return serialIn; } }

	/// <summary>
	/// Gets the lines count.
	/// </summary>
	/// <value>The lines count.</value>
	public int linesCount { get { return linesIn.Count; } }

	// buffer data as they arrive, until a new line is received
	private string bufferIn = "";

	void Start() {

		string portName = GetPortName();

		if (portName == "") {
			print("Error: Couldn't find serial port.");
			return;
		}
		else {
			//print("Opening serial port: " + portName);
		}

		serial = new SerialPort(portName, 9600);

		serial.Open();
		//print ("default ReadTimeout: " + serial.ReadTimeout);
		//serial.ReadTimeout = 10;

		// cler input buffer from previous garbage
		serial.DiscardInBuffer();

		StartCoroutine(ReadSerialLoop());

	}


	public void OnApplicationQuit () {

		if (serial != null && serial.IsOpen) 
			serial.Close();

	}


	void Update() {

		/*if(serial.IsOpen && serial != null) {

			try {
				serialIn = serial.ReadLine();
			} catch(System.Exception) {

			}

		}*/

	}


	public IEnumerator ReadSerialLoop() {

		while(true) {
			
			try {
				while (serial.BytesToRead > 0) {

					string serialIn = serial.ReadExisting();

					// prepend pending buffer to received data and split by line
					string [] lines = (bufferIn + serialIn).Split ('\n');

					// If last line is not empty, it means the line is not complete (new line did not arrive yet), 
					// We keep it in buffer for next data.
					int nLines = lines.Length;
					bufferIn = lines[nLines - 1];

					// Loop until the penultimate line (don't use the last one: either it is empty or it has already been saved for later)
					for (int iLine = 0; iLine < nLines - 1; iLine++) {
						string line = lines[iLine];
						//print(line);

						// add line to lines array
						linesIn.Add(line);
					}
				}

				// avoid using all memory if nobody consumes lines
				int overflow = linesIn.Count - 1000;
				if (overflow > 0) {
					print ("Serial removing " + overflow + " unused lines");
					linesIn.RemoveRange(0, overflow);
				}

			} catch(System.Exception e) {
				print("System.Exception in serial.ReadLine: " + e.ToString());
			}
			
			yield return null;
		}
	}

	/// return all received lines and clear them
	/// Useful if you need to process all the received lines, even if there are several since last call
	public List<string> GetLines(bool keepLines = false) {

		List<string> lines = new List<string>(linesIn);

		if (!keepLines)
			linesIn.Clear();
		
		return lines;
	}
	
	/// return only the last received line and clear them all
	/// Useful when you need only the last received values and can ignore older ones
	public string GetLastLine(bool keepLines = false) {
		
		string line = "";
		if (linesIn.Count > 0)
			line = linesIn[linesIn.Count - 1];
		
		if (!keepLines)
			linesIn.Clear();
		
		return line;
	}
	

	public void Write(string message, bool overwriteCurrentValue=true) {



	}





	string GetPortName() {

		string[] portNames;

		switch (Application.platform) {

			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXDashboardPlayer:
			case RuntimePlatform.LinuxPlayer:

				portNames = System.IO.Ports.SerialPort.GetPortNames();
				
				if (portNames.Length ==0) {
				        portNames = System.IO.Directory.GetFiles("/dev/");                
				}
                     
				foreach (string portName in portNames) {                                
				        if (portName.StartsWith("/dev/tty.usb") || portName.StartsWith("/dev/ttyUSB")) return portName;
				}                
				return ""; 

			default: // Windows

				portNames = System.IO.Ports.SerialPort.GetPortNames();
				    
				if (portNames.Length > 0) return portNames[0];
				else return "COM3";

		}

	}

}
