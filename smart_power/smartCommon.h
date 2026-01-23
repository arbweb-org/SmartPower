#ifndef SMART_COMMON_H
#define SMART_COMMON_H

// Sampling Configuration
#define SAMPLE_INTERVAL_US 200
#define BUFFER_SIZE 2500  // Adjust based on RAM availability

struct __attribute__((packed)) Sample {
  uint32_t time;  // microseconds
  int s1;
  int s2;
};

Sample buffer[BUFFER_SIZE];
Sample snapshot[BUFFER_SIZE];  // Secondary buffer for the web server
volatile int head = 0;

#endif