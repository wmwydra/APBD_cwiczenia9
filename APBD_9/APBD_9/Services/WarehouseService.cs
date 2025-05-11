using System.Data;
using System.Data.Common;
using APBD_9.Models;
using Microsoft.Data.SqlClient;

namespace APBD_9.Services;

public class WarehouseService : iWarehouseService
{
    private readonly IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesProductExist(int productId)
    {
        var query = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesOrderExist(int productId, int amount)
    {
        var query = "SELECT 1 FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@Amount", amount);


        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesWarehouseExist(int warehouseId)
    {
        var query = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
        
        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> OrderNotCompleted(int orderId)
    {
        var query = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdOrder", orderId);


        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is null;
    }

    public async Task<int> GetOrderId(int productId, int amount)
    {
        int id = 0;
        var query = "SELECT IdOrder FROM [Order] WITH (NOLOCK) WHERE IdProduct = @IdProduct AND Amount = @Amount";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@Amount", amount);


        await connection.OpenAsync();
        
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            id = reader.GetInt32(0);
        }
        return id;
    }

    public async Task<decimal> GetProductPrice(int productId)
    {
        decimal price = 0;
        var query = "SELECT Price FROM Product WITH (NOLOCK) WHERE IdProduct = @IdProduct";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);
        
        await connection.OpenAsync();
        
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            price = reader.GetDecimal(0);
        }
        return price;
    }


    public async Task<int> putInProductWarehouse(ProductInWarehouseDTO body)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;

        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //zmiana kolumny FulfilledAt
            var query = @"UPDATE [Order] SET FulfilledAt = @Now 
                            WHERE IdProduct = @IdProduct AND Amount = @Amount";
            command.CommandText = query;
            command.Parameters.AddWithValue("@Now", DateTime.Now);
            command.Parameters.AddWithValue("@IdProduct", body.IdProduct);
            command.Parameters.AddWithValue("@Amount", body.Amount);

            await command.ExecuteNonQueryAsync();

            //wstawienie rekordu
            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                                    VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                                    SELECT SCOPE_IDENTITY();";


            int idOrder = await GetOrderId(body.IdProduct, body.Amount);
            var price = await GetProductPrice(body.IdProduct);
            var totalPrice = body.Amount * price;

            command.Parameters.AddWithValue("@IdWarehouse", body.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", body.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", body.Amount);
            command.Parameters.AddWithValue("@Price", totalPrice);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());

            await transaction.CommitAsync();
            return newId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> putInProductWarehouseProcedure(ProductInWarehouseDTO body)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", body.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", body.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", body.Amount);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        
        var newId = Convert.ToInt32(await command.ExecuteScalarAsync());

        return newId;
    }
}