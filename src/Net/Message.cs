namespace OpenCCG.Net;

public record Message<T>(string Id, T Type, string? Json = null);