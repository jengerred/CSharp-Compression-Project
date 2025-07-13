using System;
using System.Collections.Generic;
using System.IO;
using CompressionProject.GUI.Animation;
using CompressionProject.GUI;

namespace CompressionProject
{
    // Handles all Huffman compression logic, no GUI code here.
    public class Compressor
    {
        // Stores the most recent Huffman tree built during compression
        public HuffmanTree LastTree { get; private set; }

        // Stores the most recent code table (character to Huffman code)
        public Dictionary<char, string> LastCodeTable { get; private set; }

        // Stores the most recent frequency table used for compression
        public CharacterFrequency[] LastFrequencies { get; private set; }

        /// <summary>
        /// Compresses the input file and writes the compressed output.
        /// Also saves the frequency table, Huffman tree, and code table for later use.
        /// </summary>
        /// <param name="inputFile">Path to the file to compress</param>
        /// <param name="compressedFile">Path to write the compressed output</param>
        public void CompressFile(string inputFile, string compressedFile)
        {
            // 1. Count character frequencies
            CharacterFrequency[] frequencies = new CharacterFrequency[256];
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1)
                {
                    if (frequencies[b] == null)
                        frequencies[b] = new CharacterFrequency((char)b);
                    else
                        frequencies[b].Increment();
                }
            }
            LastFrequencies = frequencies;

            // 2. Build Huffman tree and code table
            HuffmanTree tree = new HuffmanTree();
            tree.Build(frequencies);
            LastTree = tree;
            LastCodeTable = tree.CodeTable;

            // 3. Write compressed file
            WriteCompressed(inputFile, compressedFile, LastCodeTable);
        }

        /// <summary>
        /// Writes the compressed output file using the code table.
        /// </summary>
        private void WriteCompressed(string inputFile, string compressedFile, Dictionary<char, string> codeTable)
        {
            using (var input = new FileStream(inputFile, FileMode.Open))
            using (var output = new FileStream(compressedFile, FileMode.Create))
            {
                int bitBuffer = 0, bitCount = 0;
                int b;
                while ((b = input.ReadByte()) != -1)
                {
                    char c = (char)b;
                    string code = codeTable[c];
                    foreach (char bit in code)
                    {
                        bitBuffer = bitBuffer << 1 | (bit == '1' ? 1 : 0);
                        bitCount++;
                        if (bitCount == 8)
                        {
                            output.WriteByte((byte)bitBuffer);
                            bitBuffer = 0;
                            bitCount = 0;
                        }
                    }
                }
                if (bitCount > 0)
                {
                    bitBuffer <<= 8 - bitCount;
                    output.WriteByte((byte)bitBuffer);
                }
            }
        }
    }
}
