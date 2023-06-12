namespace OpenCCG.Net.Rpc;

public record Message<T>(string Id, T Type, string? Json = null);