/*
 * SerialConfig component
 * 
 * See configuration section in Serial script for information on how to use this script.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialConfig : MonoBehaviour {

	public string[] portNames = {"/dev/tty.usb", "/dev/ttyUSB", "/dev/cu.usb", "/dev/cuUSB"};

	public int speed = 9600;

	/// <summary>
	/// Log some debug informations to the console to help find what's wrong when needed.
	/// </summary>
	public bool logDebugInfos = false;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
