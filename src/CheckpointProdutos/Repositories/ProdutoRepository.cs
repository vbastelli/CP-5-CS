using System.Data;
using CheckpointProdutos.Models;
using CheckpointProdutos.Services;
using Microsoft.Data.SqlClient;

namespace CheckpointProdutos.Repositories;

public class ProdutoRepository
{
    private readonly string _connectionString;
    private readonly ArquivoLogger _logger;

    public ProdutoRepository(string connectionString, ArquivoLogger logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public int Inserir(Produto produto)
    {
        const string sql = """
            INSERT INTO Produtos (Nome, Preco, Estoque, Categoria)
            VALUES (@Nome, @Preco, @Estoque, @Categoria);
            SET @NovoId = CONVERT(INT, SCOPE_IDENTITY());
            """;

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            conexao.Open();

            // A transação garante que a inclusão e a obtenção do ID formem uma unidade atômica.
            using var transacao = conexao.BeginTransaction();
            try
            {
                using var comando = new SqlCommand(sql, conexao, transacao);
                AdicionarParametrosProduto(comando, produto);
                var parametroId = comando.Parameters.Add("@NovoId", SqlDbType.Int);
                parametroId.Direction = ParameterDirection.Output;

                comando.ExecuteNonQuery();
                transacao.Commit();

                produto.Id = (int)parametroId.Value;
                _logger.Registrar("INSERÇÃO", $"Produto ID {produto.Id} ('{produto.Nome}') inserido.");
                return produto.Id;
            }
            catch
            {
                transacao.Rollback();
                throw;
            }
        }
        catch (SqlException ex)
        {
            _logger.Registrar("ERRO", $"Falha ao inserir produto. SQL {ex.Number}: {ex.Message}");
            throw new RepositorioException("Não foi possível inserir o produto no banco de dados.", ex);
        }
    }

    public List<Produto> Listar()
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos ORDER BY Id;";

        try
        {
            var produtos = new List<Produto>();
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);
            conexao.Open();

            using var leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                produtos.Add(MapearProduto(leitor));
            }

            _logger.Registrar("LISTAGEM", $"Listagem realizada: {produtos.Count} produto(s) retornado(s).");
            return produtos;
        }
        catch (SqlException ex)
        {
            _logger.Registrar("ERRO", $"Falha ao listar produtos. SQL {ex.Number}: {ex.Message}");
            throw new RepositorioException("Não foi possível listar os produtos.", ex);
        }
    }

    public Produto? BuscarPorId(int id)
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @Id;";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conexao.Open();

            using var leitor = comando.ExecuteReader();
            var produto = leitor.Read() ? MapearProduto(leitor) : null;
            _logger.Registrar("BUSCA", produto is null
                ? $"Produto ID {id} não encontrado."
                : $"Produto ID {id} encontrado.");
            return produto;
        }
        catch (SqlException ex)
        {
            _logger.Registrar("ERRO", $"Falha ao buscar produto ID {id}. SQL {ex.Number}: {ex.Message}");
            throw new RepositorioException("Não foi possível buscar o produto.", ex);
        }
    }

    public bool Atualizar(Produto produto)
    {
        const string sql = """
            UPDATE Produtos
            SET Nome = @Nome,
                Preco = @Preco,
                Estoque = @Estoque,
                Categoria = @Categoria
            WHERE Id = @Id;
            """;

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);
            AdicionarParametrosProduto(comando, produto);
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = produto.Id;
            conexao.Open();

            var atualizado = comando.ExecuteNonQuery() > 0;
            _logger.Registrar("ATUALIZAÇÃO", atualizado
                ? $"Produto ID {produto.Id} atualizado."
                : $"Produto ID {produto.Id} não encontrado para atualização.");
            return atualizado;
        }
        catch (SqlException ex)
        {
            _logger.Registrar("ERRO", $"Falha ao atualizar produto ID {produto.Id}. SQL {ex.Number}: {ex.Message}");
            throw new RepositorioException("Não foi possível atualizar o produto.", ex);
        }
    }

    public bool Excluir(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id;";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conexao.Open();

            var excluido = comando.ExecuteNonQuery() > 0;
            _logger.Registrar("EXCLUSÃO", excluido
                ? $"Produto ID {id} excluído."
                : $"Produto ID {id} não encontrado para exclusão.");
            return excluido;
        }
        catch (SqlException ex)
        {
            _logger.Registrar("ERRO", $"Falha ao excluir produto ID {id}. SQL {ex.Number}: {ex.Message}");
            throw new RepositorioException("Não foi possível excluir o produto.", ex);
        }
    }

    private static void AdicionarParametrosProduto(SqlCommand comando, Produto produto)
    {
        comando.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = produto.Nome.Trim();

        var parametroPreco = comando.Parameters.Add("@Preco", SqlDbType.Decimal);
        parametroPreco.Precision = 10;
        parametroPreco.Scale = 2;
        parametroPreco.Value = produto.Preco;

        comando.Parameters.Add("@Estoque", SqlDbType.Int).Value = produto.Estoque;
        comando.Parameters.Add("@Categoria", SqlDbType.NVarChar, 60).Value = produto.Categoria.Trim();
    }

    private static Produto MapearProduto(SqlDataReader leitor)
    {
        return new Produto
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("Id")),
            Nome = leitor.GetString(leitor.GetOrdinal("Nome")),
            Preco = leitor.GetDecimal(leitor.GetOrdinal("Preco")),
            Estoque = leitor.GetInt32(leitor.GetOrdinal("Estoque")),
            Categoria = leitor.GetString(leitor.GetOrdinal("Categoria"))
        };
    }
}
