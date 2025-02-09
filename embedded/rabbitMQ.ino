#include <WiFiNINA.h>
#include <PubSubClient.h>

// WiFi credentials
char ssid[] = "MAGS-OLC";
char pass[] = "Merc1234!";

// RabbitMQ credentials
const char* mqttServer = "rabbit.mercantec.tech";
const int mqttPort = 1883;
const char* mqttUser = "guest";
const char* mqttPassword = "guest";

// Device ID
const char* deviceId = "1";

// Sensor data variables
float lastHumidity = 0;
float lastTemp = 0;

WiFiClient espClient;
PubSubClient client(espClient);

void setup() {
  Serial.begin(9600);
  setupWiFi();
  client.setServer(mqttServer, mqttPort);
}

void loop() {
  handleTempHumid();
}

void handleTempHumid() {
  float humidity = carrier.Env.readHumidity();
  float temp = carrier.Env.readTemperature();

  // Print value if it has changed
  if (humidity != lastHumidity || temp != lastTemp) {
    lastHumidity = humidity;
    lastTemp = temp;

    int intTemp = temp * 100;
    int intHumid = humidity * 100;

    // Create the MQTT message
    String postBody = "{ \"temp\": " + String(intTemp) + ", \"humidity\": " + String(intHumid) + ", \"sitSmartDeviceId\": \"" + String(deviceId) + "\" }";

    // Publish the message to the RabbitMQ queue
    if (!client.connected()) {
      reconnect();
    }
    client.publish("sensor/data", postBody.c_str());
  }
}

void setupWiFi() {
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
}

void reconnect() {
  while (!client.connected()) {
    if (client.connect("ArduinoClient", mqttUser, mqttPassword)) {
      Serial.println("Connected to MQTT");
    } else {
      delay(5000);
    }
  }
}