#ifndef SMART_SERVER_H
#define SMART_SERVER_H

#include <WiFi.h>
#include <WebServer.h>
#include <WebSocketsServer.h>

#include "smartCommon.h"

// WiFi Configuration
const char* ssid = "WE_D2D139";
const char* password = "e6c37eb3";

// Static IP Configuration
IPAddress local_IP(192, 168, 100, 33);
IPAddress gateway(192, 168, 100, 1);
IPAddress subnet(255, 255, 255, 0);

WebSocketsServer webSocket = WebSocketsServer(81);

void sendBinaryData(uint8_t num) {
  takeSnapshot();

  // Calculate total bytes: 2500 samples * 12 bytes each = 30,000 bytes
  size_t totalBytes = BUFFER_SIZE * sizeof(Sample);

  // Send the entire snapshot array as a binary "blob"
  webSocket.sendBIN(num, (uint8_t*)snapshot, totalBytes);
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