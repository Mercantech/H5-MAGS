#include <WiFiNINA.h> // Inkluderer WiFiNINA-biblioteket
#include "config.h" // Inkluderer konfigurationsfilen

void setup() {
  // put your setup code here, to run once:

  // Opret forbindelse til WiFi
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  // Vent på forbindelse
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Forbinder til WiFi...");
  }
  
  Serial.println("Forbundet til WiFi!");
}

void loop() {
  // put your main code here, to run repeatedly:

}
