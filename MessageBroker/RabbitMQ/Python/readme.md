# RabbitMQ med Python

RabbitMQ er en messaging broker, der bruges til at sende og modtage beskeder mellem applikationer. Her kører vi en RabbitMQ server i en Docker container og bruger den med 2 Python scripts.

## Send

Send.py sender en besked til RabbitMQ.

## Receive

Receive.py modtager en besked fra RabbitMQ.
Receive, kan bruges til at modtage beskeder fra RabbitMQ og tilføje dem til en database.
### Kodegennemgang af Receive.py
RabbitMQ operer ud fra køer (queues), som er en liste med beskeder. Vores Receive og Send script skal være enig om hvilken kø de sender og modtager fra.

En kø kan håndtere flere forskellige datastrukturer, men vi bruger kun strings, her!
```python
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
```

## HTTP(S) vs RabbitMQ

RabbitMQ bruger AMQP, som er en protokol for messaging ligesom MQTT. Normalt har vi tilføjet værdier til vores database gennem HTTP(S) requests, dog er AMQP hurtigere og mere effektivt, specielt for embedded eller IoT enheder, som har begrænsede ressourcer.