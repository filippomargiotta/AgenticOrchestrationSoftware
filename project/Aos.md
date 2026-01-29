# AOS Project Handoff

Date: 2026-01-29
Repo: /Users/filippomargiotta/Documents/Personal/AgenticOrchestrationSoftware

## Month 1 status (current)
Done:
- Added Aos.WebApi.Tests with xUnit v3, smoke test, golden test skeleton
- Added event log writer service (JSONL) + options + models
- Added manifest + event log schema models
- Replaced WeatherForecast with POST /workflow/hello that emits manifest + writes event log entry
- Wired Serilog (console) + OpenTelemetry tracing (OTLP exporter)
- Added Scalar UI and Swagger (Swashbuckle) + launch settings open Swagger
- Cleaned Rider caches; removed SonarQube MSBuild ImportBefore targets

Key files:
- source/Aos/Aos.WebApi/Controllers/WorkflowController.cs
- source/Aos/Aos.WebApi/Models/Manifest.cs
- source/Aos/Aos.WebApi/Models/EventLogEntry.cs
- source/Aos/Aos.WebApi/Models/EventLogSchema.cs
- source/Aos/Aos.WebApi/Services/FileEventLogWriter.cs
- source/Aos/Aos.WebApi/Options/EventLogOptions.cs
- source/Aos/Aos.WebApi/Program.cs
- source/Aos/Aos.WebApi/Aos.WebApi.csproj
- source/Aos/Aos.WebApi/Properties/launchSettings.json
- source/Aos/Aos.WebApi.Tests/Aos.WebApi.Tests.csproj
- source/Aos/Aos.WebApi.Tests/GoldenTests.cs

How to run:
- dotnet restore source/Aos/Aos.sln
- dotnet run --project source/Aos/Aos.WebApi/Aos.WebApi.csproj
- POST http://localhost:5057/workflow/hello
- Event log output: source/Aos/Aos.WebApi/data/eventlog.jsonl
- Swagger UI: http://localhost:5057/swagger
- Scalar UI (dev): http://localhost:5057/scalar

## Month 1 remaining (if any)
- CI pipeline file (optional for Month 1)
- Replace placeholder determinism fields in manifest (when determinism contract is defined)

## Month 2 suggested next steps
- Determinism context (seed locking + time source injection)
- Deterministic log/manifest replay CLI
- Golden test harness wired to real output
- Replace placeholder policies/tools/models in manifest

Notes:
- Swashbuckle pinned to 10.0.0 for net10 compatibility
- OpenTelemetry packages at 1.15.0
