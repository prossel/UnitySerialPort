/* 
 * Version 0.3.3, 2016-03-17, Pierre Rossel
 * 
 * This behavior helps sending and receiving data from a serial port. 
 * It detects line breaks and notifies the attached gameObject of new lines as they arrive.
 * 
 * Usage 1: Receive data when you expect line breaks
 * -------
 * 
 * - drop this script to a gameObject
 * - create a script on the same gameObject to receive new line notifications
 * - select "Notify Lines" checkbox on Serial script
 * - add the OnSerialLine() function, here is an example
 * 
 * 	void OnSerialLine(string line) {
 *		print "Got a line: " + line;
 *	}
 * 
 * Usage 2: Receive data (when you don't expect line breaks)
 * -------
 * 
 * - drop this script to a gameObject
 * - from any script, use the static props ReceivedBytesCount, ReceivedBytes 
 *   and don't forget to call ClearReceivedBytes() to avoid overflowing the buffer
 * 
 * Usage 3: Send data
 * -------
 * 
 * - from any script, call the static functions Serial.Write() or Serial.WriteLn()
 * - if not not already, the serial port will be opened automatically.
 * 
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
	/// Discard all received data until first line.
	/// Do not enable if you do not expect a \n as 
	/// this would prevent the notification of any line or value.
	/// Data notification is not impacted by this parameter.
	/// </summary>
	public bool SkipFirstLine = false;

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

	/// <summary>
	/// The enable debug infos.
	/// The first script with debug infos enabled will enable them until the program stop. 
	/// Therefore, only one script need to enable the debug info to have them in all app, even with multiple scene
	/// and multiple instances, until the program stops.
	/// </summary>
	public bool EnableDebugInfos = false;

	/// <summary>
	/// The first line has been received.
	/// </summary>
	bool FirstLineReceived = false;

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

	// flag to detect whether coroutine is still running to workaround coroutine being stopped after saving scripts while running in Unity
	private int nCoroutineRunning = 0;
	#endregion

	#region Static vars

	// Only one serial port shared among all instances and living after all instances have been destroyed
	private static SerialPort s_serial;

	// 
	private static List<Serial> s_instances = new List<Serial> ();

	// Enable debug info
	private static bool s_debug = false; // Do not change here. Use EnableDebugInfo on any script instance

	private static float s_lastDataIn = 0;
	private static float s_lastDataCheck = 0;

	#endregion

	void Start ()
	{
		// print ("Serial Start ");
	}

	void OnValidate ()
	{
		if (RememberLines < 0)
			RememberLines = 0;
	}

	void OnEnable ()
	{
		//		print("Serial OnEnable");
		//		if (s_serial != null)
		//			print ("serial IsOpen: " + s_serial.IsOpen);
		//		else
		//			print ("no serial: ");

		s_instances.Add (this);

		if (EnableDebugInfos && !s_debug) {
			Debug.LogWarning("Serial debug informations enabled by " + this);
			s_debug = true;
		}

		checkOpen (9600);

	}

	void OnDisable ()
	{
		//print("Serial OnDisable");
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
		//print ("Serial Update");

		if (s_serial != null && s_serial.IsOpen) {
			if (nCoroutineRunning == 0) {

				//print ("starting ReadSerialLoop coroutine");

				switch (Application.platform) {

				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsWebPlayer:

					s_serial.ReadTimeout = 1;

					// Each instance has its own coroutine but only one will be active
					StartCoroutine (ReadSerialLoopWin ());
					break;

				default:
					// Each instance has its own coroutine but only one will be active
					StartCoroutine (ReadSerialLoop ());
					break;

				}
			} else {
				if (nCoroutineRunning > 1)
					print (nCoroutineRunning + " coroutines in " + name);

				nCoroutineRunning = 0; 
			}
		}
	}

	public IEnumerator ReadSerialLoop ()
	{

		while (true) {

			if (!enabled) {
				//print ("behaviour not enabled, stopping coroutine");
				yield break; 
			}

			//print("ReadSerialLoop ");
			nCoroutineRunning++; 

			try {
				s_lastDataCheck = Time.time;
				while (s_serial.BytesToRead > 0) {  // BytesToRead crashes on Windows -> use ReadLine or ReadByte in a Thread or Coroutine


					string serialIn = s_serial.ReadExisting ();

					// Dispatch new data to each instance
					foreach (Serial inst in s_instances) {
						inst.receivedData (serialIn);
					}

					s_lastDataIn = s_lastDataCheck;
				}

			} catch (System.Exception e) {
				print ("System.Exception in serial.ReadExisting: " + e.ToString ());
			}

			yield return null;
		}

	}

	public IEnumerator ReadSerialLoopWin ()
	{

		while (true) {

			if (!enabled) {
				//print ("behaviour not enabled, stopping coroutine");
				yield break; 
			}

			//print("ReadSerialLoopWin ");
			nCoroutineRunning++; 
			//print ("nCoroutineRunning: " + nCoroutineRunning);

			string serialIn = "";
			try {
				while (true) {  // BytesToRead crashes on Windows -> use ReadLine or ReadByte in a Thread or Coroutine
					char c = (char)s_serial.ReadByte();
					serialIn += c;

					//serialIn += s_serial.ReadLine();
				}

			} catch (System.TimeoutException) {
				//print ("System.TimeoutException in serial.ReadLine: " + te.ToString ());
			} catch (System.Exception e) {
				print ("System.Exception in serial.ReadLine: " + e.ToString ());
			}

			if (serialIn.Length > 0) {

				//Debug.Log("just read some data: " + serialIn);
				// Dispatch new data to each instance
				foreach (Serial inst in s_instances) {
					inst.receivedData (serialIn);
				}
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

	/// <summary>
	/// Send data to the serial port.
	/// </summary>
	public static void Write (string message)
	{
		if (checkOpen ())
			s_serial.Write (message);
	}

	/// <summary>
	/// Send data to the serial port and append a new line character (\n)
	/// </summary>
	public static void WriteLn (string message = "")
	{
		s_serial.Write (message + "\n");
	}

	/// <summary>
	/// Act as if the serial port has received data followed by a new line.
	/// </summary>
	public void SimulateDataReceptionLn(float data) {
		foreach (Serial inst in s_instances) {
			inst.receivedData(data + "\n");
		}
	}

	/// <summary>
	/// Act as if the serial port has received data followed by a new line.
	/// </summary>
	public void SimulateDataReceptionLn(string data) {
		foreach (Serial inst in s_instances) {
			inst.receivedData(data + "\n");
		}
	}

	/// <summary>
	/// Verify if the serial port is opened and opens it if necessary
	/// </summary>
	/// <returns><c>true</c>, if port is opened, <c>false</c> otherwise.</returns>
	/// <param name="portSpeed">Port speed.</param>
	public static bool checkOpen (int portSpeed = 9600)
	{

		if (s_serial == null) {

			string portName = GetPortName ();

			if (portName == "") {
				print ("Error: Couldn't find serial port.");
				return false;
			} else {
				if (s_debug)
					print("Opening serial port: " + portName);
			}

			s_serial = new SerialPort (portName, portSpeed);

			s_serial.Open ();
			//print ("default ReadTimeout: " + s_serial.ReadTimeout);
			//s_serial.ReadTimeout = 10;

			// clear input buffer from previous garbage
			s_serial.DiscardInBuffer ();
		}

		return s_serial.IsOpen;
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
				//Debug.Log ("Received a line: " + line);

				// skip first line 
				if (!FirstLineReceived) {
					FirstLineReceived = true;

					if (SkipFirstLine) {
						if (EnableDebugInfos) {
							Debug.Log("First line skipped: " + line);
						}
						continue;
					}
				}

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

	static string GetPortName ()
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

			// Defaults to last port in list (most chance to be an Arduino port)
			if (portNames.Length > 0)
				return portNames [portNames.Length - 1];
			else
				return "";
		}
	}

	void OnGUI() {

		// Show debug only if enabled and by the first instance to avoid overwrite same data
		if (s_debug && this == s_instances[0]) {
			GUILayout.Label("Serial last data: " + s_lastDataIn + " (last check: " + s_lastDataCheck + ")");
		}
	}

}
