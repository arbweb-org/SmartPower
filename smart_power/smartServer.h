#ifndef SMART_SERVER_H
#define SMART_SERVER_H

#include <WiFi.h>
#include <WebServer.h>
#include <WebSocketsServer.h>

#include "smartCommon.h"
#include "smartCalibration.h"

// WiFi Configuration
const char* ssid = "WE_D2D139";
const char* password = "e6c37eb3";

// Static IP Configuration
IPAddress local_IP(192, 168, 100, 33);
IPAddress gateway(192, 168, 100, 1);
IPAddress subnet(255, 255, 255, 0);

WebSocketsServer webSocket = WebSocketsServer(81);

// Header structure for binary response (8 bytes)
struct __attribute__((packed)) DataHeader {
  int32_t cal1;
  int32_t cal2;
};

void sendBinaryData(uint8_t num) {
  takeSnapshot();

  // Create header with calibration factors
  DataHeader header;
  header.cal1 = (int32_t)s1CalFactX10000;
  header.cal2 = (int32_t)s2CalFactX10000;

  // Total bytes: 8 (header) + 2000 samples * 12 bytes each = 24,008 bytes
  size_t headerBytes = sizeof(DataHeader);
  size_t sampleBytes = BUFFER_SIZE * sizeof(Sample);
  size_t totalBytes = headerBytes + sampleBytes;

  // Create combined buffer
  uint8_t* combinedBuffer = (uint8_t*)malloc(totalBytes);
  memcpy(combinedBuffer, &header, headerBytes);
  memcpy(combinedBuffer + headerBytes, snapshot, sampleBytes);

  // Send the entire buffer as a binary "blob"
  webSocket.sendBIN(num, combinedBuffer, totalBytes);

  free(combinedBuffer);
}

void webSocketEvent(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
  switch (type) {
    case WStype_DISCONNECTED:
      break;
    case WStype_CONNECTED:
      break;
    case WStype_TEXT:
      // Check for calibration command "cal-INT1-INT2"
      if (length >= 4 && strncmp((char*)payload, "cal|", 4) == 0) {
        char* pEnd;
        // Parse first integer starting after "cal-"
        long val1 = strtol((char*)payload + 4, &pEnd, 10);
        if (pEnd && *pEnd == '|') {
           // Parse second integer starting after the delimiter
           long val2 = strtol(pEnd + 1, NULL, 10);
           
           // Update calibration only if changed
           if (s1CalFactX10000 != val1 || s2CalFactX10000 != val2) {
             s1CalFactX10000 = val1;
             s2CalFactX10000 = val2;
             saveCalibration();
           }
        }
      }
      // Check for data request "get"
      else if (length >= 3 && strncmp((char*)payload, "get", 3) == 0) {
        sendBinaryData(num);
      }
      break;
  }
}

void initWiFi() {
  digitalWrite(LED_BUILTIN, HIGH);

  if (!WiFi.config(local_IP, gateway, subnet)) {
    delay(1000);
    digitalWrite(LED_BUILTIN, LOW);
    delay(1000);
    digitalWrite(LED_BUILTIN, HIGH);
    delay(1000);
    digitalWrite(LED_BUILTIN, LOW);
    return;
  }

  digitalWrite(LED_BUILTIN, LOW);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(250);
    digitalWrite(LED_BUILTIN, LOW);
    delay(250);
  }
  digitalWrite(LED_BUILTIN, HIGH);

  webSocket.begin();
  webSocket.onEvent(webSocketEvent);
}

#endif