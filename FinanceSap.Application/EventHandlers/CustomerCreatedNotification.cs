using MediatR;

namespace FinanceSap.Application.EventHandlers;

// Wrapper INotification — adapta o Domain event CustomerCreatedEvent para o MediatR.
// Mantém o Domain livre de dependências externas (Clean Architecture).
public sealed record CustomerCreatedNotification(Guid CustomerId, string FullName) : INotification;
