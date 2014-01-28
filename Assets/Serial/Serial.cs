/* 
 * This behavior helps sending and receiving data from a serial port. 
 * It detects line breaks and notifies the attached gameObject of new lines as they arrive.
 * 
 * Usage 1: (when you expect line breaks)
 * -------
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
 * -------
 * 
 * - drop this script to a gameObject
 * - from any script, use the static props ReceivedBytesCount, ReceivedBytes 
 *   and don't forget to call ClearReceivedBytes() to avoid overflowing the buffer
 * 
 * Troubleshooting
 * ---------------
 * 
 * You may get the following error:
 *     error CS0234: The type or namespace name `Ports' does not exist in the namespace `System.IO'. 
 *     Are you missing an assembly reference?
 * Solution: 
 *     File | Build Settings | Optimization | API Compatibility Level: .Net 2.0
 * 
 * Author: Pierre Rossel, 2014-01-28
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
	/// Enable notification of data as it arrives
	/// Sends OnSerialData(string data) message
	/// </summary>
	public bool NotifyData = false;

	/// <summary>
	/// Enable line detection and notification on received data.
	/// Message OnSerialLine(string line) is sent for every received line
	/// </summary>
	public bool NotifyLines = false;

	/// <summary>
	/// Maximum number of lines to remember. Get them with GetLines() or GetLastLine()
	/// </summary>
	public int RememberLines = 0;

	/// <summary>
	/// Enable lines detection, values separation and notification.
	/// Each line is split with the value separator (TAB by default)
	/// Sends Message OnSerialValues(string [] values)
	/// </summary>
	public bool NotifyValues = false;

	/// <summary>
	/// The values separator.
	/// </summary>
	public char ValuesSeparator = '\t';

	//string serialOut = "";
	private List<string> linesIn = new List<string> ();

	/// <summary>
	/// Gets the received bytes count.
	/// </summary>
	/// <value>The received bytes count.</value>
	public int ReceivedBytesCount { get { return BufferIn.Length; } }

	/// <summary>
	/// Gets the received bytes.
	/// </summary>
	/// <value>The received bytes.</value>
	public string ReceivedBytes { get { return BufferIn; } }

	/// <summary>
	/// Clears the received bytes. 
	/// Warning: This prevents line detection and notification. 
	/// To be used when no \n is expected to avoid keeping unnecessary big amount of data in memory
	/// You should normally not call this function if \n are expected.
	/// </summary>
	public void ClearReceivedBytes ()
	{
		BufferIn = "";
	}

	/// <summary>
	/// Gets the lines count.
	/// </summary>
	/// <value>The lines count.</value>
	public int linesCount { get { return linesIn.Count; } }

	#region Private vars

	// buffer data as they arrive, until a new line is received
	private string BufferIn = "";

	#endregion

	#region Static vars

	// Only one serial port shared among all instances and living after all instances have been destroyed
	private static SerialPort s_serial;

	// 
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
		if (RememberLines < 0)
			RememberLines = 0;
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

					// Dispatch new data to each instance
					foreach (Serial inst in s_instances) {
						inst.receivedData (serialIn);
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

	// Data has been received, do what this instance has to do with it
	protected void receivedData (string data)
	{

		if (NotifyData) {
			SendMessage ("OnSerialData", data);
		}

		// Detect lines
		if (NotifyLines || NotifyValues) {
		
			// prepend pending buffer to received data and split by line
			string [] lines = (BufferIn + data).Split ('\n');
			
			// If last line is not empty, it means the line is not complete (new line did not arrive yet), 
			// We keep it in buffer for next data.
			int nLines = lines.Length;
			BufferIn = lines [nLines - 1];
			
			// Loop until the penultimate line (don't use the last one: either it is empty or it has already been saved for later)
			for (int iLine = 0; iLine < nLines - 1; iLine++) {
				string line = lines [iLine];
				//print(line);
				
				// Buffer line
				if (RememberLines > 0) {
					linesIn.Add (line);
						
					// trim lines buffer
					int overflow = linesIn.Count - RememberLines;
					if (overflow > 0) {
						print ("Serial removing " + overflow + " lines from lines buffer. Either consume lines before they are lost or set RememberLines to 0.");
						linesIn.RemoveRange (0, overflow);
					}
				}
					
				// notify new line
				if (NotifyLines) {
					SendMessage ("OnSerialLine", line);
				}

				// Notify values
				if (NotifyValues) {
					string [] values = line.Split (ValuesSeparator);
					SendMessage ("OnSerialValues", values);
				}

			}
		}
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
