#include <Arduino.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include "refrigerator.h"
#include "Multimeter.h"

const int relayPins[] = {2, 3, 4, 5}; // Compressor, Fan, Defrost, Light
const int pinSensor1 = 7, pinSensor2 = 8;

OneWire oneWire1(pinSensor1), oneWire2(pinSensor2);
DallasTemperature sensor1(&oneWire1), sensor2(&oneWire2);
Refrigerator fridge;
Multimeter multimeter;

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

void sendOK() {
  Serial.println("OK");
}

void loopSerial() {
  if (!Serial.available()) return;
  char cmd = Serial.read();

  // Handshake
  // Returns "OK" to confirm the device is responsive
  if (cmd == 'X') Serial.println("OK");

  // Control relays (For testing, '0'-'3' turn on, '4'-'7' turn off)
  // Returns "OK" after setting the relay state
  else if (cmd >= '0' && cmd <= '3') { digitalWrite(relayPins[cmd - '0'], LOW); sendOK(); }
  else if (cmd >= '4' && cmd <= '7') { digitalWrite(relayPins[cmd - '4'], HIGH); sendOK(); }
  
  // Read temperatures (For testing, '8' reads sensor1, '9' reads sensor2)
  // Returns temperature in Celsius with 2 decimal places, e.g. "5.25"
  else if (cmd == '8') { sensor1.requestTemperatures(); Serial.println(sensor1.getTempCByIndex(0)); }
  else if (cmd == '9') { sensor2.requestTemperatures(); Serial.println(sensor2.getTempCByIndex(0)); }

  // Read multimeter RMS values
  // Returns: "Voltage|Current0|Current1", e.g. "120.00|0.50|0.30"
  else if (cmd == 'A') { Serial.println(multimeter.readRMS()); }

  // Get parameters
  // Returns: "TargetTemp|DefrostTemp|Differential|DelayTime|CoolingDuration|DefrostDuration"
  else if (cmd == 'P') {
    Refrigerator::Parameters p = fridge.getParameters();
    Serial.print(p.TargetTemp); Serial.print("|"); 
    Serial.print(p.DefrostTemp); Serial.print("|"); 
    Serial.print(p.Differential); Serial.print("|"); 
    Serial.print(p.DelayTime); Serial.print("|"); 
    Serial.print(p.CoolingDuration); Serial.print("|"); 
    Serial.println(p.DefrostDuration);
  }

  // Update parameters (For simplicity, we assume the new value is sent immediately after the command)
  // Retrns "OK" if successful, "ERROR" if validation fails or EEPROM write fails
  else if (cmd == 'C') { Serial.println(fridge.updateTargetTemp(Serial.parseInt())); }
  else if (cmd == 'D') { Serial.println(fridge.updateDefrostTemp(Serial.parseInt())); }
  else if (cmd == 'E') { Serial.println(fridge.updateDifferential(Serial.parseInt())); }
  else if (cmd == 'F') { Serial.println(fridge.updateDelayTime(Serial.parseInt())); }
  else if (cmd == 'G') { Serial.println(fridge.updateCoolingDuration(Serial.parseInt())); }
  else if (cmd == 'H') { Serial.println(fridge.updateDefrostDuration(Serial.parseInt())); }

  // Get calibration
  // Returns: "CurrentCal|VoltageCal"
  else if (cmd == 'K') {
    Serial.print(multimeter.Params.CurrentCal); Serial.print("|");
    Serial.println(multimeter.Params.VoltageCal);
  }
  
  // Update calibration (For simplicity, we assume the new value is sent immediately after the command)
  // Returns "OK" if successful, "ERROR" if validation fails or EEPROM write fails
  else if (cmd == 'U') { Serial.println(multimeter.updateCurrentCal(Serial.parseFloat())); }
  else if (cmd == 'V') { Serial.println(multimeter.updateVoltageCal(Serial.parseFloat())); }
}

unsigned long lastLoopTime = 0;
void loopFridge() {
  if (millis() - lastLoopTime < 1000) // Limit to 1 loops per second{
  {
    return;
  }

  sensor1.requestTemperatures();
  sensor2.requestTemperatures();

  multimeter.loop();
  fridge.loop(sensor1.getTempCByIndex(0), sensor2.getTempCByIndex(0));

  digitalWrite(relayPins[0], fridge.CompressorOn ? LOW : HIGH);
  digitalWrite(relayPins[2], fridge.DefrostOn ? LOW : HIGH);

  lastLoopTime = millis();
}

void loop() {
  loopSerial();
  loopFridge();
}