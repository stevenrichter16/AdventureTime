using AdventureTime.Application.Models;
using MediatR;

namespace AdventureTime.Application.Queries.Episodes.GetEpisodeByIdQuery;

/// <summary>
/// A query represents a request for data - it doesn't change anything.
/// Think of it as asking a question rather than giving an order.
/// </summary>
public class GetEpisodeByIdQuery : IRequest<Episode?>
{
    public int Id { get; init; }
}