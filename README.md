# What is this ?

This is a script to make easy to use serial port in [Unity](http://unity3d.com/). 
Especially parsing lines and values when data is [TSV](http://fr.wikipedia.org/wiki/Format_TSV) formatted.

A demo is provided with a sample Arduino sketch to generate sample serial data. 

The script component helps sending and receiving data from a serial port.
It detects line breaks and notifies the attached gameObject of new lines as they arrive.

# Install

Download the latest [package](https://github.com/prossel/UnitySerialPort/raw/master/UnitySerialPort.unitypackage) and import in your project.

You do not need to clone or download the entire project unless you want to modify it (and then make a pull request).

# How to use

See the demo in Assets/Serial/Demo/Serial Demo.unity

## Usage 1: Receive data when you expect line breaks

- drop this script to a gameObject
- check the NotifyLines parameter
- create a script on the same gameObject to receive new line notifications
- add the `OnSerialLine()` function, here is an example

```c#
void OnSerialLine(string line) {
  Debug.Log("Got a line: " + line);
}
```

## Usage 2: Receive data (when you don't expect line breaks)

- drop this script to a gameObject
- from any script, use the static props `Serial.ReceivedBytesCount`, `Serial.ReceivedBytes`
and don't forget to call `ClearReceivedBytes()` to avoid overflowing the buffer

## Usage 3: Send data

- from any script, call the static functions `Serial.write()` or `Serial.writeLn()`
- if not not already, the serial port will be opened automatically.

## Configuration
Drop the SerialConfig component to an empty GameObject in your scene and configure:

- the preferred ports
- the speed
- whether you wand debug informations in console
 
# Troubleshooting

## Error CS0234 Ports does not exist in the namespace

You may get the following error:

> error CS0234: The type or namespace name \`Ports' does not exist in the namespace \`System.IO'.
> Are you missing an assembly reference?

Solution:

First make sure the correct platform is selected in File | Build Settings. It should be "PC, Mac & Linux standalone". Other platforms are not supported. In some circonstances, the setting switches back to another platform.

Then go to Edit | Project Settings | Player | PC, Mac & Linux Standalone settings | Other Settings |  Optimization | API Compatibility Level and select ".Net 2.0". The other option does not contain the Ports namespace.

In some older version of Unity, you would find this option in:
File | Build Settings | Optimization | API Compatibility Level: .Net 2.0
