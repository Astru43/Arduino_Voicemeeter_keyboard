int pins[] = {3,4,10,11};
volatile bool state = true;
volatile int buffer;
bool open = false;

void setup() {
  // put your setup code here, to run once:
  cli();
  TCCR2A = 0;
  TCCR2B = 0;
  TCNT2 = 0;
  OCR2A = 124;
  TIMSK2 = 0;
  
  TCCR2A |= (1 << WGM21);
  TCCR2B |= (1 << CS22);
  TIMSK2 |= (1 << OCIE2A);

  sei();
  for(int i = 0;i<2;i++)
  pinMode(pins[i], OUTPUT);
  for(int i = 2;i<4;i++)
  pinMode(pins[i], INPUT_PULLUP);
  Serial.begin(250000);
}

ISR(TIMER2_COMPA_vect) {
  if(state) {
    PORTD = 0x8;
  } else {
    PORTD = 0x10;
  }
  switch(PINB) {
    case 12:
      break;
    case 8:
      if (state) buffer = 0xF2;
      else buffer = 0xF1;
      break;
    case 4:
      if (state) buffer = 0xF4;
      else buffer = 0xF3;
      break;
  }

  
  state = !state;
}

void loop() {
  // put your main code here, to run repeatedly:
  
  while(Serial.available() > 0) {
    int val = Serial.read();
    switch (val) {
      case 255:
        Serial.write(254);
        break;
      case 253:
        open = true;
        break;
      case 252:
        open = false;
        break;
      default:
        Serial.write(val);
    }
  }
  if (open) {
    Serial.write(buffer);
    Serial.flush();
  } 
  buffer = 0;
  delay(1);
  
}
