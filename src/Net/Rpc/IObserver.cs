using System;

namespace OpenCCG.Net.Rpc;

public interface IObserver : IDisposable
{
    void Handle(string json);
}