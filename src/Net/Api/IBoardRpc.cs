namespace OpenCCG.Net.Api;

public interface IBoardRpc
{
    void PlaceCard(string cardGameStateDtoJson);

    void UpdateCard(string cardGameStateDtoJson);

    void RemoveCard(string id);
}