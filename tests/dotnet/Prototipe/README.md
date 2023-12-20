# C# Helper per Nuvolaris

## Panoramica

Questo progetto è una soluzione .NET per OpenWhisk, progettata per gestire operazioni di database e servizi web. La componente principale è una DLL che, una volta caricata in OpenWhisk, utilizza il metodo `Execute` come punto di ingresso per tutte le richieste.

## Funzionalità

Il progetto offre diverse funzionalità chiave, tra cui:

### Interazione con Database

- **Operazioni CRUD**: Supporta operazioni Create, Read, Update, Delete su database PostgreSQL.
- **Gestione Tabelle**: Capacità di creare, troncare e eliminare tabelle.
- **Query Personalizzate**: Esegue query SQL personalizzate con validazione per la sicurezza.

### Gestione Richieste HTTP

- **Routing Dinamico**: Gestisce richieste HTTP in base al metodo (GET, POST, PUT, DELETE, PATCH).
- **Elaborazione Richieste**: Converte le richieste JSON in operazioni di database o altre azioni.
- **Risposte Formattate**: Restituisce i risultati in un formato JSON strutturato.

### Configurazione e Personalizzazione

- **Custom Attributes**: Configura azione, package di OpenWhisk e la route di APISIX tramite Custom Attributes sulla classe `Execute`.
- **DTO Personalizzabile**: Modifica lo schema della tabella modificando il DTO.
- **Handle Verbi HTTP Personalizzati**: Modifica gli handle dei verbi per operazioni non standard.

### Integrazione OpenWhisk

- **Metodo `Execute`**: Punto di ingresso unificato per tutte le richieste in OpenWhisk.
- **Scalabilità e Prestazioni**: Progettato per sfruttare la scalabilità e l'efficienza di OpenWhisk.

### Strumenti di Sviluppo e Deploy

- **Generazione File di Deploy**: Script per generare automaticamente il file di deploy per OpenWhisk.
- **Automazione Deploy**: Script `build.sh` per un deploy semplificato e automatizzato in OpenWhisk.

## Configurazione

Per il corretto funzionamento del progetto, è necessario configurare:

- `appsettings.json` con i dettagli specifici dell'ambiente.
- Custom Attributes sopra il metodo `Execute` per definire azione, package di OpenWhisk e route di APISIX.
- DTO per determinare lo schema della tabella.
- Handle dei verbi HTTP per operazioni personalizzate.
- Nuvolaris per una corretta integrazione con OpenWhisk.

## Test e Debug

- **Debug**: Esegui `.\debug.sh` per avviare il debug dell'eseguibile e fermarti sulla sezione test.
- **Test Operativi**: Dopo il deploy, utilizza il plugin di VSCode per OpenWhisk per eseguire i test con facilità.

## Installazione e Uso

Per utilizzare e configurare correttamente il progetto, segui questi passaggi:

### 1. Configurazione dei File e Attributi

- **Configura `appsettings.json`**: Modifica questo file per impostare i parametri di configurazione, come le credenziali del database e altre impostazioni dell'applicazione.

  ```json
  {
    "DB_SERVER": "localhost",
    "DB_NAME": "miodatabase",
    "DB_USER": "utente",
    "DB_PASSWORD": "password"
  }
  ```

- **Imposta Custom Attributes**: Modifica i Custom Attributes sopra il metodo `Execute` per definire azione, package di OpenWhisk e route di APISIX. Ad esempio:

  ```csharp
  [Action("NomeAzione")]
  [Package("NomePackage")]
  [Route("/percorso/api")]
  public class MyMicroservicesOpenWhiskExecutor
  {
      // ...
  }
  ```

### 2. Personalizzazione del DTO e dei Verbi HTTP

- **Modifica il DTO**: Adatta il DTO per corrispondere allo schema della tua tabella nel database. Ad esempio, per aggiungere un nuovo campo:

  ```csharp
  public class FncDto
  {
      public Guid Id { get; set; }
      public string? NuovoCampo { get; set; }
      // Altri campi...
  }
  ```

- **Personalizza gli Handle dei Verbi HTTP**: Modifica gli handle per adattarli alle tue esigenze specifiche. Ad esempio, per cambiare la logica dell'operazione GET:

  ```csharp
  public static async Task<JObject> HandleGet(Guid? id, FncDtoRepository repository)
  {
      // Logica personalizzata per GET
  }
  ```

### 3. Deploy della Funzione

- **Esegui `build.sh`**: Questo script automatizza il processo di deploy della funzione in OpenWhisk.

  ```bash
  ./build.sh
  ```

### 4. Test della Funzione

- **Utilizzo del Plugin di VSCode per OpenWhisk**: Dopo il deploy, puoi testare la funzione direttamente da VSCode utilizzando il plugin di OpenWhisk. Questo ti permette di inviare richieste e visualizzare le risposte direttamente dall'IDE.

## Contribuire

Le contribuzioni sono benvenute. Si prega di aprire una issue o una pull request per suggerimenti o miglioramenti.

## Licenza

Questo progetto è rilasciato sotto la licenza Apache 2.0.
