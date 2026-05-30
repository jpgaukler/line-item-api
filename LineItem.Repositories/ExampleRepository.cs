using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace LineItem.Repositories;

public class ExampleRepository : DatabaseRepository, IExampleRepository
{
    public ExampleRepository(string connectionString)
        : base(connectionString) { }

    /// <summary>
    /// This function is just example code, it should not be considered final. It might not need a transactin, and the
    /// reading of Id could maybe be simplified.
    /// </summary>
    public async Task<long> CreateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    )
    {
        using var connection = GetConnection();
        using var transaction = connection.BeginTransaction();
        using var command = new SqlCommand(
            "LineItem.SPR_Example_Insert",
            connection,
            transaction
        );

        command.Parameters.AddWithValue("@name", example.Name);
        command.Parameters.AddWithValue("@description", example.Description);

        await connection.OpenAsync(cancellationToken);

        int? id = null;

        try
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (reader.Read())
            {
                id = reader.IsDBNull("Id") ? null : reader.GetInt32("Id");
            }

            await transaction.CommitAsync(cancellationToken);

            return id.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<ExampleModel?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
