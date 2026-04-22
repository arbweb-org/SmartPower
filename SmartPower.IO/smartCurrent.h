#include "Arduino.h"
#ifndef SMART_CURRENT_H
#define SMART_CURRENT_H

#include "smartCommon.h"

#define PIN_S1 34
#define PIN_S2 35

// Global variables to store the first-run RMS results
float rmsNoiseFloor1 = -1.0; 
float rmsNoiseFloor2 = -1.0;

void initCurrent() {
  // No startup calibration needed for this method
}

float getRMSSensor(int pin) {
  const int samples = 100; 
  double sumSq = 0;
  
  // Select which offset variable to update/use
  float *storedOffset = (pin == PIN_S1) ? &rmsNoiseFloor1 : &rmsNoiseFloor2;

  // 1. Calculate raw RMS (Root Mean Square of raw ADC counts)
  for (int i = 0; i < samples; i++) {
    long val = analogRead(pin);
    sumSq += (uint32_t)(val * val);
  }
  float currentRMS = sqrt((float)sumSq / samples);

  // 2. Store the first result as offset and return 0
  if (*storedOffset < 0) {
    *storedOffset = currentRMS;
    return 0.00;
  }

  // 3. Subtract the stored offset from the calculated RMS
  float result = currentRMS - *storedOffset;
  
  return (result < 0) ? 0.00 : result;
}

int getCurrentS1() { return analogRead(PIN_S1); }
int getCurrentS2() { return analogRead(PIN_S2); }

#endif