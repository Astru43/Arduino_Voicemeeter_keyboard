int pins[] = { 3,4,10,11 };
volatile byte buffer[2];
volatile byte lastPress = 0, lastState = 0, state = 1;

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
	for (int i = 0; i < 2; i++)
		pinMode(pins[i], OUTPUT);
	for (int i = 2; i < 4; i++)
		pinMode(pins[i], INPUT_PULLUP);
	Serial.begin(250000);
}

ISR(TIMER2_COMPA_vect) {
	if (state == 1) {
		PORTD = 0x8;
	} else if (state == 2){
		PORTD = 0x10;
	}
	switch (PINB) {
	case 8:
		if (state == 1) { 
			if (lastPress == 0) {
				buffer[0] = (0x02 | 0x20);
				lastPress = 0x02;
				lastState = state;
			}
		}
		else {
			if (lastPress == 0) {
				buffer[0] = (0x01 | 0x20);
				lastPress = 0x01;
				lastState = state;
			}
		}
		break;
	case 4:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x04 | 0x20);
				lastPress = 0x04;
				lastState = state;
			}
		}
		else {
			if (lastPress == 0) {
				buffer[0] = (0x03 | 0x20);
				lastPress = 0x03;
				lastState = state;
			}
		}
		break;
	default:
		if (lastState == state && lastPress != 0) {
			buffer[0] = lastPress;
			lastPress = 0;
		}
	}

	state++;
	if (state > 2) state = 1;
}

void loop() {
	// put your main code here, to run repeatedly:

	while (Serial.available() > 0) {
		int val = Serial.read();
		switch (val) {
		case 0x81:
			Serial.write(0x82);
			break;
		}
	}
	if (buffer[0] > 0) {
		Serial.write(buffer[0]);
		if (buffer[1] > 0) Serial.write(buffer[1]);
			Serial.flush();
	}
	buffer[0] = 0;
	buffer[1] = 0;
	delay(1);

}