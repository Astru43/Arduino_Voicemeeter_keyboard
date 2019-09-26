int msg;
bool done = true;
void setup() {
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    msg = Serial.read();
    switch(msg) {
      case 255: 
        Serial.write(254);
        break;
      case 253:
        done = false;
        break;
    }
  }
  if (!done) {
    Serial.write(20);
    done = true;
    delay(5000);
    Serial.write(222);
    Serial.flush();
  }
}
