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
  float cal1;
  float cal2;
};

void sendBinaryData(uint8_t num) {
  takeSnapshot();

  // Create header with calibration factors
  DataHeader header;
  header.cal1 = calFactor1;
  header.cal2 = calFactor2;

  // Total bytes: 8 (header) + 2500 samples * 12 bytes each = 30,008 bytes
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

void sendCalibration(uint8_t num) {
  char response[64];
  snprintf(response, sizeof(response), "cal:%.6f,%.6f", calFactor1, calFactor2);
  webSocket.sendTXT(num, response);
}

void handleSetCalibration(uint8_t num, char* payload) {
  // Expected format: "setcal:1.23,4.56"
  char* data = payload + 7;  // Skip "setcal:"
  char* comma = strchr(data, ',');
  if (comma) {
    *comma = '\0';
    calFactor1 = atof(data);
    calFactor2 = atof(comma + 1);
    saveCalibration();
    webSocket.sendTXT(num, "ok");
  } else {
    webSocket.sendTXT(num, "err");
  }
}

void webSocketEvent(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
  switch (type) {
    case WStype_DISCONNECTED:
      break;
    case WStype_CONNECTED:
      break;
    case WStype_TEXT:
      // If client sends "get", we blast the data
      if (strncmp((char*)payload, "get", 3) == 0) {
        sendBinaryData(num);
      }
      // Get calibration factors
      else if (strncmp((char*)payload, "getcal", 6) == 0) {
        sendCalibration(num);
      }
      // Set calibration factors (format: "setcal:1.23,4.56")
      else if (strncmp((char*)payload, "setcal:", 7) == 0) {
        handleSetCalibration(num, (char*)payload);
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