# Azure Service Bus Version (Managed Identity)

This backend is the Azure Service Bus port of the RabbitMQ POC. Only the transport changed:
`MassTransit.RabbitMQ` -> `MassTransit.Azure.ServiceBus.Core` + `Azure.Identity`.
All consumers, outbox configuration, endpoints, and the Blazor front end are unchanged.

## Concept mapping (RabbitMQ -> Azure Service Bus)

| RabbitMQ | Azure Service Bus |
|---|---|
| Fanout exchange per message type | Topic per message type |
| Queue bound to exchange | Queue + topic Subscription forwarding into it |
| `{queue}_error` queue | `{queue}_error` queue (MassTransit) + built-in dead-letter sub-queues |
| Management UI :15672 | Azure Portal / Service Bus Explorer |
| Broker clustering | PaaS - Microsoft runs the replication |

MassTransit keeps the same endpoint names, so the topology is recognizable:
one topic per event type, one subscription + queue per consumer service.

## Azure setup (one time)

1. **Create a Service Bus namespace** - **Standard tier or higher** (Basic has no topics;
   MassTransit publish/subscribe requires topics).
2. **Grant roles** on the namespace (IAM -> Add role assignment):
   - `Azure Service Bus Data Owner` to the identity running the services.
     Data Owner (not just Sender/Receiver) is required because MassTransit
     **creates the topics/queues/subscriptions** on startup.
   - For local development: assign the role to **your own Azure AD user**.
   - For Azure hosting (App Service / Container Apps / VM / AKS): enable the
     **system-assigned managed identity** on each service and assign the role to it.
3. **Configure** each service's `appsettings.json`:
   ```json
   "AzureServiceBus": {
     "Namespace": "sb://<your-namespace>.servicebus.windows.net",
     "ConnectionString": ""
   }
   ```
   Leave `ConnectionString` empty to use `DefaultAzureCredential` (managed identity in
   Azure; VS / Azure CLI / VS Code sign-in locally - run `az login` once).

## How auth resolves (DefaultAzureCredential chain)

In Azure: environment -> **managed identity** (IMDS). Locally: Visual Studio ->
Azure CLI -> Azure PowerShell sign-in. Same code, no secrets anywhere - that is the
whole point of managed identity: no connection strings to rotate, leak, or commit.

## Local alternative: Service Bus emulator

Microsoft ships a Service Bus emulator (Docker). It does **not** support Entra ID /
managed identity - only its fixed SAS connection string - which is why the code keeps a
connection-string branch. Put the emulator's connection string into
`AzureServiceBus:ConnectionString` and it takes precedence. Note the emulator requires
queues/topics pre-declared in its config file, so MassTransit's auto-topology is limited
there; for a first run, a real Standard namespace is less friction.

## Things that behave differently vs RabbitMQ

- **Startup now needs valid Azure credentials** - services fail fast if the identity
  lacks permissions (`UnauthorizedAccessException` -> check role assignment; roles can
  take a few minutes to propagate).
- **First startup is slower** - MassTransit provisions topics/subscriptions via ARM-ish
  management operations.
- **No docker-compose rabbitmq container needed** - remove it; keep SQL Server + Papercut.
- **Message browsing** - use Service Bus Explorer (portal) to peek queues, including
  dead-letter sub-queues, similar to the RabbitMQ "Get messages" workflow.
