using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MerkleLib
{
    public static class TaggedHashUtil
    {
        public static byte[] ComputeTaggedHash(string tag, byte[] message)
        {
            using var sha256 = SHA256.Create();
            var tag_bytes = Encoding.UTF8.GetBytes(tag);
            byte[] tagHash = sha256.ComputeHash(tag_bytes);

            var concatenated = new byte[tagHash.Length * 2 + message.Length];
            Buffer.BlockCopy(tagHash, 0, concatenated, 0, tagHash.Length);
            Buffer.BlockCopy(tagHash, 0, concatenated, tagHash.Length, tagHash.Length);
            Buffer.BlockCopy(message, 0, concatenated, tagHash.Length * 2, message.Length);

            return sha256.ComputeHash(concatenated);

        }

        public static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public class MerkleNode
    {
        public byte[] Hash { get; set; }
        public MerkleNode Left { get; set; }
        public MerkleNode Right { get; set; }
        public bool IsLeaf => Left == null && Right == null;
    }

    public class MerkleTree
    {
        private readonly string _Leaftag;
        private readonly string _Branchtag;

        public MerkleNode Root { get; private set; }

        public MerkleTree(string Leaftag, string Branchtag)
        {
            _Leaftag = Leaftag;
            _Branchtag = Branchtag;
        }
        public byte[] ComputeMerkleRoot(List<string> strLeaves)
        {
            List<byte[]> nodes = new List<byte[]>();
            //Compute and store the hash of each leaf on the node
            for (int i = 0; i < strLeaves.Count; i++)
            {
                nodes.Add(TaggedHashUtil.ComputeTaggedHash(_Leaftag, Encoding.UTF8.GetBytes(strLeaves[i])));
            }

            //compute merkle by iteratively hashing each nodes level by level
            while (nodes.Count > 1)
            {
                if (nodes.Count % 2 != 0)
                {
                    nodes.Add(nodes[^1]);
                }
                List<byte[]> newNodes = new List<byte[]>();

                for (int i = 0; i < nodes.Count; i += 2)
                {
                    var concatenated = new byte[nodes[i].Length * 2];
                    Buffer.BlockCopy(nodes[i], 0, concatenated, 0, nodes[i].Length);
                    Buffer.BlockCopy(nodes[i + 1], 0, concatenated, nodes[i].Length, nodes[i + 1].Length);
                    newNodes.Add(TaggedHashUtil.ComputeTaggedHash(_Branchtag, concatenated));
                }
                //reassign newly calculated nodes to old nodes
                nodes = newNodes;
            }
            return nodes[0];
        }

        public (byte[] root, List<(byte[] Hash, int Position)> Proof, int UserBalance) GenerateMerkleProof(List<User> users, int userId)
        {
            List<byte[]> nodes = new List<byte[]>();
            //Compute and store the hash of each leaf on the node
            for (int i = 0; i < users.Count; i++)
            {
                nodes.Add(TaggedHashUtil.ComputeTaggedHash(_Leaftag, Encoding.UTF8.GetBytes(users[i].ToString())));
            }

            List<(byte[] Hash, int Position)> Proof = new List<(byte[] Hash, int Position)>();

            //Find the index in the list of users
            int index = users.FindIndex(u => u.Id == userId);
            if (index == -1)
                throw new Exception("User ID not found");

            var bal = users[index].Balance;


            while (nodes.Count > 1)
            {
                if (nodes.Count % 2 != 0)
                {
                    nodes.Add(nodes[^1]);
                }
                List<byte[]> newNodes = new List<byte[]>();

                for (int i = 0; i < nodes.Count; i += 2)
                {
                    var left = nodes[i];
                    var right = nodes[i + 1];
                    var concatenated = new byte[left.Length * 2];
                    Buffer.BlockCopy(left, 0, concatenated, 0, left.Length);
                    Buffer.BlockCopy(right, 0, concatenated, left.Length, right.Length);
                    if (i == index || i + 1 == index)
                    {
                        if (index % 2 == 0)
                            Proof.Add((right, 1)); // right sibling
                        else
                            Proof.Add((left, 0)); // left sibling
                    }
                    newNodes.Add(TaggedHashUtil.ComputeTaggedHash(_Branchtag, concatenated));
                }
                index = index / 2;
                nodes = newNodes;
            }

            return (nodes[0], Proof, bal);
        }

        public byte[] CreateMerkleTree(List<User> users)
        {

            List<MerkleNode> nodes = new List<MerkleNode>();

            for (int i = 0; i < users.Count; i++)
            {
                nodes.Add(new MerkleNode
                {
                    Hash = TaggedHashUtil.ComputeTaggedHash(_Leaftag, Encoding.UTF8.GetBytes(users[i].ToString())),
                    Left = null,
                    Right = null
                });
            }
            while (nodes.Count > 1)
            {
                if (nodes.Count % 2 != 0) nodes.Add(nodes[^1]);
                var parents = new List<MerkleNode>();
                for (int i = 0; i < nodes.Count; i += 2)
                {
                    var leftNode = nodes[i];
                    var rightNode = nodes[i + 1];
                    var combined = leftNode.Hash.Concat(rightNode.Hash).ToArray();
                    var parent = new MerkleNode
                    {
                        Left = leftNode,
                        Right = rightNode,
                        Hash = TaggedHashUtil.ComputeTaggedHash(_Branchtag, combined)
                    };
                    parents.Add(parent);
                }
                nodes = parents;
            }
            Root = nodes[0];
            return Root.Hash;
        }

        public bool GenerateMerkleProofWithTree(MerkleNode node, byte[] hashLeaf, List<(byte[] Hash, int Position)> path)
        {
            if (node.IsLeaf && node.Hash.SequenceEqual(hashLeaf))
                return true;

            if (node.Left != null && GenerateMerkleProofWithTree(node.Left, hashLeaf, path))
            {
                path.Add((node.Right.Hash, 1));
                return true;
            }

            if (node.Right != null && GenerateMerkleProofWithTree(node.Right, hashLeaf, path))
            {
                path.Add((node.Left.Hash, 0));
                return true;
            }

            return false;
        }

        public bool ValidateMerkleProof(List<(byte[] Hash, int Position)> Proof, User user, byte[] expectedRoot)
        {
            byte[] currentHash = TaggedHashUtil.ComputeTaggedHash(_Leaftag, Encoding.UTF8.GetBytes(user.ToString()));
            foreach (var (siblingHash, position) in Proof)
            {
                if (position == 0) //sibling is on the left
                {
                    currentHash = TaggedHashUtil.ComputeTaggedHash(_Branchtag, siblingHash.Concat(currentHash).ToArray());
                }
                else
                {
                    currentHash = TaggedHashUtil.ComputeTaggedHash(_Branchtag, currentHash.Concat(siblingHash).ToArray());
                }
            }
            return currentHash.SequenceEqual(expectedRoot);
        }

    }
}