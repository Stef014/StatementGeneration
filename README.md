# StatementGeneration

A distributed system for generating and delivering monthly account statements to bank customers.

## System Overview

### AccountsStatementsData

A shared data access layer built with Entity Framework Core 8.0 that provides ORM-based database management for the statement generation system. It defines the database schema for bank accounts and their associated statements, including entity configurations, migrations, and the PostgreSQL-backed DbContext. This project is referenced by other services that need to interact with the accounts and statements database.

### MonthlyAccountProcessingService

A background job orchestrator that triggers monthly statement generation for all accounts. Running as a hosted service with Hangfire for job scheduling, it queries all accounts from the PostgreSQL database, calculates the previous month's date range and creates encrypted statement generation requests for each account. These requests are batched and pushed to a Redis queue using a producer-consumer pattern with bounded channels for efficient memory management and backpressure handling.

### StatementGenerationService

A background worker service that processes statement generation requests from the Redis queue. For each request, it fetches the account's transactions from DynamoDB, generates a PDF statement using QuestPDF with transaction details and balance, uploads the PDF to AWS S3, persists statement metadata to PostgreSQL and sends an email with a pre-signed download link to the account holder via AWS SES. This service assumes that transaction data has already been pushed via a queue to DynamoDb.

---

## Inputs

### MonthlyAccountProcessingService

| Input           | Schema                                                                                                          | Source                                                         |
| --------------- | --------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Account Records | `AccountId (Guid)`, `AccountHolderName (string)`, `AccountHolderEmailAddress (string)`, `ClosingBalance (long)` | PostgreSQL `Accounts` table via Entity Framework Core          |
| Time Period     | Calculated from `DateTimeOffset.UtcNow`                                                                         | Previous calendar month (start/end timestamps in milliseconds) |

### StatementGenerationService

| Input               | Schema                                                                                                                                                          | Source                            |
| ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| Statement Request   | `AccountId (Guid)`, `AccountHolderName (string)`, `AccountBalance (long)`, `AccountHolderEmailAddress (string)`, `StartTimestamp (long)`, `EndTimestamp (long)` | Redis queue (AES encrypted JSON)  |
| Transaction Records | `AccountId`, `TransactionTimestamp`, `Description`, `Category`, `Amount`, `Direction`                                                                           | AWS DynamoDB `Transactions` table |

---

## Outputs

### MonthlyAccountProcessingService

| Output                       | Format                            | Destination                                      |
| ---------------------------- | --------------------------------- | ------------------------------------------------ |
| Encrypted Statement Requests | Base64-encoded AES-encrypted JSON | Redis List (`monthly-statement-proccessing` key) |

### StatementGenerationService

| Output             | Format                                                                      | Destination                                |
| ------------------ | --------------------------------------------------------------------------- | ------------------------------------------ |
| PDF Statement      | `{AccountId}_{Date:yyyyMMdd}_Statement.pdf`                                 | AWS S3 bucket (`capitec-statement-bucket`) |
| Pre-signed URL     | HTTPS URL (24-hour expiration)                                              | Generated from S3                          |
| Statement Metadata | `StatementId`, `AccountId`, `StartTimestamp`, `EndTimestamp`, `DownloadUrl` | PostgreSQL `Statements` table              |
| Email Notification | HTML email with download link                                               | AWS SES to account holder's email          |

---

## Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) and Docker Compose
- (Optional) PostgreSQL client for database inspection

### Quick Start with Docker Compose

The easiest way to run the entire system is using Docker Compose, which sets up all infrastructure and services:

```bash
# Clone the repository and navigate to the solution root
cd StatementGeneration

# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

This starts:

- **PostgreSQL** (port 5432) - Databases: `accounts_statements`, `hangfire`
- **Redis** (port 6379) - Message queue
- **LocalStack** (port 4566) - AWS S3, SES, DynamoDB emulation
- **MonthlyAccountProcessingService** - Job scheduler
- **StatementGenerationService** - Statement processor

### Infrastructure Dependencies

#### PostgreSQL

Two databases are required:

- `accounts_statements` - Account and statement data
- `hangfire` - Hangfire job scheduling data

The `init-db.sql` script creates these automatically when using Docker Compose.

#### Redis

Used as a message queue between MonthlyAccountProcessingService and StatementGenerationService. Default connection: `localhost:6379`

#### AWS LocalStack (Local Development)

Emulates AWS services for local development:

- **S3** - Statement PDF storage
- **SES** - Email delivery
- **DynamoDB** - Transaction data

The `init-localstack.sh` script initializes:

- S3 bucket: `capitec-statement-bucket`
- Verified SES email: `noreply@capitecbank.co.za`
- DynamoDB `Statements` table (if needed)

### Local Development Setup

To run services locally without Docker:

1. **Start infrastructure services:**

   ```bash
   docker-compose up -d postgres redis localstack
   ```

2. **Apply database migrations:**

   ```bash
   cd AccountsStatementsDataSolution/AccountsStatementsData
   dotnet ef database update
   ```

3. **Run MonthlyAccountProcessingService:**

   ```bash
   cd MonthlyAccountProcessingService
   dotnet run
   ```

4. **Run StatementGenerationService:**
   ```bash
   cd StatementGenerationService
   dotnet run
   ```

### Configuration

Both services use `appsettings.json` and environment variables. Key settings:

#### MonthlyAccountProcessingService

```json
{
  "ConnectionStrings": {
    "AccountsStatementsConnection": "Host=localhost;Port=5432;Database=accounts_statements;Username=postgres;Password=postgres"
  },
  "Hangfire": {
    "ConnectionString": "Host=localhost;Port=5432;Database=hangfire;Username=postgres;Password=postgres",
    "RecurringJobId": "monthly-statement-proccessing",
    "CronExpression": "30 3 1 * *",
    "TimeZoneId": "Africa/Johannesburg"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "AccountQueueKey": "monthly-statement-proccessing"
  },
  "Encryption": {
    "Key": "<base64-encoded-32-byte-AES-key>"
  }
}
```

#### StatementGenerationService

```json
{
  "DynamoDbServer": "http://localhost:4566/",
  "AwsServiceUrl": "http://localhost:4566/",
  "AwsRegion": "us-east-1",
  "S3BucketName": "capitec-statement-bucket",
  "AwsAccessKey": "test",
  "AwsSecretKey": "test",
  "Redis": {
    "ConnectionString": "localhost:6379",
    "AccountQueueKey": "monthly-statement-proccessing"
  },
  "ConnectionStrings": {
    "AccountsStatementsConnection": "Host=localhost;Port=5432;Database=accounts_statements;Username=postgres;Password=postgres"
  },
  "Encryption": {
    "Key": "<base64-encoded-32-byte-AES-key>"
  }
}
```

> **Important:** The `Encryption:Key` must be identical in both services for encrypted message passing to work.

### Generating an Encryption Key

For production, generate a cryptographically secure 256-bit AES key:

```csharp
var key = new byte[32];
System.Security.Cryptography.RandomNumberGenerator.Fill(key);
Console.WriteLine(Convert.ToBase64String(key));
```

---

## Architecture

```
┌─────────────────────────┐      ┌─────────────────┐      ┌─────────────────────────┐
│ MonthlyAccountProcessing│      │     Redis       │      │ StatementGeneration     │
│        Service          │─────▶│     Queue       │─────▶│       Service           │
└───────────┬─────────────┘      └─────────────────┘      └───────────┬─────────────┘
            │                                                         │
            │ reads accounts                                          │ reads transactions
            ▼                                                         ▼
┌─────────────────────────┐                              ┌─────────────────────────┐
│      PostgreSQL         │                              │     AWS DynamoDB        │
│  (accounts_statements)  │◀─────────────────────────────│    (Transactions)       │
└─────────────────────────┘     writes statement         └─────────────────────────┘
                                    metadata
                                                         ┌─────────────────────────┐
                                                         │       AWS S3            │
                                                         │  (PDF storage)          │
                                                         └─────────────────────────┘
                                                                    │
                                                                    │ generates URL
                                                                    ▼
                                                         ┌─────────────────────────┐
                                                         │       AWS SES           │
                                                         │  (email delivery)       │
                                                         └─────────────────────────┘
```

## Testing

1. In order to test the system, it is reccommended to scaffold the AccountsStatementsDataSolution first and load account data.

2. Thereafter transactions need to be populated in the transactions database in DynamoDb.

3. Run the MonthlyAccountProcessingService and StatementGenerationService simultaneously, ensuring Redis and an instance of AWS localstack is running.

4. Account details should be pulled from both the Accounts- and Transactions databases and PDF's will be generated and saved to S3 with download links mailed to clients.

TAKE NOTE: In the event that Localstack is used, links aren't actually mailed but can be seen in the sent box of SES. In the case where real AWS SES was used, this would work differently. Also take note that the e-mail address used to send the statements should be verified with the SES instance.

---

## Contact Us

**Engineer:** Stefan Olivier  
**Email:** stef.o14@gmail.com
