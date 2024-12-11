using Microsoft.Extensions.DependencyInjection;
using Tickette.Application.Common.CQRS;

namespace Tickette.Infrastructure.CQRS;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QueryDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
        return await handler.Handle(query, cancellationToken);
    }
}

