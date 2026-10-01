namespace RepCortex.Application.Abstractions.Context;

public interface IRequestContext
{
    string RemoteIpAddress { get; }
}
