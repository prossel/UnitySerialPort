// Generate 4 values from 0 to 9999 and send them to the serial port
// Values are separated by a TAB and sets are separated by a new line 

int frame = 0;

void setup() { 

  //Initialize serial and wait for port to open:
  Serial.begin(9600); 
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

} 

void loop() {
  
  
  unsigned long ms = millis();
  float f = 0.2;
  
  Serial.print(frame);
  Serial.print("\t");
  
  Serial.print((int)(9999 * (sin(TWO_PI * f * ms / 1000) / 2.0) ));
  //Serial.print(1000 + frame);
  Serial.print("\t");
  
  //Serial.print(random(9999));
  Serial.print(2000 + frame);
  Serial.print("\t");
  
  //Serial.print(random(9999));
  Serial.print(3000 + frame);
  Serial.println();
  
  //delay(500);
  delay(10);

  frame = (frame + 1) % 9999;
}

