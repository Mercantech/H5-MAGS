import pika
import sys

# Forbindelsesindstillinger
credentials = pika.PlainCredentials('admin', 'admin')  
parameters = pika.ConnectionParameters(
    host='localhost',
    port=5672,
    credentials=credentials,
    heartbeat=600
)

def callback(ch, method, properties, body):
    print(f" [x] Modtaget: {body.decode()}")
    ch.basic_ack(delivery_tag=method.delivery_tag)

try:
    # Opret forbindelse til RabbitMQ serveren
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    # Opret en kø
    channel.queue_declare(queue='hello')

    # Opsæt modtagelse af beskeder
    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='hello',
                         on_message_callback=callback)

    print(' [*] Venter på beskeder. Tryk CTRL+C for at afslutte')
    channel.start_consuming()

except pika.exceptions.AMQPConnectionError:
    print("Kunne ikke forbinde til RabbitMQ. Tjek at serveren kører.")
except KeyboardInterrupt:
    print("\nLukker ned...")
except Exception as e:
    print(f"Der opstod en fejl: {str(e)}")
finally:
    if 'connection' in locals() and connection.is_open:
        connection.close() 