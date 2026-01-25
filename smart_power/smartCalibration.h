#ifndef SMART_CALIBRATION_H
#define SMART_CALIBRATION_H

#include <Preferences.h>

Preferences prefs;

float calFactor1 = 1.0f;
float calFactor2 = 1.0f;

void initCalibration() {
  prefs.begin("smartpower", false);
  calFactor1 = prefs.getFloat("cal1", 1.0f);
  calFactor2 = prefs.getFloat("cal2", 1.0f);
}

void saveCalibration() {
  prefs.putFloat("cal1", calFactor1);
  prefs.putFloat("cal2", calFactor2);
}

#endif