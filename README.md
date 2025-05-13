# Tickette Backend (Web API)

Tickette is a ticket-selling website that enables users to create events and sell tickets to other users. This README is intended for running the Backend Web API only. To make it work correctly, please start the backend (.NET Web API) first.

This project is fully integrated with Docker and supports both containerized and non-containerized development workflows.


This backend contains two projects:

* **Tickette.API** – Public-facing endpoints for ticket buyers and event creators
* **Tickette.Admin** – Internal endpoints for moderators and admin-specific functionality

And other class libraries following the Clean Architecture and CQRS Pattern

> Project maintained by **Nguyen Hoang Hai** (Vincent Nguyen)

## ✨ Core Features of the application

* Event creation, updating, and approval workflows
* Ticket ordering, purchasing, and QR code generation
* Seat reservation and validation logic
* Role-based access control for event owners, staff, and admins
* User authentication and session handling (via JWT & NextAuth integration)
* Order tracking and history
* Coupon code and discount handling
* Admin audit logging
* Real-time communication endpoints for support chat (SignalR-based)
* Redis-based caching and distributed locking
* RabbitMQ-based messaging for async operations

## 🔐 Required User Secrets

Sensitive values must be stored in **User Secrets**, not in `appsettings.json`. This prevents secrets from leaking into source control.

Use the following command to initialize:

```bash
dotnet user-secrets init
```

Then set values using `dotnet user-secrets set` or your editor of choice.

<details>
<summary>Expected Keys in User Secrets</summary>

```json
{
  "Stripe:SecretKey": "<used for processing ticket payments>",
  "QrCodeSecretKey": "<used to encrypt and validate QR check-in codes>",
  "Jwt:Key": "<used to sign and validate authentication tokens>",
  "Jwt:Issuer": "http://localhost:5031",
  "Jwt:Audience": "http://localhost:3000",
  "EmailSettings:UseTls": "True",
  "EmailSettings:UseSsl": "False",
  "EmailSettings:UnsubscribeSecretKey": "<used for unsubscribe links>",
  "EmailSettings:SmtpServer": "smtp.gmail.com",
  "EmailSettings:SmtpPort": "587",
  "EmailSettings:SenderPassword": "ldfb ifdy sqcf csco",
  "EmailSettings:SenderName": "Tickette",
  "EmailSettings:SenderEmail": "<from email address>",
  "EmailSettings:ClientUrl": "http://localhost:3000",
  "EmailSettings:BaseUrl": "http://localhost:5031",
  "AWS:SecretKey": "<AWS credentials>",
  "AWS:S3:BucketName": "<S3 bucket name>",
  "AWS:Region": "<AWS region>",
  "AWS:Profile": "<AWS CLI profile>",
  "AWS:AccessKey": "<AWS access key>"
}
```

</details>

> ✅ These are required to support payment, mailing, auth, QR, and cloud operations.

## ⚙️ Explanation of `appsettings.json`

The `appsettings.json` contains default configurations for local or Docker-based development. These values are safe to check into source control:

```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq",       // Docker internal hostname
    "Port": "5672",
    "Username": "guest",
    "Password": "guest"
  },
  "Redis": {
    "ConnectionString": "redis:6379",  // Docker internal Redis address
    "InstanceName": "Tickette:",
    "DefaultExpirationMinutes": 15,
    "TicketCacheExpirationMinutes": 15,
    "AgentCacheExpirationMinutes": 15,
    "Host": "redis",                   // Redis Docker hostname
    "Port": 6379,
    "DefaultDatabase": 0,
    "AbortOnConnectFail": false
  }
}
```

> 📌 These hostnames only work inside Docker containers. If you're running the app locally, you must replace `rabbitmq` and `redis` with `localhost`.

## 🚀 Running with Docker

Before you start the services, you must first create the shared Docker network if it doesn't already exist:

```bash
docker network create tickette-network
```

Then to run the backend services in a production-simulated environment using Docker Compose:

```bash
docker compose up --build -d
```

This command will start:

* Tickette Web API
* Tickette Admin API
* PostgreSQL
* Redis
* RabbitMQ

> 🛑 Do **not** expose this project without configuring secure credentials in user secrets.

> [!NOTE]
> 🗃️ Inserting Dummy Data into Docker PostgreSQL
<details>
  <summary>
    If you prefer to have dummy data in the PostgreSQL Docker when the application runs, follow the steps below after run Docker command
  </summary>
  
  **Instructions**:
  
  1. Ensure your PostgreSQL container is running. By default, the container is named:
  ```bash
  local-database
  ```
  
  2. Open a terminal session into the container:
  ```bash
  docker exec -it local-database bash
   ```
  
  3. Access the PostgreSQL CLI inside the container:
  ```bash
  psql -U HaiNguyen -d tickette-db
  ```

  4. Once inside the psql prompt, you can manually execute SQL statements or import a file. To run an SQL file that you’ve mounted into the container (e.g. init.sql placed in /init-scripts):
  ```bash
  \i /docker-entrypoint-initdb.d/init.sql
  ```

  5. Verify the inserted data using standard SQL queries `(e.g., SELECT * FROM "events";)`.
  
  🔐 This method is useful when the container already exists and you do not want to recreate it from scratch.
</details>

## 🖥️ Running Without Docker

If you want to run the backend locally:

1. Make sure you have PostgreSQL, Redis, and RabbitMQ installed and running
2. Change relevant hostnames in `appsettings.Development.json`:

   * `rabbitmq` → `localhost`
   * `redis` → `localhost`
3. Then start the project manually:

```bash
dotnet run --project Tickette.API
```

Or for admin API:

```bash
dotnet run --project Tickette.Admin
```

## 📌 Notes

* Use Docker if you're unsure — all services are pre-wired to work within the Docker network.
* Make sure User Secrets are populated with valid keys, especially for mail and JWT.
* This backend is designed to be consumed by both the Tickette Web Client and Tickette Admin Client.

## ⚖️ License

This project is licensed under the **Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License**.

This means you may use, share, and study the code **only for non-commercial and educational purposes**. You may not modify, redistribute, or use this work for profit or harmful intent.

Read the full license here:  
[https://creativecommons.org/licenses/by-nc-nd/4.0/](https://creativecommons.org/licenses/by-nc-nd/4.0/)

> Maintained by **Nguyen Hoang Hai** (Vincent Nguyen)
