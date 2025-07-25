using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using MerkleLib;
using System.Text.Json;

/// <summary>
/// Web API Controller for Proof of Reserve functionality using Merkle trees.
/// Exposes endpoints to add users, compute Merkle roots, generate and verify proofs.
/// </summary>
[ApiController]
[Route("api")]
public class ProofController : ControllerBase
{
    /// <summary>
    /// Adds a new user to UserList.json if the ID doesn't already exist.
    /// </summary>
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

        //Part -3 (suggestion): This merkle tree can be generated in the background after every addition or deletion of the user 
        //and can be stored and used for future requests for faster validation
        // var root = merkleTree.CreateMerkleTree(users);
        System.IO.File.WriteAllText(filePath, updatedJson);
        var message = $"User ({tmpUsr}) added successfully";
        return Ok(new
        {
            message = message
        });
    }

    /// <summary>
    /// Returns the Merkle root computed from the current user list.
    /// </summary>
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
        // var root2 = merkleTree.CreateMerkleTree(users.Select(u => u.ToString()).ToList());

        return Ok(new { merkleRoot = TaggedHashUtil.ToHex(root) });
    }

    /// <summary>
    /// Returns a Merkle proof for a specific user based on the current Merkle root.
    /// </summary>
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

    /// <summary>
    /// Verifies a Merkle proof for a given user (via query string).
    /// </summary>
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

        User tmpUsr = new User { Id = id, Balance = balance };
        bool isValid = merkleTree.ValidateMerkleProof(proof, tmpUsr, root);

        return Ok(new
        {
            isValid
        });
    }

    /// <summary>
    /// Returns a Merkle proof using a fully built tree (faster for large trees, less recomputation).
    /// </summary>
    [HttpGet("merkle-proof-fast/{id}")]
    public IActionResult GetProofFast(int id)
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

        //Part -3 (suggestion): This merkle tree can be generated in the background after every addition or deletion of the user 
        //and can be stored and used for future requests for faster validation. In that case the merkle tree that is already pre
        //generated will be used for generating proofs.
        var merkleTree = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");
        var root = merkleTree.CreateMerkleTree(users); 
        List<(byte[] Hash, int Position)> _path = new List<(byte[] Hash, int Position)>();
        var leafHash = TaggedHashUtil.ComputeTaggedHash("ProofOfReserve_Leaf", Encoding.UTF8.GetBytes(user.ToString()));
        bool foundLeaf = merkleTree.GenerateMerkleProofWithTree(merkleTree.Root, leafHash, _path);
        if (!foundLeaf)
            return NotFound($"User with ID {id} is invalid/corrupted");

        var proofResponse = _path.Select(p => new
        {
            hash = TaggedHashUtil.ToHex(p.Hash),
            position = p.Position
        });

        return Ok(new
        {
            userBalance = user.Balance,
            merkleProof = proofResponse
        });
    }

    /// <summary>
    /// Validates a Merkle proof using a fully built tree (efficient for large lists).
    /// </summary>
    [HttpGet("merkle-verify-fast")]
    public IActionResult ValidateProofFast([FromQuery] int id, [FromQuery] int balance)
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
        

        User tmpUsr = new User{ Id = id, Balance = balance};
        var merkleTree = new MerkleTree("ProofOfReserve_Leaf", "ProofOfReserve_Branch");

        //Part -3 (suggestion): This merkle tree can be generated in the background after every addition or deletion of the user 
        //and can be stored and used for future requests for faster validation. In that case the merkle tree that is already pre
        //generated will be used for generating proofs.
        var root = merkleTree.CreateMerkleTree(users); 
        List<(byte[] Hash, int Position)> _path = new List<(byte[] Hash, int Position)>();
        var leafHash = TaggedHashUtil.ComputeTaggedHash("ProofOfReserve_Leaf", Encoding.UTF8.GetBytes(tmpUsr.ToString()));
        bool foundLeaf = merkleTree.GenerateMerkleProofWithTree(merkleTree.Root, leafHash, _path);
        if (!foundLeaf)
            return NotFound($"User with ID {id} is invalid/corrupted");

        bool isValid = merkleTree.ValidateMerkleProof(_path, tmpUsr, root);

        return Ok(new
        {
            isValid
        });
    }    
}