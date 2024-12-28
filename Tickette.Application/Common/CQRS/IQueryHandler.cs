namespace Tickette.Application.Common.CQRS;

public interface IQueryHandler<in TQuery, TQueryResult> where TQuery : notnull
{
    Task<TQueryResult> Handle(TQuery query, CancellationToken cancellation);
}