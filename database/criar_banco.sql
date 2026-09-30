USE [master];
GO

IF DB_ID(N'CheckpointProdutosDb') IS NULL
BEGIN
    CREATE DATABASE [CheckpointProdutosDb];
END;
GO

USE [CheckpointProdutosDb];
GO

IF OBJECT_ID(N'dbo.Produtos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Produtos
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Produtos PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Preco DECIMAL(10,2) NOT NULL
            CONSTRAINT CK_Produtos_Preco_NaoNegativo CHECK (Preco >= 0),
        Estoque INT NOT NULL
            CONSTRAINT CK_Produtos_Estoque_NaoNegativo CHECK (Estoque >= 0),
        Categoria NVARCHAR(60) NOT NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Produtos WHERE Nome = N'Teclado Mecanico' AND Categoria = N'Perifericos')
BEGIN
    INSERT INTO dbo.Produtos (Nome, Preco, Estoque, Categoria)
    VALUES (N'Teclado Mecanico', 249.90, 12, N'Perifericos');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Produtos WHERE Nome = N'Mouse Optico' AND Categoria = N'Perifericos')
BEGIN
    INSERT INTO dbo.Produtos (Nome, Preco, Estoque, Categoria)
    VALUES (N'Mouse Optico', 79.90, 25, N'Perifericos');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Produtos WHERE Nome = N'Monitor 24 polegadas' AND Categoria = N'Monitores')
BEGIN
    INSERT INTO dbo.Produtos (Nome, Preco, Estoque, Categoria)
    VALUES (N'Monitor 24 polegadas', 899.00, 8, N'Monitores');
END;
GO

SELECT Id, Nome, Preco, Estoque, Categoria
FROM dbo.Produtos
ORDER BY Id;
GO
