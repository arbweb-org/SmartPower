#ifndef SMART_SENSOR_H
#define SMART_SENSOR_H

#include "smartCommon.h"

static const int pin05 = 34;
static const int pin30 = 35;

static float factor05 = 386.61;
static float factor30 = 42.84;

// Settings for 50Hz
const int numSamples = 200;                 // Number of readings (2 cycles)
const unsigned long sampleInterval = 200;   // Microseconds between readings

// Matricies to store the samples of 2 cycles
// In order to calculate RMS
inline int matrix05[numSamples];
inline int matrix30[numSamples];

void sensor_setup(){

}

void sensor_loop_once(){
  long sum05 = 0;
  long sum30 = 0;

  float mean05 = 0;
  float mean30 = 0;

  unsigned long lastSampleTime = micros();
  // 1. COLLECT DATA (First Loop)
  for (int j = 0; j < numSamples; j++) {
    // Wait until it's exactly time for the next sample
    while (micros() - lastSampleTime < sampleInterval);
    lastSampleTime += sampleInterval;

    matrix05[j] = analogRead(pin05);
    sum05 = sum05 + matrix05[j];

    matrix30[j] = analogRead(pin30);
    sum30 = sum30 + matrix30[j];
  }

  // Calculate the DC Offset (Mean)
  mean05 = (float)sum05 / numSamples;
  mean30 = (float)sum30 / numSamples;

  // 2. CALCULATE RMS
  double sumSquared05 = 0;
  double sumSquared30 = 0;
    
  for (int j = 0; j < numSamples; j++) {
    // Offset correction
    float val05 = (float)matrix05[j] - mean05;
    sumSquared05 += (double)val05 * val05;

    float val30 = (float)matrix30[j] - mean30;
    sumSquared30 += (double)val30 * val30;
  }

  // Update shared variables safely using Mutex
  if (xSemaphoreTake(dataMutex, portMAX_DELAY)) {
    // Convert floats to mA integers
    float temp05 = (sqrt(sumSquared05 / numSamples) / factor05);
    float temp30 = (sqrt(sumSquared30 / numSamples) / factor30);

    rms05_mA = (int)(temp05 * 1000.0);
    rms30_mA = (int)(temp30 * 1000.0);

    // Logic: If less than 1000mA (1A), use Sensor 05
    if (rms05_mA < 1000) {
      finalRms_mA = rms05_mA;
    } else {
      finalRms_mA = rms30_mA;
    }
    
    xSemaphoreGive(dataMutex);
  }
}

// Multicore Task Wrapper
inline void sensorTask(void * pvParameters) {
  for(;;) {
    unsigned long start = millis();
    sensor_loop_once();
    
    // Ensure it runs every 50ms
    long sleepTime = 50 - (millis() - start);
    vTaskDelay(pdMS_TO_TICKS(max(1L, sleepTime)));
  }
}

#endif