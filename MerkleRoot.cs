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

    public class MerkleTree
    {
        private readonly string _tag;

        public MerkleTree(string tag)
        {
            _tag = tag;
        }
        public byte[] ComputeMerkleRoot(List<string> strLeaves)
        {
            List<byte[]> nodes = new List<byte[]>();
            //Compute and store the hash of each leaf
            for (int i = 0; i < strLeaves.Count; i++)
            {
                nodes.Add(TaggedHashUtil.ComputeTaggedHash(_tag, Encoding.UTF8.GetBytes(strLeaves[i])));
            }

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
                    newNodes.Add(TaggedHashUtil.ComputeTaggedHash(_tag, concatenated));
                }
                nodes = newNodes;
            }
            return nodes[0];
        }
        
        // public byte[] ComputeMerkleRoot(List<string> leaves)
        // {
        //     List<byte[]> nodes = leaves
        //         .Select(s => TaggedHashUtil.ComputeTaggedHash(_tag, Encoding.UTF8.GetBytes(s)))
        //         .ToList();

        //     while (nodes.Count > 1)
        //     {
        //         if (nodes.Count % 2 != 0)
        //         {
        //             nodes.Add(nodes[^1]); // duplicate last element if odd
        //         }

        //         var newLevel = new List<byte[]>();

        //         for (int i = 0; i < nodes.Count; i += 2)
        //         {
        //             var combined = nodes[i].Concat(nodes[i + 1]).ToArray();
        //             var parentHash = TaggedHashUtil.ComputeTaggedHash(_tag, combined);
        //             newLevel.Add(parentHash);
        //         }

        //         nodes = newLevel;
        //     }

        //     return nodes[0];
        // }

    }
}