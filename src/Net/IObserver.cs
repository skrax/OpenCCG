using System;

namespace OpenCCG.Net;

public interface IObserver : IDisposable
{
    void Handle(string json);
}