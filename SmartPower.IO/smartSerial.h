#include "HardwareSerial.h"
#include "WString.h"
#ifndef SMART_SERIAL_H
#define SMART_SERIAL_H

#include "smartCommon.h"
#include "smartSensor.h"

void initSerial() {
  Serial.begin(115200);
}

void processIncomingByte() {
  Serial.println(String(getCurrentS1()));
  Serial.println(String(getCurrentS2()));
  
  Serial.println(String(getTemperatureS1()));
  Serial.println(String(getTemperatureS2()));
}

#endif