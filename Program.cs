using System;
using MerkleLib;
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel server to listen on both ports:
// 5000 for development, 80 for production/Docker
builder.WebHost.ConfigureKestrel(serverOptions => {
    serverOptions.ListenAnyIP(5000); // Development port
    serverOptions.ListenAnyIP(80);   // Docker port
});

// Add API controller services (for routing endpoints)
builder.Services.AddControllers();

var app = builder.Build();

// Enable HTTPS redirection and authorization middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Execute a static test runner to demonstrate Merkle functionality
MerkleTest.RunTest();

// Start the web server
app.Run();

/// <summary>
/// Class to test Merkle Tree functionality (used for assignment parts 1, 2, and 3).
/// Called once when the application starts.
/// </summary>
public class MerkleTest
{
    public static void RunTest()
    {
        // -------------------------
        // Assignment Part 1: Simple Merkle Tree from string inputs
        // -------------------------
        var inputs = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
        var merkleTree_part1 = new MerkleTree("Bitcoin_Transaction", "Bitcoin_Transaction");

        byte[] root = merkleTree_part1.ComputeMerkleRoot(inputs);
        Console.WriteLine("Merkle Root: " + TaggedHashUtil.ToHex(root));

        // -------------------------
        // Assignment Part 2: Proof of Reserve using User balances
        // -------------------------
        var merkleTree_part2 = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");
        List<User> users = new List<User>{
                            new User {Id = 1, Balance = 1111} ,
                            new User {Id = 2, Balance = 2222} ,
                            new User {Id = 3, Balance = 3333} ,
                            new User {Id = 4, Balance = 4444} ,
                            new User {Id = 5, Balance = 5555} ,
                            new User {Id = 6, Balance = 6666} ,
                            new User {Id = 7, Balance = 7777} ,
                            new User {Id = 8, Balance = 8888}
                            };

        // Print one user for visual confirmation
        Console.WriteLine("Users: " + users[0].ToString());

        // Define the user we want to verify (simulate client input)
        User newUser = new User { Id = 7, Balance = 7777 };

        // Validate the proof against the Merkle root
        var (merkleRoot, proof, balance) = merkleTree_part2.GenerateMerkleProof(users, 7);
        bool valid = merkleTree_part2.ValidateMerkleProof(proof, newUser, merkleRoot);

        if (valid)
        {
            Console.WriteLine("Valid user");
        }
        else
        {
            Console.WriteLine("InValid user");
        }

        //Assignment part 3
        //It is a web application and can be validated by sending http request on a browser
    }
}