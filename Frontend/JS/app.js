document.addEventListener('DOMContentLoaded', function() {
    // Elementer
    const connectionStatus = document.getElementById('connectionStatus');
    const temperature = document.getElementById('temperature');
    const humidity = document.getElementById('humidity');
    const pressure = document.getElementById('pressure');
    const lastUpdated = document.getElementById('lastUpdated');
    const messageLog = document.getElementById('messageLog');

    // Backend-server URL (dette bør være din API-gateway/WebSocket-proxy)
    const WEBSOCKET_URL = 'ws://10.135.61.102:15674/ws';  // Standard WebSTOMP port

    let stompClient = null;
    
    // Tilføj disse variabler i toppen efter de andre konstanter
    const ctx = document.getElementById('sensorChart').getContext('2d');
    const maxDataPoints = 20; // Antal datapunkter der vises på grafen
    
    // Arrays til at gemme historiske data
    const timeLabels = [];
    const tempData = [];
    const humidityData = [];
    const pressureData = [];
    
    const API_URL = 'http://localhost:5100/api';

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
                    tension: 0.1,
                    yAxisID: 'pressure'
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: false,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Temperatur (°C) / Luftfugtighed (%)'
                    }
                },
                pressure: {
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Lufttryk (hPa)'
                    }
                }
            }
        }
    });

    // Temperatur gauge (0-50°C range)
    const tempGauge = new Chart(document.getElementById('tempGauge'), {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [25, 75],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(200, 200, 200, 0.2)'
                ],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            circumference: 180,
            rotation: 270,
            cutout: '75%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false
                }
            },
            layout: {
                padding: {
                    bottom: 30
                }
            }
        }
    });

    // Luftfugtighed gauge (0-100% range)
    const humidityGauge = new Chart(document.getElementById('humidityGauge'), {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [50, 50],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(200, 200, 200, 0.2)'
                ],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            circumference: 180,
            rotation: 270,
            cutout: '75%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false
                }
            },
            layout: {
                padding: {
                    bottom: 30
                }
            }
        }
    });

    // Tilføj statistik funktioner
    function updateStatistics(data) {
        try {
            // Filtrer kun dagens målinger
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            const todaysData = data.filter(reading => {
                // Konverter createdAt string til Date objekt hvis det ikke allerede er det
                const readingDate = new Date(reading.createdAt);
                // Nulstil tiden til midnat for at sammenligne datoer korrekt
                readingDate.setHours(0, 0, 0, 0);
                return readingDate.getTime() === today.getTime();
            });

            // Beregn temperatur statistik
            const temps = todaysData.map(d => parseFloat(d.temperature));
            const validTemps = temps.filter(t => !isNaN(t));
            
            if (validTemps.length > 0) {
                const avgTemp = validTemps.reduce((a, b) => a + b, 0) / validTemps.length;
                const minTemp = Math.min(...validTemps);
                const maxTemp = Math.max(...validTemps);
                
                document.getElementById('avgTemp').textContent = `${avgTemp.toFixed(1)}°C`;
                document.getElementById('minMaxTemp').textContent = `${minTemp.toFixed(1)}°C / ${maxTemp.toFixed(1)}°C`;
                document.getElementById('measurementCount').textContent = todaysData.length.toString();
            } else {
                document.getElementById('avgTemp').textContent = 'Ingen data';
                document.getElementById('minMaxTemp').textContent = 'Ingen data';
                document.getElementById('measurementCount').textContent = '0';
            }

            // Opdater gauge charts med seneste værdier
            if (todaysData.length > 0) {
                const latestReading = todaysData[todaysData.length - 1];
                
                // Opdater temperatur gauge
                tempGauge.data.datasets[0].data = [latestReading.temperature * 2, 100 - latestReading.temperature * 2];
                tempGauge.update();

                // Opdater luftfugtighed gauge
                humidityGauge.data.datasets[0].data = [latestReading.humidity, 100 - latestReading.humidity];
                humidityGauge.update();
            }
        } catch (error) {
            console.error('Fejl i updateStatistics:', error);
            logMessage('Fejl ved opdatering af statistik: ' + error.message);
        }
    }

    // Tilføj funktion til at hente historisk data
    async function fetchHistoricalData() {
        try {
            const response = await fetch(`${API_URL}/DeviceData`);
            if (!response.ok) {
                throw new Error('Netværksfejl ved hentning af historisk data');
            }
            const data = await response.json();
            
            // Sorter data efter timestamp
            data.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
            
            // Nulstil arrays
            timeLabels.length = 0;
            tempData.length = 0;
            humidityData.length = 0;
            pressureData.length = 0;

            // Tilføj historisk data
            data.forEach(reading => {
                const date = new Date(reading.createdAt);
                timeLabels.push(date.toLocaleTimeString('da-DK'));
                tempData.push(reading.temperature);
                humidityData.push(reading.humidity);
                pressureData.push(reading.pressure);
            });

            // Opdater seneste værdier i UI
            if (data.length > 0) {
                const latest = data[data.length - 1];
                temperature.textContent = latest.temperature.toFixed(2);
                humidity.textContent = latest.humidity.toFixed(2);
                pressure.textContent = latest.pressure.toFixed(2);
                lastUpdated.textContent = `Sidst opdateret: ${new Date(latest.createdAt).toLocaleString('da-DK')}`;

                // Opdater gauge charts
                const tempValue = parseFloat(latest.temperature);
                const humidityValue = parseFloat(latest.humidity);
                updateGauges(tempValue, humidityValue);
                document.getElementById('tempGaugeValue').textContent = tempValue.toFixed(1) + '°C';
                document.getElementById('humidityGaugeValue').textContent = humidityValue.toFixed(1) + '%';
            }

            // Opdater graf
            sensorChart.data.labels = timeLabels;
            sensorChart.data.datasets[0].data = tempData;
            sensorChart.data.datasets[1].data = humidityData;
            sensorChart.data.datasets[2].data = pressureData;
            sensorChart.update();

            // Opdater statistik
            updateStatistics(data);

            logMessage('Historisk data indlæst');
        } catch (error) {
            logMessage('Fejl ved hentning af historisk data: ' + error.message);
        }
    }

    function connect() {
        // Hent først historisk data
        fetchHistoricalData().then(() => {
            // Derefter opret WebSocket forbindelse
            logMessage('Forbinder til AMQP WebSocket...');
            
            const ws = new WebSocket(WEBSOCKET_URL);
            stompClient = Stomp.over(ws);
            
            stompClient.connect(
                {
                    login: 'admin',
                    passcode: 'admin'
                },
                function(frame) {
                    connectionStatus.className = 'connected';
                    connectionStatus.textContent = 'Forbundet til AMQP-server';
                    logMessage('Forbundet til AMQP-server!');
                    
                    stompClient.subscribe('/topic/ArduinoData', function(message) {
                        handleOlcData(message.body);
                    });
                },
                function(error) {
                    connectionStatus.className = 'disconnected';
                    connectionStatus.textContent = 'Forbindelsesfejl: ' + error;
                    logMessage('Forbindelsesfejl: ' + error);
                    
                    setTimeout(connect, 5000);
                }
            );
        });
    }

    // Håndter OLC-data modtaget fra AMQP
    function handleOlcData(messageBody) {
        try {
            const data = JSON.parse(messageBody);
            
            // Opdater UI
            if (temperature) temperature.textContent = parseFloat(data.temperature).toFixed(2);
            if (humidity) humidity.textContent = parseFloat(data.humidity).toFixed(2);
            if (pressure) pressure.textContent = parseFloat(data.pressure).toFixed(2);
            
            // Håndter timestamp
            let dateTime = new Date();
            if (data.timestamp) {
                if (data.timestamp < 20000000000) {
                    dateTime = new Date(data.timestamp * 1000);
                } else {
                    dateTime = new Date(data.timestamp);
                }
            }
            
            // Opdater tidsstempel
            lastUpdated.textContent = `Sidst opdateret: ${dateTime.toLocaleString('da-DK')}`;
            
            // Tilføj nye datapunkter
            const timeLabel = dateTime.toLocaleTimeString('da-DK');
            
            // Tilføj nye værdier og fjern gamle hvis nødvendigt
            if (timeLabels.length >= maxDataPoints) {
                timeLabels.shift();
                tempData.shift();
                humidityData.shift();
                pressureData.shift();
            }
            
            timeLabels.push(timeLabel);
            tempData.push(data.temperature);
            humidityData.push(data.humidity);
            pressureData.push(data.pressure);

            // Opdater grafen
            sensorChart.update();
            
            // Opdater gauge charts med nye værdier
            const tempValue = parseFloat(data.temperature);
            const humidityValue = parseFloat(data.humidity);
            updateGauges(tempValue, humidityValue);
            document.getElementById('tempGaugeValue').textContent = tempValue.toFixed(1) + '°C';
            document.getElementById('humidityGaugeValue').textContent = humidityValue.toFixed(1) + '%';

            // Opdater statistik med alle data punkter
            updateStatistics([...tempData.map((temp, index) => ({
                temperature: temp,
                humidity: humidityData[index],
                pressure: pressureData[index],
                createdAt: new Date()
            }))]);
            
            logMessage(`Ny måling modtaget: Temp ${data.temperature.toFixed(2)}°C, Fugt ${data.humidity.toFixed(2)}%, Tryk ${data.pressure.toFixed(2)}hPa`);
        } catch (error) {
            logMessage('Fejl ved behandling af ny måling: ' + error.message);
        }
    }

    // Logfunktion til messageLog-elementet
    function logMessage(message) {
        const now = new Date();
        const timestamp = now.toLocaleTimeString('da-DK');
        const logEntry = document.createElement('div');
        logEntry.className = 'log-entry';
        logEntry.innerHTML = `<span class="timestamp">[${timestamp}]</span> ${message}`;
        messageLog.appendChild(logEntry);
        messageLog.scrollTop = messageLog.scrollHeight;
    }

    // Tilføj denne funktion til at opdatere målerne
    function updateGauges(temp, humidity) {
        // Opdater temperatur gauge (skaler 0-50°C til 0-100%)
        const tempPercentage = Math.min(Math.max(temp * 2, 0), 100);
        tempGauge.data.datasets[0].data = [tempPercentage, 100 - tempPercentage];
        tempGauge.update();

        // Opdater luftfugtighed gauge (allerede i procent)
        humidityGauge.data.datasets[0].data = [humidity, 100 - humidity];
        humidityGauge.update();

        // Opdater værdi-labels
        document.getElementById('tempGaugeValue').textContent = temp.toFixed(1) + '°C';
        document.getElementById('humidityGaugeValue').textContent = humidity.toFixed(1) + '%';
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