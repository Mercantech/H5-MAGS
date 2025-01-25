# RabbitMQ

## Om RabbitMQ
RabbitMQ er en open source message broker software, der understøtter flere forskellige messaging protokoller. Det fungerer som en mellemmand der kan modtage og videresende beskeder mellem forskellige dele af et system.

## Installation med Docker
I dette projekt kører vi RabbitMQ i en Docker container for nem opsætning og vedligeholdelse.

1. **Start RabbitMQ med Docker**
   ```bash
   docker run -d --name rabbitmq \
     -p 5672:5672 \
     -p 15672:15672 \
     rabbitmq:3-management
   ```
   Dette starter en RabbitMQ container med:
   - Management interface på port 15672
   - AMQP protokol på port 5672
   - Standardbruger 'guest' med password 'guest'

2. **Verificer at containeren kører**
   ```bash
   docker ps
   ```

3. **Management Interface**
   - Åbn http://localhost:15672 i din browser
   - Log ind med guest/guest

## Python Dependencies
- Opret et virtuelt miljø: `python -m venv .venv`
- Aktiver miljøet:
  - Windows: `.venv\Scripts\activate`
  - Linux/Mac: `source .venv/bin/activate`
- Installer dependencies: `pip install -r requirements.txt`

## Kørsel af eksempler
1. Sørg for at RabbitMQ containeren kører
2. Åbn to terminaler
3. I første terminal kør: `python receive.py`
4. I anden terminal kør: `python send.py "Din besked her"`

Beskederne vil blive sendt via RabbitMQ serveren fra send.py til receive.py.

## Docker Administration
- Stop containeren: `docker stop rabbitmq`
- Start containeren igen: `docker start rabbitmq`
- Se logs: `docker logs rabbitmq`
- Fjern containeren: `docker rm -f rabbitmq`

