using System.Collections.Generic;

namespace CompressionProject
{
    // Encapsulates the Huffman tree and related operations
    public class HuffmanTree
    {
        public HuffmanNode Root { get; private set; }
        public Dictionary<char, string> CodeTable { get; private set; }

        // Build the Huffman tree from character frequencies
        public void Build(CharacterFrequency[] frequencies)
        {
            var nodes = new List<HuffmanNode>();
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] != null)
                    nodes.Add(new HuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });
            }

            while (nodes.Count > 1)
            {
                nodes.Sort();
                var left = nodes[0];
                var right = nodes[1];
                nodes.RemoveRange(0, 2);
                nodes.Add(new HuffmanNode
                {
                    Character = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                });
            }
            Root = nodes.Count > 0 ? nodes[0] : null;
            CodeTable = new Dictionary<char, string>();
            if (Root != null)
                GenerateCodes(Root, "");
        }

        // Recursively generate the code table from the tree
        private void GenerateCodes(HuffmanNode node, string code)
        {
            if (node == null) return;
            if (node.Character != null)
            {
                CodeTable[node.Character.Value] = code;
                return;
            }
            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }
}
