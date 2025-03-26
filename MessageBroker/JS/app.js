document.addEventListener('DOMContentLoaded', function() {
    // Elementer
    const connectionStatus = document.getElementById('connectionStatus');
    const temperature = document.getElementById('temperature');
    const humidity = document.getElementById('humidity');
    const pressure = document.getElementById('pressure');
    const lastUpdated = document.getElementById('lastUpdated');
    const messageLog = document.getElementById('messageLog');

    // Backend-server URL (dette bør være din API-gateway/WebSocket-proxy)
    const WEBSOCKET_URL = 'ws://10.134.11.100:15674/ws';  // Standard WebSTOMP port

    let stompClient = null;
    
    // Tilføj disse variabler i toppen efter de andre konstanter
    const ctx = document.getElementById('sensorChart').getContext('2d');
    const maxDataPoints = 20; // Antal datapunkter der vises på grafen
    
    // Arrays til at gemme historiske data
    const timeLabels = [];
    const tempData = [];
    const humidityData = [];
    const pressureData = [];
    
    // Opret graf
    const sensorChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: timeLabels,
            datasets: [
                {
                    label: 'Temperatur (°C)',
                    data: tempData,
                    borderColor: 'rgb(255, 99, 132)',
                    tension: 0.1
                },
                {
                    label: 'Luftfugtighed (%)',
                    data: humidityData,
                    borderColor: 'rgb(54, 162, 235)',
                    tension: 0.1
                },
                {
                    label: 'Lufttryk (hPa)',
                    data: pressureData,
                    borderColor: 'rgb(75, 192, 192)',
                    tension: 0.1
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: false
                }
            }
        }
    });

    function connect() {
        // Logfør forbindelsesforsøg
        logMessage('Forbinder til AMQP WebSocket...');
        
        // Opret direkte WebSocket-forbindelse (bemærk: IKKE SockJS)
        const ws = new WebSocket(WEBSOCKET_URL);
        stompClient = Stomp.over(ws);
        
        // Konfigurer STOMP klient
        stompClient.connect(
            { // Autentificeringsoplysninger hvis nødvendigt
                login: 'admin',
                passcode: 'admin'
            }, 
            // Ved forbindelse
            function(frame) {
                connectionStatus.className = 'connected';
                connectionStatus.textContent = 'Forbundet til AMQP-server';
                logMessage('Forbundet til AMQP-server!');
                
                // Abonner på OLC-data-emnet
                stompClient.subscribe('/topic/ArduinoData', function(message) {
                    handleOlcData(message.body);
                });
            },
            // Ved fejl
            function(error) {
                connectionStatus.className = 'disconnected';
                connectionStatus.textContent = 'Forbindelsesfejl: ' + error;
                logMessage('Forbindelsesfejl: ' + error);
                
                // Prøv at forbinde igen efter 5 sekunder
                setTimeout(function() {
                    connect();
                }, 5000);
            }
        );
    }

    // Håndter OLC-data modtaget fra AMQP
    function handleOlcData(messageBody) {
        try {
            logMessage('Rå data modtaget: ' + messageBody);
            
            const data = JSON.parse(messageBody);
            
            // Opdater UI
            temperature.textContent = data.temperature.toFixed(2);
            humidity.textContent = data.humidity.toFixed(2);
            pressure.textContent = data.pressure.toFixed(2);
            
            // Konverter timestamp baseret på længde (ligesom i C#-koden)
            let dateTime;
            if (data.timestamp < 20000000000) {
                dateTime = new Date(data.timestamp * 1000);
            } else {
                dateTime = new Date(data.timestamp);
            }
            
            // Opdater tidsstempel
            lastUpdated.textContent = `Sidst opdateret: ${dateTime.toLocaleString('da-DK')}`;
            
            // Tilføj nye datapunkter
            const timeLabel = dateTime.toLocaleTimeString('da-DK');
            timeLabels.push(timeLabel);
            tempData.push(data.temperature);
            humidityData.push(data.humidity);
            pressureData.push(data.pressure);

            // Behold kun de seneste maxDataPoints datapunkter
            if (timeLabels.length > maxDataPoints) {
                timeLabels.shift();
                tempData.shift();
                humidityData.shift();
                pressureData.shift();
            }

            // Opdater grafen
            sensorChart.update();
            
            logMessage(`Behandlet OLC-data: Temperatur ${data.temperature.toFixed(2)}°C, Fugtighed ${data.humidity.toFixed(2)}%, Tryk ${data.pressure.toFixed(2)}hPa`);
        } catch (error) {
            logMessage('Fejl ved parsing af besked: ' + error.message);
        }
    }

    // Logfunktion til messageLog-elementet
    function logMessage(message) {
        const now = new Date();
        const timestamp = now.toLocaleTimeString('da-DK');
        const logEntry = document.createElement('div');
        logEntry.textContent = `[${timestamp}] ${message}`;
        messageLog.appendChild(logEntry);
        
        // Auto-scroll til bunden
        messageLog.scrollTop = messageLog.scrollHeight;
    }

    // Start forbindelsen
    connect();
    
    // Tilføj en vindueslukningshåndtering for at lukke forbindelsen pænt
    window.addEventListener('beforeunload', function() {
        if (stompClient !== null) {
            stompClient.disconnect();
        }
    });
});