namespace OpenCCG.Net.Api;

public interface IStatusPanelRpc
{
    void SetEnergy(int value);
    
    void SetCardCount(int value);

    void SetHealth(int value);
}