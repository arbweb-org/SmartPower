#ifndef SMART_CONTROL_H
#define SMART_CONTROL_H

#include "smartCommon.h"

const int relayPins[] = {6, 7, 8, 9};

void initControl() {
  for (int i = 0; i < 4; i++) {
    pinMode(pins[i], OUTPUT);
  }
}

void setOn(int pin){
  digitalWrite(pin, HIGH);
}

void setOff(int pin){
  digitalWrite(pin, LOW);
}

#endif