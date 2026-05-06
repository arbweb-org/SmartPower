// --- Relay Configuration ---
const int relayPins[] = {2, 3, 4, 5};
const int numRelays = 4;

void setup() {
  Serial.begin(9600);

  for (int i = 0; i < numRelays; i++) {
    digitalWrite(relayPins[i], HIGH);
    pinMode(relayPins[i], OUTPUT);
  }

  for (int i = 0; i < numRelays; i++) {
    digitalWrite(relayPins[i], LOW);
    pinMode(relayPins[i], OUTPUT);
    delay(1000);
  }
}

void loop() {

}