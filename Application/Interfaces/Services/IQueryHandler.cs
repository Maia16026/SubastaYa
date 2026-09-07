namespace Application.Interfaces.Services;

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> Handle(
        TQuery query,
        CancellationToken ct = default);
}