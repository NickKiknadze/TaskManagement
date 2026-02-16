# Architecture Notes

## Overview
This document outlines the architectural decisions and patterns used in the Tasky platform.

## Architecture Style
We follow **Clean Architecture** principles to ensure separation of concerns and testability.

### Layers
1. **Domain**: Contains enterprise logic and entities. No dependencies.
2. **Application**: Contains business logic, use cases (CQRS via MediatR), and interfaces. Depends on Domain.
3. **Infrastructure**: Implements interfaces (Persistence, External Services). Depends on Application and Domain.
4. **Api**: Entry point (REST/SignalR). Hosting layer. Depends on Application and Infrastructure.
5. **Worker**: Background service for async processing (Kafka Consumer).

## Key Technologies & Patterns

### Orleans (State & Coordination)
- **Role**: Used as a distributed cache and coordinator for active entities (Grains).
- **Grains**: `TaskGrain` forces consistency for task updates.
- **Hosting**: Co-hosted in the API project for simplicity in this MVP, but separable.
- **Persistence**: Currently uses explicit read-through pattern (loading from DB/Redis) rather than internal Orleans persistence, giving us fine-grained control over the "Source of Truth" (SQL).

### Redis (Caching)
- **Role**: Shared cache for high-frequency reads (Tags, Statuses, User Profiles).
- **Pattern**: Read-through. Services check Redis -> DB -> Populate Redis.
- **Service**: `IRedisService` wraps `StackExchange.Redis` for typed access.

### Kafka (Async Messaging)
- **Role**: Decouples write operations from notifications and side effects.
- **Flow**: API writes to DB -> Produces Event -> Worker Consumes Event -> Calls Grain/SignalR.
- **Benefit**: API remains responsive; heavy lifting (notifications, analytics) happens in background.

### SignalR (Real-time)
- **Role**: Pushes updates to connected clients.
- **Hubs**: `TaskHub` manages groups per task (`task:{id}`).
- **Backplane**: In a multi-instance scenario, we would use Redis backplane. For this MVP, single instance assumed or Orleans grain handles notification logic. (Current impl: Worker calls Grain -> Grain calls Notifier -> Notifier calls HubContext).

## Security
- **Auth**: JWT (Stateless). Permissions embedded in token or resolved via caching.
- **RBAC**: Policy-based authorization in API (`RequireClaim`).

## Scaling
- **Stateless API**: Can scale horizontally.
- **Orleans**: Handles distributed state scaling.
- **Kafka**: Handles message throughput.
