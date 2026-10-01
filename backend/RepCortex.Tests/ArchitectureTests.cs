using RepCortex.Application.Services;
using RepCortex.Domain.Entities;
using RepCortex.Infrastructure.Data;
using Xunit;

namespace RepCortex.Tests;

public class ArchitectureTests
{
    [Fact]
    public void DominioNaoDependeDeCamadasExternas()
    {
        var referencias = typeof(Tenant).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain(referencias, a => a!.StartsWith("RepCortex.") && a != "RepCortex.Domain");
        Assert.DoesNotContain(referencias, a => a!.StartsWith("Microsoft.AspNetCore") || a.StartsWith("Microsoft.EntityFrameworkCore"));
    }

    [Fact]
    public void AplicacaoDependeSomenteDoDominio()
    {
        var referencias = typeof(AvaliacaoService).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain(referencias, a => a is "RepCortex.Infrastructure" or "RepCortex.API");
        Assert.DoesNotContain(referencias, a => a!.StartsWith("Microsoft.AspNetCore") || a.StartsWith("Microsoft.EntityFrameworkCore"));
        Assert.Contains("RepCortex.Domain", referencias);
    }

    [Fact]
    public void InfrastructureImplementaPortasDaAplicacao()
    {
        var referencias = typeof(AppDbContext).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.Contains("RepCortex.Application", referencias);
        Assert.Contains("RepCortex.Domain", referencias);
    }
}
