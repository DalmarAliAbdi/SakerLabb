using Microsoft.Data.Sqlite;
using SakerLabb.Web.Services;

namespace SakerLabb.Web.Data;

public class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Support")
            ?? "Data Source=sakerlabb.db";
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        using var connection = Open();
        var schema = connection.CreateCommand();
        schema.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL,
                Email TEXT NOT NULL,
                Personnummer TEXT NOT NULL,
                SecurityAnswer TEXT NOT NULL,
                ResetToken TEXT
            );
            CREATE TABLE IF NOT EXISTS Tickets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Body TEXT NOT NULL,
                Status TEXT NOT NULL,
                Priority TEXT NOT NULL,
                OwnerId INTEGER NOT NULL,
                Internal INTEGER NOT NULL DEFAULT 0,
                Created TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Comments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TicketId INTEGER NOT NULL,
                Author TEXT NOT NULL,
                Text TEXT NOT NULL,
                Created TEXT NOT NULL
            );
            """;
        schema.ExecuteNonQuery();

        var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Users";
        if (Convert.ToInt32(count.ExecuteScalar()) > 0)
        {
            return;
        }

        Seed(connection);
    }

    private static void Seed(SqliteConnection connection)
    {
        AddUser(connection, "admin", "admin123", "Admin", "admin@sakerlabb.internal", "19710412-8814", "Skogen");
        AddUser(connection, "mohamed", "Sommar2026!", "Agent", "mohamed@sakerlabb.internal", "19880903-4471", "Rex");
        AddUser(connection, "lina", "lina", "Agent", "lina@sakerlabb.internal", "19950127-2233", "Volvo");
        AddUser(connection, "peter", "Passw0rd", "User", "peter@kund.example", "20010715-9982", "Malmö");
        AddUser(connection, "sara", "hejhej", "User", "sara@kund.example", "19771130-1157", "Blomman");

        AddTicket(connection, "Skrivaren på plan 3 svarar inte", "Den blinkar orange och kön växer.", "Öppet", "Normal", 4, false);
        AddTicket(connection, "VPN kopplar ner var tionde minut", "Sker bara när jag jobbar hemifrån.", "Pågående", "Hög", 5, false);
        AddTicket(connection, "Begäran om utdrag enligt GDPR", "Kunden vill ha allt vi lagrar om hen.", "Öppet", "Normal", 4, false);
        AddTicket(connection, "INTERNT: lösenordsrotation domänkonton", "Servicekontot svc_backup har kvar lösenordet Vinter2019. Ska bytas innan revisionen.", "Öppet", "Kritisk", 1, true);
        AddTicket(connection, "INTERNT: brandväggsregel 10.20.4.0/24", "Tillfällig öppning mot leverantören ligger kvar sedan i mars.", "Pågående", "Hög", 2, true);
        AddTicket(connection, "Ny laptop till nyanställd", "Startdatum 1 september.", "Stängt", "Låg", 5, false);

        AddComment(connection, 1, "lina", "Bytte tonerkassett, ingen skillnad.");
        AddComment(connection, 2, "mohamed", "Ser paketförluster i loggen mellan 22 och 23.");
        AddComment(connection, 4, "admin", "Rotationen är beställd hos driftleverantören.");
    }

    private static void AddUser(SqliteConnection connection, string username, string password, string role, string email, string personnummer, string securityAnswer)
    {
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Username, PasswordHash, Role, Email, Personnummer, SecurityAnswer) VALUES ($u, $p, $r, $e, $n, $s)";
        command.Parameters.AddWithValue("$u", username);
        command.Parameters.AddWithValue("$p", CryptoService.HashPassword(password));
        command.Parameters.AddWithValue("$r", role);
        command.Parameters.AddWithValue("$e", email);
        command.Parameters.AddWithValue("$n", personnummer);
        command.Parameters.AddWithValue("$s", securityAnswer);
        command.ExecuteNonQuery();
    }

    private static void AddTicket(SqliteConnection connection, string title, string body, string status, string priority, int ownerId, bool isInternal)
    {
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Tickets (Title, Body, Status, Priority, OwnerId, Internal, Created) VALUES ($t, $b, $s, $p, $o, $i, $c)";
        command.Parameters.AddWithValue("$t", title);
        command.Parameters.AddWithValue("$b", body);
        command.Parameters.AddWithValue("$s", status);
        command.Parameters.AddWithValue("$p", priority);
        command.Parameters.AddWithValue("$o", ownerId);
        command.Parameters.AddWithValue("$i", isInternal ? 1 : 0);
        command.Parameters.AddWithValue("$c", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
        command.ExecuteNonQuery();
    }

    private static void AddComment(SqliteConnection connection, int ticketId, string author, string text)
    {
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Comments (TicketId, Author, Text, Created) VALUES ($t, $a, $x, $c)";
        command.Parameters.AddWithValue("$t", ticketId);
        command.Parameters.AddWithValue("$a", author);
        command.Parameters.AddWithValue("$x", text);
        command.Parameters.AddWithValue("$c", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
        command.ExecuteNonQuery();
    }
}
