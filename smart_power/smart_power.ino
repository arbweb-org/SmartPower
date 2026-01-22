#include <WiFi.h>
#include <WebServer.h>
#include <Preferences.h>
#include <uri/UriBraces.h>

#include "smartCommon.h"
#include "smartServer.h"
#include "smartSensor.h"

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, HIGH);  // turn the LED on (HIGH is the voltage level)

  // 1. Initialize Mutex before starting tasks
  dataMutex = xSemaphoreCreateMutex();
  
  // 2. Setup Server (Core 1)
  server_setup();

  // 3. Start Sensor Task on Core 0
  xTaskCreatePinnedToCore(
    sensorTask,   /* Function */
    "SensorTask", /* Name */
    10000,        /* Stack size */
    NULL,         /* Parameter */
    1,            /* Priority */
    NULL,         /* Handle */
    0             /* Core 0 */
  );
}

void loop() {
  // Core 1 handles the Web Server
  server_loop_once();
  // We can add a tiny delay to let the IDLE task breathe
  vTaskDelay(1);
}