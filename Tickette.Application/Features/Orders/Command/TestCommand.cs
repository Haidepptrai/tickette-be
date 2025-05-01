using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command;

public record TestCommand
{

}

public class TestCommandHandler : ICommandHandler<TestCommand, Unit>
{
    private readonly IMessageRequestClient _requestClient;

    public TestCommandHandler(IMessageRequestClient requestClient)
    {
        _requestClient = requestClient;
    }

    public async Task<Unit> Handle(TestCommand command, CancellationToken cancellation)
    {
        await _requestClient.PublishAsync<TestCommand, RedisReservationResult>(command, cancellation);

        return Unit.Value;
    }
}