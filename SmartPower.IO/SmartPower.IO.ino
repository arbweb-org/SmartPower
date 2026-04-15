#include "smartCommon.h"
#include "smartSerial.h"
#include "smartCurrent.h"
#include "smartTemp.h"
#include "smartControl.h"

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);

  initSerial();
  initCurrent();
  initTemp();
  initControl();

  digitalWrite(LED_BUILTIN, HIGH);
}

void loop() {
  delay(1000);
  setOn();

  processIncomingByte();

  delay(1000);
  setOff();
}