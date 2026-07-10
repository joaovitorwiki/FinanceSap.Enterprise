using MediatR;

namespace FinanceSap.Domain.Common;

/// <summary>
/// Base class para todas as entidades do domínio.
/// Fornece propriedades comuns e suporte a Domain Events (extensibilidade futura).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Data de criação da entidade (UTC).
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Data da última atualização (UTC). Null se nunca foi atualizada.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    // ── Domain Events (Extensibility Point) ──────────────────────────────────
    // Lista de eventos de domínio que serão disparados após a persistência.
    // Permite implementar Event Sourcing ou Event-Driven Architecture no futuro.
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Eventos de domínio pendentes de publicação.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adiciona um evento de domínio à lista de eventos pendentes.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpa todos os eventos de domínio pendentes.
    /// Chamado pelo UnitOfWork após a publicação dos eventos.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marca a entidade como atualizada (timestamp).
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Marker interface para eventos de domínio.
/// Permite identificar eventos de domínio no sistema de forma polimórfica.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
