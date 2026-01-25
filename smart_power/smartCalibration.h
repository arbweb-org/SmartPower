#ifndef SMART_CALIBRATION_H
#define SMART_CALIBRATION_H

#include <Preferences.h>

Preferences prefs;

long s1CalFactX10000 = 10000;
long s2CalFactX10000 = 10000;

void initCalibration() {
  prefs.begin("smartpower", false);
  s1CalFactX10000 = prefs.getLong("cal1", 10000);
  s2CalFactX10000 = prefs.getLong("cal2", 10000);
}

void saveCalibration() {
  prefs.putLong("cal1", s1CalFactX10000);
  prefs.putLong("cal2", s2CalFactX10000);
}

#endif