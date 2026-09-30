using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Profiles;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record GetPromptCatalogQuery : IQuery<IReadOnlyList<PromptCatalogItemDto>>;

internal sealed class GetPromptCatalogHandler : IRequestHandler<GetPromptCatalogQuery, IReadOnlyList<PromptCatalogItemDto>>
{
    public Task<IReadOnlyList<PromptCatalogItemDto>> Handle(GetPromptCatalogQuery q, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PromptCatalogItemDto>>(
            PromptCatalog.Texts.Select(kv => new PromptCatalogItemDto(kv.Key, kv.Value)).ToList());
}
