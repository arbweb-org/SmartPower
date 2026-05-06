#include <Arduino.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include "refrigerator.h"

const int relayPins[] = {2, 3, 4, 5}; // Compressor, Fan, Defrost, Light
const int pinSensor1 = 7, pinSensor2 = 8;
const int analogPin1 = A1, analogPin2 = A3;
const int NUM_SAMPLES = 100, SAMPLE_DELAY_US = 200;

OneWire oneWire1(pinSensor1), oneWire2(pinSensor2);
DallasTemperature sensor1(&oneWire1), sensor2(&oneWire2);
Refrigerator fridge;

float readRMS(int pin) {
    long sum = 0, sumSq = 0;
    unsigned long startMicros = micros();
    for (int i = 0; i < NUM_SAMPLES; i++) {
        while (micros() - startMicros < (unsigned long)i * SAMPLE_DELAY_US);
        int v = analogRead(pin);
        sum += v;
        sumSq += (long)v * v;
    }
    float mean = (float)sum / NUM_SAMPLES;
    float diff = ((float)sumSq / NUM_SAMPLES) - (mean * mean);
    return (diff > 0) ? sqrt(diff) : 0.0;
}

void setup() {
  randomSeed(analogRead(0));
  Serial.begin(9600);
  for (int i = 0; i < 4; i++)
  {
    digitalWrite(relayPins[i], HIGH);
    pinMode(relayPins[i], OUTPUT);
  }

  sensor1.begin();
  sensor2.begin();
  sensor1.requestTemperatures();
  sensor2.requestTemperatures();

  sensor1.setWaitForConversion(false);
  sensor2.setWaitForConversion(false);
}

void loopSerial() {
  if (!Serial.available()) return;
  char cmd = Serial.read();
  // Handshake
  if (cmd == 'X') Serial.println("OK");
  // Control relays
  else if (cmd >= '0' && cmd <= '3') { digitalWrite(relayPins[cmd - '0'], LOW); Serial.println("OK"); }
  else if (cmd >= '4' && cmd <= '7') { digitalWrite(relayPins[cmd - '4'], HIGH); Serial.println("OK"); }
  // Read sensors
  else if (cmd == '8') { sensor1.requestTemperatures(); Serial.println(sensor1.getTempCByIndex(0)); }
  else if (cmd == '9') { sensor2.requestTemperatures(); Serial.println(sensor2.getTempCByIndex(0)); }
  else if (cmd == 'A') { Serial.println(readRMS(analogPin1)); }
  else if (cmd == 'B') { Serial.println(readRMS(analogPin2)); }
  // Set parameters
  else if (cmd == 'C') { }
  else if (cmd == 'D') { }
  else if (cmd == 'E') { }  
  else if (cmd == 'F') { }
  else if (cmd == 'G') { }
  else if (cmd == 'H') { }
  // Set sensors calibration factors
  else if (cmd == 'U') { }
  else if (cmd == 'V') { }
}

unsigned long lastLoopTime = 0;
void loopFridge() {
  if (millis() - lastLoopTime < 1000) // Limit to 1 loops per second{
  {
    return;
  }

  sensor1.requestTemperatures();
  sensor2.requestTemperatures();
    
  fridge.loop(sensor1.getTempCByIndex(0), sensor2.getTempCByIndex(0));

  digitalWrite(relayPins[0], fridge.CompressorOn ? LOW : HIGH);
  digitalWrite(relayPins[2], fridge.DefrostOn ? LOW : HIGH);

  lastLoopTime = millis();
}

void loop() {
  loopSerial();
  loopFridge();
}