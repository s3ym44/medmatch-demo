using MedMatch.Domain.Common;
using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Profiles;

public class ProfilePrompt : Entity
{
    public const int MaxAnswerLength = 200;

    public Guid ProfileId { get; private set; }
    public PromptKey PromptKey { get; private set; }
    public string Answer { get; private set; } = default!;
    public int Order { get; private set; }

    private ProfilePrompt() { }

    internal ProfilePrompt(Guid profileId, PromptKey promptKey, string answer, int order)
    {
        ProfileId = profileId;
        PromptKey = promptKey;
        Answer = answer;
        Order = order;
    }
}
