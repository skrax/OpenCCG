namespace OpenCCG.Net.Api;

public interface IMainRpc
{
    void PlayCard(string id);
    
    void CombatPlayerCard(string attackerId, string targetId);
}