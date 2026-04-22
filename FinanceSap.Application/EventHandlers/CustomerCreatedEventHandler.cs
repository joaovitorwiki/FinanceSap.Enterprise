using FinanceSap.Application.Commands.CreateAccount;
using MediatR;

namespace FinanceSap.Application.EventHandlers;

// Event Handler — CustomerCreated.
// Escuta CustomerCreatedNotification e dispara CreateAccountCommand automaticamente.
// Side-effect: toda criação de Customer resulta em criação de Account.
public sealed class CustomerCreatedEventHandler(IMediator mediator)
    : INotificationHandler<CustomerCreatedNotification>
{
    public async Task Handle(CustomerCreatedNotification notification, CancellationToken ct)
    {
        // Dispara o comando de criação de conta de forma assíncrona.
        // Se falhar, o erro é logado mas não impede a criação do Customer (eventual consistency).
        var command = new CreateAccountCommand(notification.CustomerId);
        await mediator.Send(command, ct);
    }
}
