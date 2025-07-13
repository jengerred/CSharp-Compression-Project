using System;
using System.IO;

namespace CompressionProject
{
    // Decompressor handles the logic for decompressing files that were compressed using Huffman coding.
    // This class does NOT interact with any user interface elements.
    // It simply takes a compressed file and a Huffman tree, and writes the decompressed output.
    public class Decompressor
    {
        /// <summary>
        /// Decompresses a file using the provided Huffman tree.
        /// </summary>
        /// <param name="compressedFile">Path to the compressed (binary) file.</param>
        /// <param name="decompressedFile">Path where the decompressed output will be written.</param>
        /// <param name="root">The root node of the Huffman tree used for decompression.</param>
        public void DecompressFile(string compressedFile, string decompressedFile, HuffmanNode root)
        {
            // Open the compressed input file for reading and the output file for writing.
            using (var input = new FileStream(compressedFile, FileMode.Open))
            using (var output = new FileStream(decompressedFile, FileMode.Create))
            {
                HuffmanNode current = root;
                int b;
                // Read each byte from the compressed file.
                while ((b = input.ReadByte()) != -1)
                {
                    // Process each bit in the byte, from left (most significant) to right.
                    for (int i = 7; i >= 0; i--)
                    {
                        // Extract the current bit (true for 1, false for 0).
                        bool bit = (b >> i & 1) == 1;

                        // Traverse the Huffman tree: right for 1, left for 0.
                        current = bit ? current.Right : current.Left;

                        // If we've reached a leaf node (a character), write it to output.
                        if (current.Left == null && current.Right == null)
                        {
                            output.WriteByte((byte)current.Character.Value);
                            current = root; // Go back to the top for the next character.
                        }
                    }
                }
            }
        }
    }
}
