namespace CheckpointProdutos.Repositories;

public class RepositorioException : Exception
{
    public RepositorioException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
