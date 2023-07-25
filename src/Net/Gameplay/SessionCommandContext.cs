using OpenCCG.Net.Messaging;

namespace OpenCCG.Net.Gameplay;

public record SessionCommandContext(MessageContext MessageContext, SessionCommand Command, PlayerState PlayerState);
