namespace SakerLabb.Web.Data;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public string Email { get; set; } = "";
    public string Personnummer { get; set; } = "";
    public string SecurityAnswer { get; set; } = "";
    public string? ResetToken { get; set; }
}

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Status { get; set; } = "Öppet";
    public string Priority { get; set; } = "Normal";
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public bool Internal { get; set; }
    public string Created { get; set; } = "";
}

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Author { get; set; } = "";
    public string Text { get; set; } = "";
    public string Created { get; set; } = "";
}
