#ifndef SMART_COMMON_H
#define SMART_COMMON_H

#include <Arduino.h>

// Values are now in Milliamperes (e.g., 500 = 0.5A)
inline volatile int rms05_mA = 0;
inline volatile int rms30_mA = 0;
inline volatile int finalRms_mA = 0;

inline SemaphoreHandle_t dataMutex;

#endif