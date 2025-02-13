import paho.mqtt.client as mqtt

# Callback-funktion, der kaldes, når en besked modtages
def on_message(client, userdata, message):
    print(f"Modtaget besked: {message.payload.decode()} på emne: {message.topic}")

def on_connect(client, userdata, flags, rc):
    print(f"Forbundet med resultatkode: {rc}")
    client.subscribe(topic)  # Abonner på emnet, når forbindelsen er etableret

# MQTT konfiguration
broker = "localhost"  # eller IP-adressen til din RabbitMQ-server
port = 1883  # Porten, du har konfigureret til MQTT
topic = "test"

# Opret en MQTT-klient
client = mqtt.Client()

# Sæt brugernavn og adgangskode
client.username_pw_set("admin", "admin")  
# Tilmeld callback-funktioner
client.on_connect = on_connect
client.on_message = on_message

# Forbind til broker
client.connect(broker, port)

# Start loopet for at modtage beskeder
client.loop_forever()
