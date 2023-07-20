namespace OpenCCG.Net.Gameplay;

public interface IPlayerState
{
    public long PeerId { get; }

    public void PlayCard();

    public void CombatPlayer();

    public void CombatPlayerCard();

    public void EndTurn();
}

public class PlayerState : IPlayerState
{
    public PlayerState(long peerId)
    {
        PeerId = peerId;
    }

    public long PeerId { get; }

    public void PlayCard()
    {
        throw new System.NotImplementedException();
    }

    public void CombatPlayer()
    {
        throw new System.NotImplementedException();
    }

    public void CombatPlayerCard()
    {
        throw new System.NotImplementedException();
    }

    public void EndTurn()
    {
        throw new System.NotImplementedException();
    }
}