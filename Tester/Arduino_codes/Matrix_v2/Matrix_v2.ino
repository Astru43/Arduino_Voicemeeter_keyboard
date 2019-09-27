int pins[] = { 2, 3, 4, 5, 6, 7, 10, 11 };
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
	for (int i = 0; i < 6; i++)
		pinMode(pins[i], INPUT_PULLUP);
	for (int i = 6; i < 8; i++)
		pinMode(pins[i], OUTPUT);
	Serial.begin(250000);
}

ISR(TIMER2_COMPA_vect) {
	if (state == 1) {
		PORTB = 0x8;
	} else if (state == 2) {
		PORTB = 0x4;
	}
	switch (PIND) {
	case 0xfb:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x07 | 0x20);
				lastPress = 0x07;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x01 | 0x20);
				lastPress = 0x01;
				lastState = state;
			}
		}
		break;
	case 0xf7:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x08 | 0x20);
				lastPress = 0x08;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x02 | 0x20);
				lastPress = 0x02;
				lastState = state;
			}
		}
		break;
	case 0xef:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x09 | 0x20);
				lastPress = 0x09;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x03 | 0x20);
				lastPress = 0x03;
				lastState = state;
			}
		}
		break;
	case 0xdf:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x0a | 0x20);
				lastPress = 0x0a;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x04 | 0x20);
				lastPress = 0x04;
				lastState = state;
			}
		}
		break;
	case 0xbf:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x0b | 0x20);
				lastPress = 0x0b;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x05 | 0x20);
				lastPress = 0x05;
				lastState = state;
			}
		}
		break;
	case 0x7f:
		if (state == 1) {
			if (lastPress == 0) {
				buffer[0] = (0x0c | 0x20);
				lastPress = 0x0c;
				lastState = state;
			}
		} else {
			if (lastPress == 0) {
				buffer[0] = (0x06 | 0x20);
				lastPress = 0x06;
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