using System;
using MerkleLib;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();

MerkleTest.RunTest();
app.Run();


public class MerkleTest
{
    public static void RunTest()
    {
        //Assignment Part 1
        var inputs = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
        var merkleTree_part1 = new MerkleTree("Bitcoin_Transaction", "Bitcoin_Transaction");

        byte[] root = merkleTree_part1.ComputeMerkleRoot(inputs);
        Console.WriteLine("Merkle Root: " + TaggedHashUtil.ToHex(root));

        //Assignment Part 2
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
        Console.WriteLine("Users: " + users[0].ToString());


        User newUser = new User {Id=7, Balance=7777};

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
    }
}