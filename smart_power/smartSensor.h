#ifndef SMART_SENSOR_H
#define SMART_SENSOR_H

#include "smartCommon.h"

hw_timer_t *timer = NULL;
#define PIN_S1 34
#define PIN_S2 35

// Synchronizer
portMUX_TYPE bufferMux = portMUX_INITIALIZER_UNLOCKED;

void IRAM_ATTR onTimer() {
  portENTER_CRITICAL_ISR(&bufferMux);
  buffer[head].time = micros();
  buffer[head].s1 = analogRead(PIN_S1);
  buffer[head].s2 = analogRead(PIN_S2);
  head = (head + 1) % BUFFER_SIZE;
  portEXIT_CRITICAL_ISR(&bufferMux);
}

void takeSnapshot() {
  // Stop the world for a few microseconds to copy the data
  portENTER_CRITICAL(&bufferMux);
  memcpy(snapshot, buffer, sizeof(buffer));
  portEXIT_CRITICAL(&bufferMux);
}

void initSensor() {
  // API v3: timerBegin(frequency_in_Hz)
  // 1,000,000 Hz = 1 tick per microsecond
  timer = timerBegin(1000000);

  // API v3: Only 2 arguments
  timerAttachInterrupt(timer, &onTimer);

  // API v3: Frequency of alarm in ticks (200 ticks = 200us)
  timerAlarm(timer, SAMPLE_INTERVAL_US, true, 0);
}

#endif