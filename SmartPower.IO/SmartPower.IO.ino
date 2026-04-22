#include "smartCommon.h"
#include "smartSerial.h"
#include "smartCurrent.h"
#include "smartTemp.h"
#include "smartControl.h"

unsigned long lastReportTime = 0;
const unsigned long interval = 4000; // 1 second

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  initSerial();
  initCurrent();
  initTemp();
  initControl();
  digitalWrite(LED_BUILTIN, HIGH);
}

void loop() {
  unsigned long currentMillis = millis();

  // Check if 1 second has passed
  if (currentMillis - lastReportTime >= interval) {
    lastReportTime = currentMillis;

    // Fetch and Print Data
    Serial.print("RMS1:"); Serial.println(getRMSSensor(PIN_S1));
    Serial.print("RMS2:"); Serial.println(getRMSSensor(PIN_S2));
    Serial.print("T1:"); Serial.println(getTemperatureS1());
    Serial.print("T2:"); Serial.println(getTemperatureS2());
  }

  delay(10);
  processIncomingByte();
}