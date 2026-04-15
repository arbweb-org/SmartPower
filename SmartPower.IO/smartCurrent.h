#include "Arduino.h"
#ifndef SMART_CURRENT_H
#define SMART_CURRENT_H

#include "smartCommon.h"

// Analog pins
#define PIN_S1 A0   // 05A sensor
#define PIN_S2 A2   // 30A sensor

void initCurrent() {

}

int getCurrentS1() {
  return analogRead(PIN_S1);
}

int getCurrentS2() {
  return analogRead(PIN_S2);
}

#endif