/* This behavior helps sending and receiving data from a serial port. 
 * It detects line breaks and notifies the attached gameObject of new lines as they arrive.
 * 
 * Usage 1: (when you expect line breaks)
 * 
 * - drop this script to a gameObject
 * - create a script on the same gameObject to receive new line notifications
 * - add the OnSerialLine() function, here is an example
 * 
 * 	void OnSerialLine(string line) {
 *		print "Got a line: " + line;
 *	}
 * 
 * Usage 2: (when you don't expect line breaks)
 * 
 * - drop this script to a gameObject
 * - from any script, use the static props ReceivedBytesCount, ReceivedBytes 
 *   and don't forget to call ClearReceivedBytes() to avoid overflowing the buffer
 * 
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO.Ports;

// System.IO.Ports requires a working Serial Port. On Mac, you will need to purcase the Uniduino plug-in on the Unity Store
// This adds a folder + a file into your local folder at ~/lib/libMonoPosixHelper.dylib
// This file will activate your serial port for C# / .NET
// The functions are the same as the standard C# SerialPort library
// cf. http://msdn.microsoft.com/en-us/library/system.io.ports.serialport(v=vs.110).aspx


public class Serial : MonoBehaviour
{

	/// <summary>
	/// Number of lines to buffer. Get them with GetLines() or GetLastLine()
	/// </summary>
	public int bufferLines = 0;

	//string serialOut = "";
	private List<string> linesIn = new List<string> ();

	/// <summary>
	/// Gets the received bytes count.
	/// </summary>
	/// <value>The received bytes count.</value>
	static public int ReceivedBytesCount { get { return s_bufferIn.Length; } }

	/// <summary>
	/// Gets the received bytes.
	/// </summary>
	/// <value>The received bytes.</value>
	static public string ReceivedBytes { get { return s_bufferIn; } }

	/// <summary>
	/// Clears the received bytes. 
	/// Warning: This prevents line detection and notification. 
	/// To be used when no \n is expected to avoid keeping unnecessary big amount of data in memory
	/// You should normally not call this function if \n are expected.
	/// </summary>
	static public void ClearReceivedBytes ()
	{
		s_bufferIn = "";
	}

	/// <summary>
	/// Gets the lines count.
	/// </summary>
	/// <value>The lines count.</value>
	public int linesCount { get { return linesIn.Count; } }

	#region Static vars
	// Only one serial port shared among all instances and living after all instances have been destroyed
	private static SerialPort s_serial;
	
	// buffer data as they arrive, until a new line is received
	private static string s_bufferIn = "";
	private static List<Serial> s_instances = new List<Serial> ();
	#endregion

	void Start ()
	{

		if (s_serial == null) {

			string portName = GetPortName ();

			if (portName == "") {
				print ("Error: Couldn't find serial port.");
				return;
			} else {
				//print("Opening serial port: " + portName);
			}

			s_serial = new SerialPort (portName, 9600);

			s_serial.Open ();
			//print ("default ReadTimeout: " + serial.ReadTimeout);
			//serial.ReadTimeout = 10;

			// cler input buffer from previous garbage
			s_serial.DiscardInBuffer ();

		}

		// Each instance has its own coroutine but only one will be active a 
		StartCoroutine (ReadSerialLoop ());
	}

	void OnValidate ()
	{
		if (bufferLines < 0)
			bufferLines = 0;
	}

	void OnEnable ()
	{
		s_instances.Add (this);
	}

	void OnDisable ()
	{
		s_instances.Remove (this);
	}

	public void OnApplicationQuit ()
	{

		if (s_serial != null) {
			if (s_serial.IsOpen) {
				print ("closing serial port");
				s_serial.Close ();
			}

			s_serial = null;
		}

	}

	void Update ()
	{

		/*if(serial.IsOpen && serial != null) {

			try {
				s_bufferIn = serial.ReadLine();
			} catch(System.Exception) {

			}

		}*/

	}

	public IEnumerator ReadSerialLoop ()
	{

		while (true) {
			
			try {
				while (s_serial.BytesToRead > 0) {  // BytesToRead crashes on Windows -> use ReadLine in a Thread

					string serialIn = s_serial.ReadExisting ();

					// prepend pending buffer to received data and split by line
					string [] lines = (s_bufferIn + serialIn).Split ('\n');

					// If last line is not empty, it means the line is not complete (new line did not arrive yet), 
					// We keep it in buffer for next data.
					int nLines = lines.Length;
					s_bufferIn = lines [nLines - 1];

					// Loop until the penultimate line (don't use the last one: either it is empty or it has already been saved for later)
					for (int iLine = 0; iLine < nLines - 1; iLine++) {
						string line = lines [iLine];
						//print(line);

						// Send new line to all instances
						foreach (Serial inst in s_instances) {

							// Buffer line
							if (inst.bufferLines > 0) {
								inst.linesIn.Add (line);

								// trim lines buffer
								int overflow = inst.linesIn.Count - inst.bufferLines;
								if (overflow > 0) {
									print ("Serial removing " + overflow + " lines from lines buffer");
									inst.linesIn.RemoveRange (0, overflow);
								}
							}

							// notify new line
							inst.SendMessage ("OnSerialLine", line, SendMessageOptions.DontRequireReceiver);
						}
					}
				}


			} catch (System.Exception e) {
				print ("System.Exception in serial.ReadLine: " + e.ToString ());
			}
			
			yield return null;
		}
	}

	/// return all received lines and clear them
	/// Useful if you need to process all the received lines, even if there are several since last call
	public List<string> GetLines (bool keepLines = false)
	{

		List<string> lines = new List<string> (linesIn);

		if (!keepLines)
			linesIn.Clear ();
		
		return lines;
	}
	
	/// return only the last received line and clear them all
	/// Useful when you need only the last received values and can ignore older ones
	public string GetLastLine (bool keepLines = false)
	{
		
		string line = "";
		if (linesIn.Count > 0)
			line = linesIn [linesIn.Count - 1];
		
		if (!keepLines)
			linesIn.Clear ();
		
		return line;
	}

	public void Write (string message, bool overwriteCurrentValue=true)
	{



	}

	string GetPortName ()
	{

		string[] portNames;

		switch (Application.platform) {

		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXDashboardPlayer:
		case RuntimePlatform.LinuxPlayer:

			portNames = System.IO.Ports.SerialPort.GetPortNames ();
				
			if (portNames.Length == 0) {
				portNames = System.IO.Directory.GetFiles ("/dev/");                
			}
                     
			foreach (string portName in portNames) {                                
				if (portName.StartsWith ("/dev/tty.usb") || portName.StartsWith ("/dev/ttyUSB"))
					return portName;
			}                
			return ""; 

		default: // Windows

			portNames = System.IO.Ports.SerialPort.GetPortNames ();
				    
			if (portNames.Length > 0)
				return portNames [0];
			else
				return "COM3";

		}

	}

}
