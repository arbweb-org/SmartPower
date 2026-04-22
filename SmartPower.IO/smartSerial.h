#include "HardwareSerial.h"
#include "WString.h"
#ifndef SMART_SERIAL_H
#define SMART_SERIAL_H

#include "smartCommon.h"

void initSerial() {
  Serial.begin(115200);
}

void processIncomingByte() {

}

#endif