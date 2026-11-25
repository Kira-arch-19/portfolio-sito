# Portfolio — ASP.NET Core MVC

**Portfolio personale di Matteo Porrini Cattò**, realizzato per presentare progetti software e competenze tecniche.

Il sito è anche un progetto dimostrativo: un'applicazione web ASP.NET Core MVC con database PostgreSQL, autenticazione amministrativa e gestione di immagini per i progetti.

## Funzionalità

### Area pubblica

- Home con presentazione e card dei progetti.
- Immagine di copertina per ogni progetto, con segnaposto quando assente.
- Pagina di dettaglio con descrizione, tecnologie, competenze sviluppate e collegamento GitHub.

### Area amministrativa

- Login e logout con **ASP.NET Core Identity**, accesso riservato al ruolo `Admin`.
- Dashboard con elenco dei progetti.
- Gestione **CRUD**: creazione, lettura, modifica ed eliminazione dei progetti.
- Upload di una copertina e più screenshot per la galleria.
- Sostituzione della copertina ed endpoint per eliminare singole immagini.
- Rimozione dei file associati durante l'eliminazione del progetto.

> La View amministrativa `Images.cshtml` attualmente offre il form di caricamento. La visualizzazione e l'eliminazione delle immagini già caricate direttamente da quella pagina sono miglioramenti ancora da aggiungere.

## Stack tecnologico

| Ambito | Tecnologie |
| --- | --- |
| Linguaggio | C# |
| Framework | .NET 10, ASP.NET Core MVC |
| Frontend | Razor Views, HTML, CSS, Bootstrap 5 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 10, Npgsql |
| Autenticazione | ASP.NET Core Identity, cookie e ruoli |
| Upload | `IFormFile`, filesystem in `wwwroot/uploads` |
| Strumenti | Git, GitHub, .NET CLI, EF Core CLI |

## Struttura del repository

```text
Portfolio/
├── Controllers/          # Home, Project, Account, Admin
├── Data/                 # AppDbContext
├── Models/               # Project, ProjectImage, ApplicationUser
├── DTOs/                 # Modelli dati per create/update
├── Services/             # CrudService<T> e ProjectService
│   └── Interfaces/
├── ViewModels/           # Modelli per i form
├── Views/
│   ├── Account/
│   ├── Admin/
│   ├── Home/
│   ├── Project/
│   └── Shared/
├── Migrations/           # Migration EF Core
├── wwwroot/              # File statici e upload
├── Program.cs            # Servizi, middleware e routing
└── Portfolio.csproj
```

### Modello dati

- `Project`: nome, descrizioni, tecnologie, link GitHub, competenze e raccolta di immagini.
- `ProjectImage`: URL, `ProjectId`, flag `IsCover` e posizione `Position`.
- Relazione **uno-a-molti** tra progetto e immagini, con eliminazione a cascata dei record correlati.
- `ApplicationUser`: utente per l'autenticazione tramite Identity.

## Avvio in locale

### Requisiti

- **.NET SDK 10**.
- **PostgreSQL** raggiungibile dal PC o tramite tunnel SSH.
- Tool **dotnet-ef** compatibile con EF Core 10.
- Git.

### 1. Clona il repository

```powershell
git clone https://github.com/Kira-arch-19/portfolio-sito.git
cd portfolio-sito
dotnet restore
```

### 2. Configura la connessione PostgreSQL

Le credenziali **non vanno inserite nel repository**. In sviluppo puoi usare gli User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=15432;Database=NOME_DB;Username=UTENTE_DB;Password=PASSWORD_DB"
```

Se PostgreSQL è sulla VPS, puoi aprire un tunnel SSH in un altro terminale (adatta utente e indirizzo):

```powershell
ssh -N -o ExitOnForwardFailure=yes -L 15432:127.0.0.1:5432 UTENTE_SSH@IP_VPS
```

Mantieni aperto il tunnel mentre utilizzi l'applicazione in locale.

### 3. Configura l'amministratore di sviluppo

In `Development`, il progetto inizializza il ruolo `Admin` e un utente quando sono presenti questi valori:

```powershell
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:Password" "INSERISCI_UNA_PASSWORD_SICURA"
```

Usa una password robusta conforme ai requisiti Identity configurati. **Il seed è attivo solo in `Development`**: in produzione occorre un provisioning sicuro dell'amministratore.

### 4. Applica le migration e avvia il sito

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
dotnet run --no-launch-profile --urls "http://localhost:5124"
```

- Sito: <http://localhost:5124>
- Login Admin: <http://localhost:5124/account/login>
- Dashboard Admin: <http://localhost:5124/admin>

## Route principali

| Metodo | Route | Descrizione | Accesso |
| --- | --- | --- | --- |
| `GET` | `/` | Home e progetti | Pubblico |
| `GET` | `/pagina/{id}` | Dettaglio progetto | Pubblico |
| `GET, POST` | `/account/login` | Login | Pubblico |
| `POST` | `/account/logout` | Logout | Autenticato |
| `GET` | `/admin` | Dashboard | Admin |
| `GET, POST` | `/admin/projects/create` | Creazione progetto | Admin |
| `GET, POST` | `/admin/projects/{id}/edit` | Modifica progetto | Admin |
| `GET, POST` | `/admin/projects/{id}/delete` | Conferma/eliminazione progetto | Admin |
| `GET, POST` | `/admin/projects/{id}/images` | Gestione/upload immagini | Admin |
| `POST` | `/admin/projects/{id}/images/{imageId}/delete` | Elimina immagine | Admin |

Sono presenti anche endpoint JSON sotto `/project`: lettura pubblica (`GET`) e operazioni di modifica (`POST`, `PUT`, `DELETE`) riservate all'amministratore.

## Gestione delle immagini

- **Copertina:** una per progetto; una nuova copertina sostituisce la precedente.
- **Galleria:** fino a 10 nuove immagini per caricamento, massimo 30 immagini per progetto.
- **Formati:** JPG, PNG, WebP; massimo 5 MB per file.
- **Storage:** i file si trovano in `wwwroot/uploads/projects/{id}/` e i loro metadati in PostgreSQL (`project_images`).
- La cartella `wwwroot/uploads/` è esclusa tramite `.gitignore`: `git push` **non trasferisce** le immagini caricate.

La validazione attuale controlla estensione, dimensione e firma iniziale. Prima del deployment pubblico è opportuno aggiungere la validazione completa dei file o la ricodifica delle immagini.

## Sicurezza e deployment

Il codice utilizza ruoli Identity, cookie di autenticazione e token antiforgery per i form che modificano dati.

Prima di pubblicare su una VPS è necessario verificare:

- Configurazione **HTTPS**, hosting e segreti di produzione.
- Provisioning sicuro dell'account Admin in produzione.
- Cartella upload **persistente e scrivibile** e backup dei file.
- Backup PostgreSQL, log ed error handling.
- Test di CRUD, autenticazione, autorizzazione e caricamento immagini.

## Prossimi sviluppi

- Mostrare le immagini esistenti nella pagina Admin, con eliminazione visuale.
- Ampliare le sezioni **Chi sono**, **Competenze**, **Contatti** e collegare il CV.
- Migliorare responsive design, accessibilità e ottimizzazione immagini.
- Aggiungere test automatici e pipeline CI/CD.
- Pubblicare il Portfolio su VPS con dominio e HTTPS.

## Autore

**Matteo Porrini Cattò** — Junior Software Developer  
GitHub: [Kira-arch-19](https://github.com/Kira-arch-19)

---

Progetto personale dimostrativo, in continua evoluzione.
