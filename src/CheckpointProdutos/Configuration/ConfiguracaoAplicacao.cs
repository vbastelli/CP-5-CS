using System.Text.Json;

namespace CheckpointProdutos.Configuration;

public static class ConfiguracaoAplicacao
{
    public static string ObterConnectionString()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(caminho))
        {
            throw new FileNotFoundException("O arquivo appsettings.json não foi encontrado.", caminho);
        }

        using var documento = JsonDocument.Parse(File.ReadAllText(caminho));

        if (!documento.RootElement.TryGetProperty("ConnectionStrings", out var secoes) ||
            !secoes.TryGetProperty("DefaultConnection", out var valor) ||
            string.IsNullOrWhiteSpace(valor.GetString()))
        {
            throw new InvalidOperationException(
                "A connection string 'ConnectionStrings:DefaultConnection' não foi configurada.");
        }

        return valor.GetString()!;
    }
}
