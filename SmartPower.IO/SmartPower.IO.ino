#include <OneWire.h>
#include <DallasTemperature.h>

// --- Relay Configuration ---
const int relayPins[] = {3, 2, 5, 4};
const int numRelays = 4;

// --- Temperature ---
const int pinSensor1 = 7;
const int pinSensor2 = 8;

OneWire oneWire1(pinSensor1);
OneWire oneWire2(pinSensor2);
DallasTemperature sensor1(&oneWire1);
DallasTemperature sensor2(&oneWire2);

// --- Analog ---
const int analogPin1 = A1;
const int analogPin2 = A3;

// --- RMS Config ---
const int NUM_SAMPLES = 100;
const int SAMPLE_DELAY_US = 200; // 50Hz

float readRMS(int pin) {
  long sum = 0;
  long sumSq = 0;

  unsigned long startMicros = micros();

  for (int i = 0; i < NUM_SAMPLES; i++) {
    // Wait until exact time slot (Jitter-free)
    while (micros() - startMicros < (unsigned long)i * SAMPLE_DELAY_US);

    int v = analogRead(pin);
    sum += v;
    sumSq += (long)v * v;
  }

  float mean = (float)sum / NUM_SAMPLES;
  float meanSq = (float)sumSq / NUM_SAMPLES;

  // 1. Calculate the value inside the square root first
  float diff = meanSq - (mean * mean);

  // 2. Guard against negative results before calling sqrt
  return (diff > 0) ? sqrt(diff) : 0.0;
}

void setup() {
  Serial.begin(9600);

  for (int i = 0; i < numRelays; i++) {
    digitalWrite(relayPins[i], HIGH);
    pinMode(relayPins[i], OUTPUT);
  }

  sensor1.setWaitForConversion(false);
  sensor2.setWaitForConversion(false);

  sensor1.begin();
  sensor2.begin();

  sensor1.requestTemperatures();
  sensor2.requestTemperatures();
  delay(750);
}

void loop() {
  if (Serial.available()) {
    char cmd = Serial.read();

    // --- Relay ON ('0'–'3') ---
    if (cmd >= '0' && cmd <= '3') {
      digitalWrite(relayPins[cmd - '0'], LOW);
    }

    // --- Relay OFF ('4'–'7') ---
    else if (cmd >= '4' && cmd <= '7') {
      digitalWrite(relayPins[cmd - '4'], HIGH);
    }

    // --- Temp1 ---
    else if (cmd == '8') {
      sensor1.requestTemperatures();
      Serial.println(sensor1.getTempCByIndex(0));
    }

    // --- Temp2 ---
    else if (cmd == '9') {
      sensor2.requestTemperatures();
      Serial.println(sensor2.getTempCByIndex(0));
    }

    // --- RMS A1 ---
    else if (cmd == 'A') {
      Serial.println(readRMS(analogPin1));
    }

    // --- RMS A3 ---
    else if (cmd == 'B') {
      Serial.println(readRMS(analogPin2));
    }
  }
}