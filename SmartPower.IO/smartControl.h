#ifndef SMART_CONTROL_H
#define SMART_CONTROL_H

#include "smartCommon.h"

const int relayPins[] = {18, 19, 22, 23};

void initControl() {
  for (int i = 0; i < 4; i++) {
    pinMode(relayPins[i], OUTPUT);
  }
}

void setOn(int pin){
  digitalWrite(pin, HIGH);
}

void setOff(int pin){
  digitalWrite(pin, LOW);
}

#endif