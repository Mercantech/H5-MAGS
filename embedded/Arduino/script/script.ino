#include <WiFiNINA.h> // Inkluderer WiFiNINA-biblioteket
#include <PubSubClient.h> // Inkluderer PubSubClient-biblioteket
#include <Arduino_MKRIoTCarrier.h>
#include <ArduinoJson.h>
#include "config.h" // Inkluderer konfigurationsfilen
#include <WiFiUdp.h>
#include <NTPClient.h>

MKRIoTCarrier carrier;
const char* mqtt_server = MQTT_SERVER; 
const char* topic = "ArduinoData"; 
const char* mqtt_username = MQTT_USERNAME;
const char* mqtt_password = MQTT_PASSWORD;

WiFiClient wifiClient;
PubSubClient client(wifiClient);

WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org");
const long gmtOffset_sec = 3600; 

void setup() {
  Serial.begin(9600);
  
  // Initialiser MKR IoT Carrier
  carrier.begin();
  
  // Konfigurer display
  carrier.display.setRotation(0);
  carrier.display.fillScreen(0x0000); 
  carrier.display.setTextColor(0xFFFF); 
  
  // Opret forbindelse til WiFi
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  // Vent på forbindelse
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Forbinder til WiFi...");
  }
  
  Serial.println("Forbundet til WiFi!");

  // Opret forbindelse til MQTT-broker
  client.setServer(mqtt_server, 1883);
  if (client.connect("ArduinoClient", mqtt_username, mqtt_password)) {
    Serial.println("Forbundet til MQTT-broker!");
  } else {
    Serial.print("Fejl ved forbindelse til MQTT-broker, rc=");
    Serial.print(client.state());
  }

  // Efter WiFi forbindelse er etableret
  timeClient.begin();
  timeClient.setTimeOffset(gmtOffset_sec);
}

void loop() {
  // Hold forbindelsen til MQTT-broker
  if (!client.connected()) {
    client.connect("ArduinoClient", mqtt_username, mqtt_password);
  }
  client.loop();

  // Opdater tid før JSON generering
  timeClient.update();

  // Opret JSON dokument
  StaticJsonDocument<200> doc;
  
  // Læs sensordata
  float temperature = carrier.Env.readTemperature();
  float humidity = carrier.Env.readHumidity();
  float pressure = carrier.Pressure.readPressure();
  
  // Opdater display
  carrier.display.fillScreen(0x0000);
  
  // Temperatur
  carrier.display.setTextSize(2);
  carrier.display.setCursor(10, 30);
  carrier.display.print("Temp: ");
  carrier.display.print(temperature, 1);
  carrier.display.print(" C");
  
  // Luftfugtighed
  carrier.display.setCursor(10, 90);
  carrier.display.print("Fugt: ");
  carrier.display.print(humidity, 1);
  carrier.display.print(" %");
  
  // Tryk
  carrier.display.setCursor(10, 150);
  carrier.display.print("Tryk: ");
  carrier.display.print(pressure/1000, 1);
  carrier.display.print(" kPa");
  
  // Tilføj data til JSON dokument
  doc["timestamp"] = timeClient.getEpochTime();
  doc["temperature"] = temperature;
  doc["humidity"] = humidity;
  doc["pressure"] = pressure;
  
  // Konverter JSON til string
  char jsonBuffer[200];
  serializeJson(doc, jsonBuffer);
  
  // Send data
  if (client.publish(topic, jsonBuffer)) {
    Serial.print("Data sendt: ");
    Serial.println(jsonBuffer);
  } else {
    Serial.println("Fejl ved sending af data");
  }

  // Vent 5 sekunder før næste måling
  delay(5000);
}
