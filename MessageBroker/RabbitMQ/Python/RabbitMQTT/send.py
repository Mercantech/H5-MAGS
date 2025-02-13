import paho.mqtt.client as mqtt

# MQTT konfiguration
broker = "localhost"  # eller IP-adressen til din RabbitMQ-server
port = 1884  # Porten, du har konfigureret til MQTT
topic = "test/topic"
message = "Hello, RabbitMQ via MQTT!"

# Opret en MQTT-klient
client = mqtt.Client()

# Forbind til broker
client.connect(broker, port)

# Send besked
client.publish(topic, message)

# Afbryd forbindelsen
client.disconnect()
