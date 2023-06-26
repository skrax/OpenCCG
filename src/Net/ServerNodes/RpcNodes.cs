namespace OpenCCG.Net.ServerNodes;

public class RpcNodes
{
    public Server Server { get; init; }

    public Hand Hand { get; init; }

    public EnemyHand EnemyHand { get; init; }

    public Board Board { get; init; }

    public Board EnemyBoard { get; init; }

    public StatusPanel StatusPanel { get; init; }

    public StatusPanel EnemyStatusPanel { get; init; }

    public MidPanel MidPanel { get; init; }

    public CardEffectPreview CardEffectPreview { get; init; }

    public CardEffectPreview EnemyCardEffectPreview { get; init; }
}