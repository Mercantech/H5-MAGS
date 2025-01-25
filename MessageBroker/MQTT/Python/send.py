import paho.mqtt.client as mqtt

# Callback-funktioner
def on_connect(client, userdata, flags, rc):
    print("Forbundet med resultatkode: " + str(rc))
    client.subscribe("embedded")  # Abonner på emnet

# Opret en MQTT-klient
client = mqtt.Client()

# Tilmeld callback-funktioner
client.on_connect = on_connect

# Forbind til MQTT-broker
client.connect("localhost", 1883, 60)  # Brug MQTT-porten

# Start loopet
client.loop_start()

# Send beskeder
try:
    while True:
        topic = "embedded"
        message = input("Indtast besked: ")
        client.publish(topic, message)
except KeyboardInterrupt:
    print("Afslutter...")
finally:
    client.loop_stop()
    client.disconnect()
