import paho.mqtt.client as mqtt

# Callback-funktioner
def on_connect(client, userdata, flags, rc):
    print("Forbundet med resultatkode: " + str(rc))
    client.subscribe("embedded")  # Abonner på emnet

def on_message(client, userdata, msg):
    print(f"Modtaget besked på {msg.topic}: {msg.payload.decode()}")
    print("Besked modtaget!")  

# Opret en MQTT-klient
client = mqtt.Client()  

# Tilmeld callback-funktioner
client.on_connect = on_connect
client.on_message = on_message

# Forbind til MQTT-broker
client.connect("localhost", 1883, 60)  # Brug MQTT-porten

# Start loopet
client.loop_forever()  # Kør loopet for at modtage beskeder
