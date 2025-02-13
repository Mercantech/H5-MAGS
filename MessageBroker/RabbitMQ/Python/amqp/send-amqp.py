import pika
import sys

# Forbindelsesindstillinger
credentials = pika.PlainCredentials('admin', 'admin')  
parameters = pika.ConnectionParameters(
    host='10.135.61.102',
    port=5672,
    credentials=credentials,
    heartbeat=600
)

try:
    # Opret forbindelse til RabbitMQ serveren
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    # Opret en kø
    channel.queue_declare(queue='hello')

    # Send besked
    message = ' '.join(sys.argv[1:]) or "Hello World!"
    channel.basic_publish(exchange='',
                         routing_key='hello',
                         body=message,
                         properties=pika.BasicProperties(
                             delivery_mode=2  # gør beskeden persistent
                         ))
    print(f" [x] Sendt: {message}")

except pika.exceptions.AMQPConnectionError:
    print("Kunne ikke forbinde til RabbitMQ. Tjek at serveren kører.")
except Exception as e:
    print(f"Der opstod en fejl: {str(e)}")
finally:
    if 'connection' in locals() and connection.is_open:
        connection.close() 