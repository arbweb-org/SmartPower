#include "smartCommon.h"
#include "smartSensor.h"
#include "smartServer.h"

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);

  initCalibration();
  initSensor();
  initWiFi();
}

void loop() {
  webSocket.loop();
  delay(1);
}