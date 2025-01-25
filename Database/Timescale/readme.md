I er velkommen til at bruge forskellige databaser til at gemme værdierne fra vores message broker og embedded enheder. Til undervisningen bruger vi TimescaleDB. 

## [TimescaleDB](https://www.timescale.com/)

TimescaleDB er en database, der er bygget ovenpå PostgreSQL. Den er specielt tilpasset til at gemme store mængder data, som skal gemmes over tid.

TimescaleDB er bygget ovenpå PostgreSQL, så vi kan bruge alle de funktioner, som PostgreSQL har til rådighed. Derfor kan vi bruge vores erfaring fra H1-H4, med at lave struktureret databaser med PostgreSQL, med fordelene ved at bruge TimescaleDB.


## [TimescaleDB Docker Compose](compose.yaml)

Vi har lavet en Docker Compose fil, som vi kan bruge til at starte TimescaleDB. Denne fil starter 2 containere, som vi kan bruge til at gemme værdierne fra vores message broker og embedded enheder.
Der er lavet en til C# og en til Golang - så de har deres egen database.