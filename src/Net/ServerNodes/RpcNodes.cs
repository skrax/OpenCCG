namespace OpenCCG.Net.ServerNodes;

public class RpcNodes
{
    public Hand Hand { get; init; }

    public EnemyHand EnemyHand { get; init; }

    public Board Board { get; init; }

    public EnemyBoard EnemyBoard { get; init; }

    public StatusPanel StatusPanel { get; init; }

    public StatusPanel EnemyStatusPanel { get; init; }
    
    public MidPanel MidPanel { get; init; }
}