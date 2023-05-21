namespace OpenCCG.Data;

public interface ICardEventArgs
{
}

public class PlayedEventArgs : ICardEventArgs
{
}

public interface ICardEffect
{
    public string Id { get; }

    public string Text { get; }

    public void Handle(ICardEventArgs args);
}

public class DealDamageCardEffect : ICardEffect
{
    public string Id => "TEST-E-001";

    public string Text => $"Deal {Damage} damage";

    public int Damage { get; }

    public DealDamageCardEffect(int damage)
    {
        Damage = damage;
    }

    public void Handle(ICardEventArgs args)
    {
        throw new System.NotImplementedException();
    }
}