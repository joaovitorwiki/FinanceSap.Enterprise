namespace FinanceSap.Domain.Events;

// Domain Event — CustomerCreated.
// POCO puro — sem dependências externas (Clean Architecture).
// O wrapper INotification fica na camada Application.
public sealed record CustomerCreatedEvent(Guid CustomerId, string FullName);
