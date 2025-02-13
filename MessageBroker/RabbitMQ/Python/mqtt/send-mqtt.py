import paho.mqtt.client as mqtt
import time

def on_connect(client, userdata, flags, rc):
    print(f"[on_connect] Connected with result code: {rc}")
    # rc = 0 betyder succes

def on_publish(client, userdata, mid):
    print(f"[on_publish] Message with mid {mid} published.")

def on_log(client, userdata, level, buf):
    print(f"[on_log] {buf}")

# Opret en MQTT Client
client = mqtt.Client()

# Tilføj callbacks
client.on_connect = on_connect
client.on_publish = on_publish
client.on_log = on_log  

# Hvis der kræves login:
client.username_pw_set("admin", "admin")

broker_address = "10.135.61.102"
broker_port = 1883
topic = "test"  
payload = "Hello RabbitMQ via MQTT with debug"

# Forbind
client.connect(broker_address, broker_port, 60)
# Start loop i baggrunden
client.loop_start()

# Vent på forbindelse
time.sleep(2)

# Publish
result = client.publish(topic, payload)
print(f"[main] Publish result: {result}")

# Lad det køre lidt
time.sleep(2)

client.loop_stop()
client.disconnect()
