1. Opdater systemet
Først skal du opdatere din pakkeindeks:
``` bash
sudo apt update
sudo apt upgrade -y
```
2. Installer Mosquitto
Installer Mosquitto og Mosquitto-clients:
``` bash
sudo apt install mosquitto mosquitto-clients -y
```
3. Start og aktiver Mosquitto
Start Mosquitto-tjenesten og aktiver den, så den starter automatisk ved opstart:
``` bash
sudo systemctl start mosquitto
sudo systemctl enable mosquitto
```
4. Tjek status
Kontroller, at Mosquitto kører korrekt:
``` bash
sudo systemctl status mosquitto
```
5. Konfigurer Mosquitto
Standardkonfigurationen er ofte tilstrækkelig, men du kan tilpasse den ved at redigere konfigurationsfilen:
``` bash
sudo nano /etc/mosquitto/mosquitto.conf
```
For eksempel kan du tillade anonym adgang ved at tilføje følgende linje:
```
allow_anonymous true
```
6. Genstart Mosquitto
Når du har foretaget ændringer i konfigurationen, skal du genstarte Mosquitto:
``` bash
sudo systemctl restart mosquitto
```
7. Test installationen
Du kan teste installationen ved at sende og modtage beskeder. Åbn to terminalvinduer.
I det første vindue (abonnér på et emne):
``` bash
mosquitto_sub -h 127.0.0.1 -p 1883 -t "embedded"
```
I det andet vindue (send en besked):
``` bash
mosquitto_pub -h 127.0.0.1 -p 1883 -t "embedded" -m "Hello, MQTT!"
```
Du skulle nu se beskeden "Hello, MQTT!" i det første vindue.
8. Firewall-indstillinger
Hvis du vil tillade eksterne forbindelser, skal du sikre dig, at port 1883 er åben i din firewall:
``` bash
sudo ufw allow 1883/tcp
```
9. (Valgfrit) Opret brugernavn og adgangskode
Hvis du ønsker at tilføje brugernavn og adgangskode, kan du bruge mosquitto_passwd:
Tilføj derefter følgende linjer til din konfigurationsfil:
``` bash
sudo mosquitto_passwd -c /etc/mosquitto/passwd <username>
```

```
allow_anonymous false
password_file /etc/mosquitto/passwd
```

Genstart Mosquitto igen for at anvende ændringerne.
10. (Valgfrit) Konfigurer Mosquitto til at lytte på alle IP-adresser
For at tillade forbindelser fra eksterne enheder, kan du ændre lytteren i konfigurationsfilen:
``` bash
listener 1883 0.0.0.0
```
