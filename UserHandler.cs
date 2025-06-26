using System.Text.Json;
public class User
{
    public int Id { get; set; }
    public int Balance { get; set; }

    public override string ToString() => $"({Id},{Balance})"; // BIP340-compliant serialization
}

public static class FileUtils
{
    public static List<User> LoadUsersFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<User>>(json);
    }
}
