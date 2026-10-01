using RepCortex.Application.Abstractions.Context;

namespace RepCortex.API;

public sealed class RequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    public string RemoteIpAddress =>
        accessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
}
