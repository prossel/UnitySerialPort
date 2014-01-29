// Generate 4 values from 0 to 9999 and send them to the serial port
// Values are separated by a TAB and sets are separated by a new line 

String version = "1.1 / 2014-01-29 by Pierre Rossel";

int frame = 0;

typedef enum {
  AUTO, ECHO
} 
Mode;

Mode mode = AUTO;

String cmd;

void setup() { 

  //Initialize serial and wait for port to open:
  Serial.begin(9600); 
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

} 

void loop() {

  // Read serial for commands
  char c;
  while (Serial.available()) {
    c = Serial.read();

    if (mode == ECHO) {
      Serial.write(c);
    }

    switch (c) {

    case '\n': 
      // try to interpret command
      if (cmd == "auto")
        mode = AUTO;
      else if (cmd == "echo")
        mode = ECHO;
      else if (cmd == "reset")
        frame = 0;
      else if (cmd == "help") {
        showHelp();
      }

      cmd = "";
      break;

    default:
      cmd += c;

    }

  }

  if (mode == AUTO) {
    unsigned long ms = millis();
    float freq = 0.2;

    Serial.print(frame);
    Serial.print("\t");

    Serial.print((int)(9999 * (sin(TWO_PI * freq * ms / 1000) / 2.0) ));
    //Serial.print(1000 + frame);
    Serial.print("\t");

    //Serial.print(random(9999));
    Serial.print(2000 + frame);
    Serial.print("\t");

    //Serial.print(random(9999));
    Serial.print(3000 + frame);
    Serial.println();
  }

  //delay(500);
  delay(10);

  frame = (frame + 1) % 9999;
}

void showHelp() {
  Serial.println("");
  Serial.println("CSV Generator " + version);
  Serial.println("Available commands:");
  Serial.println("auto");
  Serial.println("    Sends continuously 4 int values separated by \\t and terminated by \\n.");
  Serial.println("    This is the default mode.");
  Serial.println("echo");
  Serial.println("    Sends back whatever it receives on its serial port.");
  Serial.println("reset");
  Serial.println("    Resets the frame count (the first value in auto mode).");
  Serial.println("help");
  Serial.println("    Shows help.");
  Serial.println("");

}




