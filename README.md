
# Portfolio — ASP.NET Core MVC

Portfolio personale sviluppato con **C# e ASP.NET Core 10 MVC**, progettato per presentare i miei progetti software, le competenze tecniche e il mio percorso di crescita come Junior Software Developer.

Il progetto non è soltanto un sito vetrina: è un'applicazione web completa con database relazionale, autenticazione, area amministrativa, gestione dinamica dei contenuti, test automatici e deployment containerizzato.

### 🌐 Live Demo

**[Visita il Portfolio](https://portfolio.codebook.tech)**

**[Profilo GitHub](https://github.com/Kira-arch-19)**

---

## 📌 Panoramica del progetto

Il Portfolio nasce con l'obiettivo di raccogliere e presentare i miei progetti informatici attraverso un'applicazione sviluppata e distribuita autonomamente.

L'architettura si basa sul pattern **Model-View-Controller (MVC)** e utilizza Entity Framework Core per l'interazione con PostgreSQL.

Il progetto comprende:

- Frontend responsive con Razor Views e Bootstrap.
- Backend ASP.NET Core 10.
- Database PostgreSQL con Entity Framework Core.
- Area amministrativa protetta con ASP.NET Core Identity.
- Gestione CRUD dei progetti.
- Caricamento e gestione delle immagini.
- Modulo Contatti con invio email tramite SMTP.
- Test automatici con xUnit.
- Continuous Integration tramite GitHub Actions.
- Deployment su VPS Ubuntu attraverso Docker.
- Reverse proxy Caddy e certificato HTTPS.

---

## 🛠️ Stack tecnologico

| Area | Tecnologie |
|---|---|
| Linguaggio | C# |
| Framework | .NET 10, ASP.NET Core MVC |
| Frontend | HTML5, CSS3, Bootstrap 5, Razor |
| Database | PostgreSQL 17 |
| ORM | Entity Framework Core 10 |
| Provider database | Npgsql |
| Autenticazione | ASP.NET Core Identity |
| Email | MailKit, SMTP |
| Testing | xUnit, SQLite, Coverlet |
| Containerizzazione | Docker, Docker Compose |
| Server | Ubuntu Linux VPS |
| Reverse Proxy | Caddy |
| Sicurezza | HTTPS, TLS, Data Protection |
| CI | GitHub Actions |
| Versionamento | Git, GitHub |

---

## ✨ Funzionalità

### Area pubblica

**Home Page**

La pagina principale presenta il mio profilo professionale, le tecnologie utilizzate e i progetti software.

I progetti vengono recuperati dinamicamente dal database PostgreSQL.

**Portfolio progetti**

Ogni progetto può includere:

- Nome e descrizione breve.
- Descrizione completa.
- Tecnologie utilizzate.
- Competenze sviluppate.
- Collegamento al repository GitHub.
- Immagine di copertina.
- Galleria di screenshot.

**Modulo Contatti**

Il sito include un modulo per ricevere messaggi dai visitatori.

L'invio delle email avviene attraverso MailKit e un server SMTP configurabile.

Sono presenti:

- Validazione dei campi.
- Protezione antiforgery.
- Honeypot antispam.
- Rate limiting delle richieste.
- Gestione degli errori di invio.

### Area amministrativa

L'accesso è riservato agli utenti con ruolo `Admin`.

Le principali funzionalità sono:

- Login e logout.
- Dashboard amministrativa.
- Creazione di nuovi progetti.
- Modifica dei progetti esistenti.
- Eliminazione dei progetti.
- Upload delle immagini.
- Sostituzione delle copertine.
- Gestione dei dati tramite Entity Framework Core.

L'autenticazione e l'autorizzazione utilizzano ASP.NET Core Identity.

---

## 🏗️ Architettura software

Il progetto segue una separazione delle responsabilità attraverso controller, servizi, modelli e accesso ai dati.

```text
Browser
   |
   v
ASP.NET Core MVC
   |
   +-- Controllers
   |      |
   |      v
   +-- Services
   |      |
   |      v
   +-- Entity Framework Core
   |      |
   |      v
   +-- PostgreSQL
   |
   +-- Razor Views
   |
   +-- ASP.NET Core Identity
   |
   +-- MailKit / SMTP
```

### Dependency Injection

I servizi applicativi vengono registrati nel container Dependency Injection di ASP.NET Core.

Esempio:

```csharp
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IContactEmailService, ContactEmailService>();
```

L'utilizzo delle interfacce permette di separare l'implementazione dalla logica dei controller e facilita l'esecuzione dei test automatici.

### Entity Framework Core

Il database utilizza un `AppDbContext` per gestire le entità e le relazioni.

Le principali entità applicative sono:

- `Project`
- `ProjectImage`
- `ApplicationUser`

Tra `Project` e `ProjectImage` è presente una relazione uno-a-molti.

Le modifiche allo schema vengono gestite attraverso le migration di Entity Framework Core.

---

## 📁 Struttura del repository

```text
Portfolio/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── ProjectController.cs
│   ├── AdminController.cs
│   └── AccountController.cs
│
├── Models/
│   ├── Project.cs
│   ├── ProjectImage.cs
│   └── ApplicationUser.cs
│
├── DTOs/
├── ViewModels/
│
├── Services/
│   ├── Interfaces/
│   ├── CrudService.cs
│   ├── ProjectService.cs
│   └── ContactEmailService.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Migrations/
│
├── Views/
│   ├── Home/
│   ├── Project/
│   ├── Admin/
│   ├── Account/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── uploads/
│
├── tests/
│   └── Portfolio.Tests/
│
├── .github/
│   └── workflows/
│       └── dotnet-tests.yml
│
├── Dockerfile
├── compose.yaml
├── caddy.compose.yaml
├── Caddyfile
├── Program.cs
├── Portfolio.csproj
└── README.md
```

---

## 🧪 Test automatici

Il Portfolio include una suite di test automatici sviluppata con **xUnit**.

I test verificano il comportamento delle principali funzionalità applicative, tra cui:

- Operazioni CRUD.
- Gestione dei progetti.
- Validazione dei DTO.
- Comportamento dei controller.
- Gestione delle risposte HTTP.
- Controlli sul modulo Contatti.
- Logica di accesso ai dati.
- Gestione delle immagini associate ai progetti.

Per i test del database viene utilizzato SQLite in memoria, evitando modifiche ai dati di produzione.

### Esecuzione dei test

```bash
dotnet test tests/Portfolio.Tests/Portfolio.Tests.csproj -c Release
```

### Risultato della suite

**33 test superati — 0 falliti.**

### Code Coverage

Per generare i dati di copertura:

```bash
dotnet test tests/Portfolio.Tests/Portfolio.Tests.csproj \
  -c Release \
  --collect:"XPlat Code Coverage"
```

---

## 🔄 Continuous Integration

Il progetto utilizza **GitHub Actions** per verificare automaticamente il codice.

La pipeline viene eseguita quando vengono effettuati:

- Push sul branch `main`.
- Pull request verso `main`.

La pipeline esegue:

1. Checkout del repository.
2. Installazione di .NET 10.
3. Ripristino delle dipendenze NuGet.
4. Compilazione del progetto.
5. Esecuzione dei test automatici.
6. Raccolta dei risultati e della code coverage.

**[Visualizza GitHub Actions](https://github.com/Kira-arch-19/portfolio-sito/actions)**

La Continuous Integration permette di rilevare eventuali regressioni prima della pubblicazione delle modifiche.

---

## 🐳 Docker e deployment

Il Portfolio è pubblicato su una VPS Ubuntu utilizzando Docker.

L'applicazione viene compilata attraverso un Dockerfile multi-stage:

- **Build stage:** utilizza l'immagine ufficiale .NET SDK 10.
- **Runtime stage:** utilizza ASP.NET Core Runtime 10.

Questa configurazione permette di separare gli strumenti di compilazione dall'ambiente di esecuzione.

### Architettura di produzione

```text
                    INTERNET
                        |
                        |
             portfolio.codebook.tech
                        |
                        v
                 HTTPS / TLS
                        |
                        v
              +------------------+
              |      Caddy       |
              |  Reverse Proxy   |
              +------------------+
                        |
                        v
              +------------------+
              |   ASP.NET Core   |
              |  Portfolio App   |
              |     Docker       |
              +------------------+
                        |
                        v
              +------------------+
              |   PostgreSQL 17  |
              |     Docker       |
              +------------------+
```

### Docker Compose

L'applicazione utilizza un servizio Docker dedicato:

```bash
docker compose up -d --build app
```

PostgreSQL è gestito da uno stack Docker separato e viene raggiunto tramite la rete Docker `postgres_default`.

### Persistenza dei dati

Sono utilizzati volumi Docker per conservare:

- Immagini dei progetti.
- Chiavi ASP.NET Core Data Protection.
- Dati PostgreSQL.

In questo modo i dati persistenti non dipendono dal ciclo di vita del container applicativo.

---

## 🔒 Sicurezza

Sono state implementate diverse misure di sicurezza:

**Autenticazione e autorizzazione**

ASP.NET Core Identity gestisce l'autenticazione degli utenti e il controllo dell'accesso alle funzionalità amministrative.

**HTTPS**

Caddy gestisce le connessioni HTTPS e il rinnovo automatico dei certificati TLS.

**Data Protection**

Le chiavi crittografiche utilizzate da ASP.NET Core vengono conservate in un volume Docker persistente.

**Reverse Proxy**

ASP.NET Core utilizza i Forwarded Headers per interpretare le informazioni inoltrate dal proxy attendibile.

**Gestione delle credenziali**

Le password e le stringhe di connessione non vengono inserite nel codice sorgente.

In sviluppo vengono utilizzati gli User Secrets di .NET.

In produzione vengono utilizzate variabili d'ambiente configurate direttamente sul server.

**Protezione del modulo Contatti**

Sono presenti validazione dei dati, protezione antiforgery, honeypot e rate limiting.

---

## 💻 Esecuzione in locale

### Requisiti

- .NET SDK 10
- PostgreSQL
- Git
- Tool EF Core per la gestione delle migration

### 1. Clonare il repository

```bash
git clone https://github.com/Kira-arch-19/portfolio-sito.git
cd portfolio-sito
```

### 2. Ripristinare le dipendenze

```bash
dotnet restore Portfolio.csproj
```

### 3. Configurare il database

Utilizzare .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=portfolio_db;Username=UTENTE;Password=PASSWORD"
```

Sostituire i segnaposto con le proprie credenziali.

Per il modulo Contatti è necessario configurare anche i parametri SMTP utilizzati dal servizio email.

### 4. Applicare le migration

Con il database configurato:

```bash
dotnet ef database update
```

### 5. Avviare il progetto

```bash
dotnet run --no-launch-profile --urls http://localhost:5124
```

L'applicazione sarà raggiungibile da:

http://localhost:5124

---

## 🚀 Aggiornamento del sito in produzione

Il deployment viene attualmente eseguito manualmente dopo la pubblicazione delle modifiche su GitHub.

### 1. Sviluppo locale

Dopo aver modificato il codice:

```bash
dotnet test tests/Portfolio.Tests/Portfolio.Tests.csproj -c Release

git status
git add .
git commit -m "feat: update portfolio"
git pull --rebase origin main
git push origin main
```

Prima del commit è importante verificare che non siano inclusi file con credenziali.

### 2. Aggiornamento sulla VPS

Collegarsi alla VPS Ubuntu ed entrare nella directory dell'applicazione:

```bash
cd /opt/portfolio/site
```

Scaricare gli aggiornamenti:

```bash
git pull --ff-only origin main
```

Ricompilare e aggiornare il container applicativo:

```bash
docker compose up -d --build --no-deps app
```

### 3. Verifica del deployment

```bash
docker compose ps
```

Verificare la risposta HTTP:

```bash
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  https://portfolio.codebook.tech/
```

Controllare eventuali errori:

```bash
docker logs --tail 50 portfolio-app
```

Questa procedura aggiorna il sito senza ricreare il database PostgreSQL o il reverse proxy Caddy.

Le modifiche allo schema del database richiedono la gestione separata delle migration e un backup preventivo.

---

## 🔮 Possibili sviluppi futuri

Il progetto è funzionante nella sua prima versione, ma può essere ulteriormente migliorato attraverso:

- Test di integrazione HTTP.
- Test con PostgreSQL isolato.
- Gestione visuale avanzata delle immagini nella dashboard.
- Backup automatici del database.
- Ottimizzazione delle immagini e delle prestazioni.
- Miglioramenti di accessibilità.
- Pipeline di Continuous Deployment.
- Monitoraggio applicativo.

---

## 👨‍💻 Autore

**Matteo Porrini Cattò**

Junior Software Developer

Interessato allo sviluppo backend, alla progettazione di applicazioni software e alle tecnologie IT.

**Portfolio:** https://portfolio.codebook.tech

**GitHub:** https://github.com/Kira-arch-19

---

**Progetto personale sviluppato per apprendimento, sperimentazione e dimostrazione delle competenze tecniche.**
