using CheckpointProdutos.Models;

namespace CheckpointProdutos.Services;

public static class ProdutoValidador
{
    public static IReadOnlyList<string> Validar(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);

        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            erros.Add("O nome é obrigatório.");
        }
        else if (produto.Nome.Trim().Length > 100)
        {
            erros.Add("O nome deve ter no máximo 100 caracteres.");
        }

        if (produto.Preco < 0)
        {
            erros.Add("O preço deve ser maior ou igual a zero.");
        }
        else if (produto.Preco > 99_999_999.99m)
        {
            erros.Add("O preço deve ser compatível com DECIMAL(10,2).");
        }

        if (produto.Estoque < 0)
        {
            erros.Add("O estoque deve ser maior ou igual a zero.");
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria))
        {
            erros.Add("A categoria é obrigatória.");
        }
        else if (produto.Categoria.Trim().Length > 60)
        {
            erros.Add("A categoria deve ter no máximo 60 caracteres.");
        }

        return erros;
    }
}
