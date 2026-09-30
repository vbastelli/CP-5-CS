using System.Text;

namespace CheckpointProdutos.Services;

public class ArquivoLogger
{
    private static readonly object Bloqueio = new();
    private readonly string _diretorioLogs;

    public ArquivoLogger(string? diretorioLogs = null)
    {
        _diretorioLogs = diretorioLogs ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    }

    public void Registrar(string operacao, string mensagem)
    {
        try
        {
            Directory.CreateDirectory(_diretorioLogs);
            var caminho = Path.Combine(_diretorioLogs, $"produtos-{DateTime.Now:yyyy-MM-dd}.log");
            var linha = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] [{operacao}] {mensagem}{Environment.NewLine}";

            lock (Bloqueio)
            {
                File.AppendAllText(caminho, linha, Encoding.UTF8);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Não foi possível gravar o log: {ex.Message}");
        }
    }
}
