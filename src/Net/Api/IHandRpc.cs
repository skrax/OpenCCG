namespace OpenCCG.Net.Api;

public interface IHandRpc
{
    void DrawCard(string cardGameStateJson);

    void RemoveCard(string id);
}