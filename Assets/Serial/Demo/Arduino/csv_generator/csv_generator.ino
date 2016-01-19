// Generate 4 values from 0 to 9999 and send them to the serial port
// Values are separated by a TAB and sets are separated by a new line 

String version = "1.3 / 2016-01-19 by Pierre Rossel";

#define LED 13

int frame = 0;

// See showHelp() for available modes doc
typedef enum {
  STOP, AUTO, ECHO
} 
Mode;

Mode mode = AUTO;

// Delay between each data sending in ms
int dataPeriod = 100; 

// When did we send the last data
unsigned long lastDataMs = 0;

// when did we last toggle the led
unsigned long lastLedMs = 0;

boolean ledValue = LOW;

// time updated on each update
unsigned long ms = 0;

// Buffer the command while receiving data on serial line
String cmd;

void setup() { 

  pinMode(LED, OUTPUT);

  // Turn led on while waiting
  setLed(HIGH);

  //Initialize serial and wait for port to open:
  Serial.begin(9600); 
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

  // turn led off after waiting
  setLed(LOW);

} 

// Extract a token from a string.
// Return the token or empty string if no more token
// tokenPos: 0 based position of the token to return
// delimiter: the char to delimit tokens
// exemples: 
// token("cmd param1 param2", ' ', 0) would return "cmd"
// token("cmd param1 param2", ' ', 1) would return "param1"
// token("cmd param1 param2", ' ', 2) would return "param2"
// token("cmd param1 param2", ' ', 3) would return ""
String token(String str, int tokenPos, char delimiter = ' ') {
  int posStart = 0;
  for (int iToken = 0; iToken <= tokenPos; iToken++) {

    // look for next delimiter
    int posDelim = str.indexOf(delimiter, posStart);
    if (posDelim == -1) {
      if (iToken == tokenPos)
        return str.substring(posStart);
      else
        return "";
    }
    else if (iToken == tokenPos)
      return str.substring(posStart, posDelim);

    // TODO: look for next token start (support multiple separators)
    posStart = posDelim + 1;
  }

  return "";
}

void loop() {

  // update time
  ms = millis();

  // Read serial for commands
  char c;
  while (Serial.available()) {
    c = Serial.read();

    if (mode == ECHO) {
      Serial.write(c);
    }

    if (c == '\n') {

      // try to interpret command
      String action = token(cmd, 0);

      if (action == "stop") {
        mode = STOP;
      }
      else if (action == "auto") {
        mode = AUTO;
        String period = token(cmd, 1);
        if (period.length() > 0)
          dataPeriod = period.toInt();
      }
      else if (action == "echo")
        mode = ECHO;
      else if (action == "reset")
        frame = 0;
      else if (action == "version")
        Serial.println(version);
      else if (action == "help") {
        showHelp();
      }
      else {
        if (mode != ECHO) {
          Serial.println("Unknown command"); 
        }
      }

      cmd = "";
      break;
    }

    else {
      cmd += c;
    }
  }

  if (mode == AUTO && ms > lastDataMs + dataPeriod) {

    lastDataMs = ms;

    float sineFreq = 0.2;

    Serial.print(frame);
    Serial.print("\t");

    Serial.print((int)(9999 * (sin(TWO_PI * sineFreq * ms / 1000) / 2.0) ));
    //Serial.print(1000 + frame);
    Serial.print("\t");

    //Serial.print(random(9999));
    Serial.print(2000 + frame);
    Serial.print("\t");

    //Serial.print(random(9999));
    Serial.print(3000 + frame);
    Serial.println();

    frame = (frame + 1) % 9999;

    // Blink led (fast during 2 sec after reset, for each data afterwards)
    if (ms > 2000) {
      setLed(1);
      //toggleLed();
    }

  }

  // Blink led (5 times during 2 seconds after reset, for each data afterwards)
  if (ms < 2000) {
    if (ms > lastLedMs + 200) {
      toggleLed();
    }
  }
  else if (ledValue && ms > lastLedMs + 10) {
    // turn off led shortly after sending data
    setLed(0);
  }

  //delay(500);
  //delay(10);

}

void setLed(boolean val) {
  digitalWrite(LED, val);
  ledValue = val;
  lastLedMs = ms;
}

void toggleLed() {
  ledValue = !ledValue;
  lastLedMs = ms;
  digitalWrite(LED, ledValue);
}

void showHelp() {
  Serial.println();
  Serial.println("CSV Generator " + version);
  Serial.println();
  Serial.println(F("Available commands:"));
  Serial.println(F("auto [period]"));
  Serial.println(F("    Sends continuously 4 int values separated by \\t and terminated by \\n."));
  Serial.println(F("    This is the default mode."));
  Serial.println(F("    period (optional): delay [ms] between each line sending."));
  Serial.println(F("stop"));
  Serial.println(F("    Stop sending data or echo mode."));
  Serial.println(F("echo"));
  Serial.println(F("    Sends back whatever it receives on its serial port."));
  Serial.println(F("reset"));
  Serial.println(F("    Resets the frame count (the first value in auto mode)."));
  Serial.println(F("version"));
  Serial.println(F("    Sends version information."));
  Serial.println(F("help"));
  Serial.println(F("    Shows help."));
  Serial.println(F(""));
}




