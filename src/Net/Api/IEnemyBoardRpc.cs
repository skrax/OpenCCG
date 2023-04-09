namespace OpenCCG.Net.Api;

public interface IEnemyBoardRpc
{
    void PlaceCard(string cardGameStateJson);

    void UpdateCard(string cardGameStateUpdateJson);

    void RemoveCard(string id);
}