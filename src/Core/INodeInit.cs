namespace OpenCCG.Core;

public interface INodeInit<in TParam>
{
    void Init(TParam outline);
}