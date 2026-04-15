#include "Arduino.h"
#include <OneWire.h>
#include <DallasTemperature.h>
#ifndef SMART_TEMP_H
#define SMART_TEMP_H

#include "smartCommon.h"

#define SENSOR_PIN_1 4
#define SENSOR_PIN_2 5

// Setup instances for Sensors
OneWire oneWire1(SENSOR_PIN_1); DallasTemperature sensors1(&oneWire1);
OneWire oneWire2(SENSOR_PIN_2); DallasTemperature sensors2(&oneWire2);

void initTemp() {
  sensors1.begin(); sensors1.setWaitForConversion(false);
  sensors2.begin(); sensors2.setWaitForConversion(false);
}

float getTemperatureS1(){
  sensors1.requestTemperatures();
  return sensors1.getTempCByIndex(0);
}

float getTemperatureS2(){
  sensors2.requestTemperatures();
  return sensors2.getTempCByIndex(0);
}

#endif