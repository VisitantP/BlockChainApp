using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using MerkleLib;
using System.Text.Json;


[ApiController]
[Route("api")]
public class ProofController : ControllerBase
{

    [HttpGet("add-user")]
    public IActionResult AddUser([FromQuery] int id, [FromQuery] int balance)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserList.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found at {filePath}");
        }
        var users = FileUtils.LoadUsersFromFile(filePath);
        // Check if user already exists 
        if (users.Any(u => u.Id == id))
        {
            return Conflict($"User with ID {id} already exists.");
        }
        // Add new user
        User tmpUsr = new User { Id = id, Balance = balance };
        users.Add(tmpUsr);
        // Save back to JSON
        string updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(filePath, updatedJson);
        var message = $"User ({tmpUsr}) added successfully";
        return Ok(new
        {
            message = message
        });
    }

    [HttpGet("merkle-root")]
    public IActionResult GetMerkleRoot()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserList.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found at {filePath}");
        }
        var users = FileUtils.LoadUsersFromFile(filePath);
        var merkleTree = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");
        var root = merkleTree.ComputeMerkleRoot(users.Select(u => u.ToString()).ToList());
        return Ok(new { merkleRoot = TaggedHashUtil.ToHex(root) });
    }

    [HttpGet("merkle-proof/{id}")]
    public IActionResult GetProof(int id)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserList.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found at {filePath}");
        }
        var users = FileUtils.LoadUsersFromFile(filePath);
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound($"User with ID {id} not found in list");

        var merkleTree = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");
        var (root, proof, balance) = merkleTree.GenerateMerkleProof(users, id);

        var proofResponse = proof.Select(p => new
        {
            hash = TaggedHashUtil.ToHex(p.Hash),
            position = p.Position
        });

        return Ok(new
        {
            userBalance = balance,
            merkleProof = proofResponse
        });
    }

    [HttpGet("merkle-verify")]
    public IActionResult ValidateProof([FromQuery] int id, [FromQuery] int balance)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserList.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found at {filePath}");
        }
        var users = FileUtils.LoadUsersFromFile(filePath);
        
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound($"User with ID {id} not found in list");

        var merkleTree = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");
        var (root, proof, usrbalance) = merkleTree.GenerateMerkleProof(users, id);

        User tmpUsr = new User{ Id = id, Balance = balance};
        bool isValid = merkleTree.ValidateMerkleProof(proof, tmpUsr, root);
        if (isValid)
        {
            Console.WriteLine("Valid user");
        }
        else
        {
            Console.WriteLine("InValid user");
        }
        return Ok(new { isValid });
    }    
}