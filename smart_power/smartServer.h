#ifndef SMART_SERVER_H
#define SMART_SERVER_H

#include <WiFi.h>
#include <WebServer.h>
#include "smartCommon.h"

const char* ssid = "WE_D2D139";
const char* password = "e6c37eb3";

// Static IP Configuration
IPAddress local_IP(192, 168, 100, 33);
IPAddress gateway(192, 168, 100, 1);
IPAddress subnet(255, 255, 255, 0);

WebServer server(80);

void handleRoot() {
  float cRms05, cRms30, cFinal;

  // Protect the read operation to get synchronized values
  if (xSemaphoreTake(dataMutex, portMAX_DELAY)) {
    cRms05 = rms05_mA / 1000.0;
    cRms30 = rms30_mA / 1000.0;
    cFinal = finalRms_mA / 1000.0;
    xSemaphoreGive(dataMutex);
  }

String html = "<html><head><meta http-equiv='refresh' content='1'>";
  html += "<style>body{font-family:sans-serif; text-align:center; background:#f4f4f4;}";
  html += ".card{margin:20px auto; padding:15px; width:80%; max-width:400px; border-radius:10px; border:2px solid;}";
  html += ".final{background:#27ae60; color:white; border-color:#219150;}";
  html += ".raw{background:white; border-color:#bdc3c7; color:#7f8c8d; font-size:0.9em;}</style></head>";
  
  html += "<body>";
  html += "<h1>Power Monitor</h1>";

  // --- Final RMS Display (The Smart Selection) ---
  html += "<div class='card final'>";
  html += "<h3>Final Calculated Load</h3>";
  html += "<span style='font-size: 3em; font-weight: bold;'>" + String(cFinal, 2) + " A</span>";
  html += "<p style='margin-top:5px; opacity:0.8;'>Source: " + String(cRms05 < 1.0 ? "High Precision (5A)" : "High Range (30A)") + "</p>";
  html += "</div>";

  // --- Raw Sensor Data (For Reference) ---
  html += "<div style='display: flex; justify-content: center; gap: 10px;'>";
  html += " <div class='card raw' style='margin:0;'>";
  html += " <b>Sensor 05</b><br>" + String(cRms05, 3) + " A</div>";
    
  html += " <div class='card raw' style='margin:0;'>";
  html += " <b>Sensor 30</b><br>" + String(cRms30, 3) + " A</div>";
  html += "</div>";

  html += "</body></html>";

  server.send(200, "text/html", html);
}

void server_setup(){
  // Apply Static IP settings
  // If config fails, it will fall back to DHCP
  if (!WiFi.config(local_IP, gateway, subnet)) {
    digitalWrite(LED_BUILTIN, LOW);   // turn the LED off by making the voltage LOW
  }

  WiFi.begin(ssid, password);

  // Wait for connection
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(LED_BUILTIN, LOW);   // turn the LED off by making the voltage LOW
    delay(500);
    digitalWrite(LED_BUILTIN, HIGH);   // turn the LED off by making the voltage LOW
    delay(500);
  }

  server.on("/", handleRoot);
  server.begin();
}

void server_loop_once(){
  server.handleClient(); 
}

#endif