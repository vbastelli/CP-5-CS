using CheckpointProdutos.Models;
using CheckpointProdutos.Services;
using Xunit;

namespace CheckpointProdutos.Tests;

public class ProdutoValidadorTests
{
    [Fact]
    public void Validar_ProdutoValido_NaoRetornaErros()
    {
        var produto = new Produto
        {
            Nome = "Caderno",
            Preco = 24.90m,
            Estoque = 10,
            Categoria = "Papelaria"
        };

        var erros = ProdutoValidador.Validar(produto);

        Assert.Empty(erros);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Validar_PrecoNegativo_RetornaErro(double preco)
    {
        var produto = CriarProdutoValido();
        produto.Preco = (decimal)preco;

        var erros = ProdutoValidador.Validar(produto);

        Assert.Contains(erros, erro => erro.Contains("preço", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_EstoqueNegativo_RetornaErro()
    {
        var produto = CriarProdutoValido();
        produto.Estoque = -1;

        var erros = ProdutoValidador.Validar(produto);

        Assert.Contains(erros, erro => erro.Contains("estoque", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_NomeVazio_RetornaErro(string nome)
    {
        var produto = CriarProdutoValido();
        produto.Nome = nome;

        var erros = ProdutoValidador.Validar(produto);

        Assert.Contains(erros, erro => erro.Contains("nome", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_CategoriaVazia_RetornaErro()
    {
        var produto = CriarProdutoValido();
        produto.Categoria = string.Empty;

        var erros = ProdutoValidador.Validar(produto);

        Assert.Contains(erros, erro => erro.Contains("categoria", StringComparison.OrdinalIgnoreCase));
    }

    private static Produto CriarProdutoValido()
    {
        return new Produto
        {
            Nome = "Produto de teste",
            Preco = 10m,
            Estoque = 1,
            Categoria = "Testes"
        };
    }
}
