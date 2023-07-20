namespace OpenCCG.Net.Messaging;

public record Error(ErrorCode Code, string? Message)
{
    public static Error FromCode(ErrorCode code) => new(code, null);
}