# Tasky - Task Management Platform

## Overview
A production-grade Task Management Platform built with .NET 10, Orleans, Kafka, Redis, and React.

## Tech Stack
- **Backend**: .NET 10, Orleans, Kafka, Redis, EF Core, SQL Server.
- **Frontend**: React, Vite, TypeScript, Mantine UI, SignalR.
- **Infrastructure**: Docker Compose.

## Prerequisites
- .NET 10 SDK
- Node.js & npm
- Docker Desktop

## Getting Started

### 1. Start Infrastructure
```bash
docker-compose up -d
```

### 2. Run Backend
```bash
cd src/Tasky.Api
dotnet run
```
(Or open `Tasky.sln` in Visual Studio/Rider)

### 3. Run Frontend
```bash
cd frontend/tasky-web
npm install
npm run dev
```

## Architecture
- **Clean Architecture**: Domain, Application, Infrastructure, Api.
- **Microservices-ready**: Orleans for state management, Kafka for async messaging.
- **Real-time**: SignalR for live updates.
