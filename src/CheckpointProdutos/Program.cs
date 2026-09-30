using System.Globalization;
using CheckpointProdutos.Configuration;
using CheckpointProdutos.Models;
using CheckpointProdutos.Repositories;
using CheckpointProdutos.Services;

namespace CheckpointProdutos;

internal static class Program
{
    private static readonly CultureInfo CulturaBrasileira = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly ArquivoLogger Logger = new();

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            var connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            var repositorio = new ProdutoRepository(connectionString, Logger);
            ExecutarMenu(repositorio);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or System.Text.Json.JsonException)
        {
            Logger.Registrar("ERRO", $"Falha de configuração: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erro de configuração: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void ExecutarMenu(ProdutoRepository repositorio)
    {
        while (true)
        {
            ExibirMenu();
            var entrada = Console.ReadLine();

            if (!int.TryParse(entrada, out var opcao) || opcao is < 1 or > 6)
            {
                ExibirAviso("Opção inválida. Digite um número de 1 a 6.");
                Pausar();
                continue;
            }

            Console.WriteLine();
            try
            {
                switch (opcao)
                {
                    case 1:
                        InserirProduto(repositorio);
                        break;
                    case 2:
                        ListarProdutos(repositorio);
                        break;
                    case 3:
                        BuscarProduto(repositorio);
                        break;
                    case 4:
                        AtualizarProduto(repositorio);
                        break;
                    case 5:
                        ExcluirProduto(repositorio);
                        break;
                    case 6:
                        Console.WriteLine("Programa encerrado. Até logo!");
                        return;
                }
            }
            catch (RepositorioException ex)
            {
                ExibirErro(ex.Message);
                ExibirAviso("Verifique se o LocalDB está iniciado e se o banco foi criado pelo script.");
            }
            catch (Exception ex)
            {
                Logger.Registrar("ERRO", $"Erro inesperado: {ex}");
                ExibirErro($"Ocorreu um erro inesperado: {ex.Message}");
            }

            Pausar();
        }
    }

    private static void InserirProduto(ProdutoRepository repositorio)
    {
        Console.WriteLine("=== Inserir produto ===");
        var produto = LerDadosProduto();
        if (produto is null)
        {
            return;
        }

        var id = repositorio.Inserir(produto);
        ExibirSucesso($"Produto inserido com sucesso! ID gerado: {id}.");
    }

    private static void ListarProdutos(ProdutoRepository repositorio)
    {
        Console.WriteLine("=== Lista de produtos ===");
        var produtos = repositorio.Listar();

        if (produtos.Count == 0)
        {
            ExibirAviso("Nenhum produto cadastrado.");
            return;
        }

        foreach (var produto in produtos)
        {
            ExibirProduto(produto);
        }

        Console.WriteLine($"Total: {produtos.Count} produto(s).");
    }

    private static void BuscarProduto(ProdutoRepository repositorio)
    {
        Console.WriteLine("=== Buscar produto ===");
        var id = LerInteiroPositivo("ID do produto: ");
        if (id is null)
        {
            return;
        }

        var produto = repositorio.BuscarPorId(id.Value);
        if (produto is null)
        {
            ExibirAviso($"Produto com ID {id} não foi encontrado.");
            return;
        }

        ExibirProduto(produto);
    }

    private static void AtualizarProduto(ProdutoRepository repositorio)
    {
        Console.WriteLine("=== Atualizar produto ===");
        var id = LerInteiroPositivo("ID do produto: ");
        if (id is null)
        {
            return;
        }

        var existente = repositorio.BuscarPorId(id.Value);
        if (existente is null)
        {
            ExibirAviso($"Produto com ID {id} não foi encontrado.");
            return;
        }

        Console.WriteLine("Produto atual:");
        ExibirProduto(existente);
        Console.WriteLine("Informe os novos dados:");

        var produto = LerDadosProduto();
        if (produto is null)
        {
            return;
        }

        produto.Id = id.Value;
        if (repositorio.Atualizar(produto))
        {
            ExibirSucesso("Produto atualizado com sucesso!");
        }
        else
        {
            ExibirAviso($"Produto com ID {id} não foi encontrado.");
        }
    }

    private static void ExcluirProduto(ProdutoRepository repositorio)
    {
        Console.WriteLine("=== Excluir produto ===");
        var id = LerInteiroPositivo("ID do produto: ");
        if (id is null)
        {
            return;
        }

        var produto = repositorio.BuscarPorId(id.Value);
        if (produto is null)
        {
            ExibirAviso($"Produto com ID {id} não foi encontrado.");
            return;
        }

        ExibirProduto(produto);
        Console.Write("Confirma a exclusão? (s/n): ");
        var confirmacao = Console.ReadLine()?.Trim();

        if (!string.Equals(confirmacao, "s", StringComparison.OrdinalIgnoreCase))
        {
            ExibirAviso("Exclusão cancelada.");
            return;
        }

        if (repositorio.Excluir(id.Value))
        {
            ExibirSucesso("Produto excluído com sucesso!");
        }
        else
        {
            ExibirAviso($"Produto com ID {id} não foi encontrado.");
        }
    }

    private static Produto? LerDadosProduto()
    {
        var nome = LerTextoObrigatorio("Nome: ", 100);
        if (nome is null)
        {
            return null;
        }

        var preco = LerDecimalNaoNegativo("Preço (ex.: 19,90): ");
        if (preco is null)
        {
            return null;
        }

        var estoque = LerInteiroNaoNegativo("Estoque: ");
        if (estoque is null)
        {
            return null;
        }

        var categoria = LerTextoObrigatorio("Categoria: ", 60);
        if (categoria is null)
        {
            return null;
        }

        var produto = new Produto
        {
            Nome = nome,
            Preco = preco.Value,
            Estoque = estoque.Value,
            Categoria = categoria
        };

        var erros = ProdutoValidador.Validar(produto);
        if (erros.Count > 0)
        {
            foreach (var erro in erros)
            {
                ExibirErro(erro);
            }

            return null;
        }

        return produto;
    }

    private static string? LerTextoObrigatorio(string mensagem, int tamanhoMaximo)
    {
        while (true)
        {
            Console.Write(mensagem);
            var valor = Console.ReadLine();
            if (valor is null)
            {
                ExibirAviso("Entrada encerrada.");
                return null;
            }

            valor = valor.Trim();
            if (valor.Length == 0)
            {
                ExibirAviso("O valor é obrigatório. Tente novamente.");
                continue;
            }

            if (valor.Length > tamanhoMaximo)
            {
                ExibirAviso($"Digite no máximo {tamanhoMaximo} caracteres.");
                continue;
            }

            return valor;
        }
    }

    private static decimal? LerDecimalNaoNegativo(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            var entrada = Console.ReadLine();
            if (entrada is null)
            {
                ExibirAviso("Entrada encerrada.");
                return null;
            }

            if (!decimal.TryParse(entrada, NumberStyles.Number, CulturaBrasileira, out var valor))
            {
                ExibirAviso("Preço inválido. Use somente números (ex.: 19,90).");
                continue;
            }

            if (valor < 0 || valor > 99_999_999.99m)
            {
                ExibirAviso("O preço deve estar entre 0 e 99.999.999,99.");
                continue;
            }

            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        }
    }

    private static int? LerInteiroNaoNegativo(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            var entrada = Console.ReadLine();
            if (entrada is null)
            {
                ExibirAviso("Entrada encerrada.");
                return null;
            }

            if (int.TryParse(entrada, out var valor) && valor >= 0)
            {
                return valor;
            }

            ExibirAviso("Digite um número inteiro maior ou igual a zero.");
        }
    }

    private static int? LerInteiroPositivo(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            var entrada = Console.ReadLine();
            if (entrada is null)
            {
                ExibirAviso("Entrada encerrada.");
                return null;
            }

            if (int.TryParse(entrada, out var valor) && valor > 0)
            {
                return valor;
            }

            ExibirAviso("Digite um ID inteiro maior que zero.");
        }
    }

    private static void ExibirProduto(Produto produto)
    {
        Console.WriteLine(new string('-', 55));
        Console.WriteLine($"ID:        {produto.Id}");
        Console.WriteLine($"Nome:      {produto.Nome}");
        Console.WriteLine($"Preço:     {produto.Preco.ToString("C", CulturaBrasileira)}");
        Console.WriteLine($"Estoque:   {produto.Estoque}");
        Console.WriteLine($"Categoria: {produto.Categoria}");
        Console.WriteLine(new string('-', 55));
    }

    private static void ExibirMenu()
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        Console.WriteLine("====================================");
        Console.WriteLine("      GERENCIAMENTO DE PRODUTOS");
        Console.WriteLine("====================================");
        Console.WriteLine("1. Inserir produto");
        Console.WriteLine("2. Listar produtos");
        Console.WriteLine("3. Buscar produto por ID");
        Console.WriteLine("4. Atualizar produto");
        Console.WriteLine("5. Excluir produto");
        Console.WriteLine("6. Sair");
        Console.Write("Escolha uma opção: ");
    }

    private static void Pausar()
    {
        Console.WriteLine();
        Console.Write("Pressione ENTER para continuar...");
        Console.ReadLine();
    }

    private static void ExibirSucesso(string mensagem) => ExibirColorido(mensagem, ConsoleColor.Green);

    private static void ExibirAviso(string mensagem) => ExibirColorido(mensagem, ConsoleColor.Yellow);

    private static void ExibirErro(string mensagem) => ExibirColorido(mensagem, ConsoleColor.Red);

    private static void ExibirColorido(string mensagem, ConsoleColor cor)
    {
        Console.ForegroundColor = cor;
        Console.WriteLine(mensagem);
        Console.ResetColor();
    }
}
