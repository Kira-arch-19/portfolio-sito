using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;

namespace Portfolio.Tests.Helpers;

/// <summary>
/// Ogni test possiede un database SQLite IN MEMORIA distinto.
/// Non modifica PostgreSQL, non richiede SSH e non usa gli User Secrets.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    public AppDbContext Context { get; }

    public SqliteTestDatabase()
    {
        // L'apertura mantiene vivo il database SQLite in memoria.
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
