using System.Collections.Generic;

namespace CompressionProject
{
    // HuffmanTree is responsible for building and storing a Huffman tree,
    // which is used to compress and decompress text by replacing common characters
    // with shorter codes and rare characters with longer codes.
    public class HuffmanTree
    {
        // This is the top node of the Huffman tree.
        // All other nodes are connected below this one.
        public HuffmanNode Root { get; private set; }

        // This dictionary links each character to its unique Huffman code (a string of 0s and 1s).
        // It lets us quickly look up the code for any character when compressing or decompressing.
        public Dictionary<char, string> CodeTable { get; private set; }

        // This method builds the Huffman tree using an array of character frequencies.
        // It is the main step in setting up compression and decompression.
        public void Build(CharacterFrequency[] frequencies)
        {
            // Step 1: Create a list of nodes, one for each character that appears in the data.
            var nodes = new List<HuffmanNode>();
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] != null)
                    nodes.Add(new HuffmanNode
                    {
                        Character = frequencies[i].Character,
                        Frequency = frequencies[i].Frequency
                    });
            }

            // Step 2: Build the tree by combining the two nodes with the lowest frequencies
            // until only one node (the root) remains.
            while (nodes.Count > 1)
            {
                // Sort the nodes so the ones with the smallest frequencies come first.
                nodes.Sort();

                // Take the two nodes with the lowest frequencies.
                var left = nodes[0];
                var right = nodes[1];

                // Remove these two nodes from the list, since we'll combine them.
                nodes.RemoveRange(0, 2);

                // Create a new parent node that combines the two nodes.
                // This new node's frequency is the sum of the two children's frequencies.
                // It does not represent a character itself (internal node).
                var parentNode = new HuffmanNode
                {
                    Character = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                // Set the parent pointers (helps with tree navigation if needed).
                left.Parent = parentNode;
                right.Parent = parentNode;

                // Add the new parent node back into the list to continue building the tree.
                nodes.Add(parentNode);
            }

            // Step 3: When only one node is left, it is the root of the Huffman tree.
            Root = nodes.Count > 0 ? nodes[0] : null;

            // Step 4: Create a table that maps each character to its Huffman code.
            // This is used for both compression and decompression.
            CodeTable = new Dictionary<char, string>();
            if (Root != null)
                GenerateCodes(Root, "");
        }

        // This private method walks through the Huffman tree and assigns a code to each character.
        // It builds the CodeTable by following the path to each character:
        // '0' for left, '1' for right.
        private void GenerateCodes(HuffmanNode node, string code)
        {
            if (node == null) return;

            // If this node represents a character (it's a leaf node), store its code.
            if (node.Character != null)
            {
                CodeTable[node.Character.Value] = code;
                return;
            }

            // Otherwise, keep going down the tree:
            // Add '0' when moving left, '1' when moving right.
            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }
}
