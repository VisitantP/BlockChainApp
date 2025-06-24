using System;
using MerkleLib;

MerkleTest.RunTest();
public class MerkleTest
{
    public static void RunTest()
    {
        var inputs = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
        var merkleTree = new MerkleTree("Bitcoin_Transaction");

        byte[] root = merkleTree.ComputeMerkleRoot(inputs);
        Console.WriteLine("Merkle Root: " + TaggedHashUtil.ToHex(root));
    }
}