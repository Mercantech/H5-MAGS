#include <WiFiNINA.h> // Inkluderer WiFiNINA-biblioteket
#include <PubSubClient.h> // Inkluderer PubSubClient-biblioteket
#include "config.h" // Inkluderer konfigurationsfilen

const char* mqtt_server = "192.168.1.234"; 
const char* topic = "embedded"; 

WiFiClient wifiClient;
PubSubClient client(wifiClient);

void setup() {
  Serial.begin(9600);
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
  if (client.connect("ArduinoClient")) {
    Serial.println("Forbundet til MQTT-broker!");
  } else {
    Serial.print("Fejl ved forbindelse til MQTT-broker, rc=");
    Serial.print(client.state());
  }
}

void loop() {
  // Hold forbindelsen til MQTT-broker
  if (!client.connected()) {
    client.connect("ArduinoClient");
  }
  client.loop();

  // Send en generisk besked
  sendMessage("Hello from Arduino!");

  // Vent i 5 sekunder før næste besked
  delay(5000);
}

void sendMessage(const char* message) {
  if (client.publish(topic, message)) {
    Serial.print("Besked sendt: ");
    Serial.println(message);
  } else {
    Serial.println("Fejl ved sending af besked");
  }
}
