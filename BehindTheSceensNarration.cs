using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionProject
{
    public static class BehindTheScenesNarration
    {
        public static string GetWaitingMessage()
        {
            return "Please use the \"Browse...\" button to select an ASCII text file for me to process. Waiting for user input.....";
        }

        public static string GetCompressionIntro()
        {
            return "⚡ Done with Compression!\n" +
                   "\nAlthough that was incredibly fast, here's what I was actually doing in the background to make your results both quick and accurate:\n\n" +
                   "As soon as you clicked Compress, I sprang into action:\n\n" +
                   "I read your file byte by byte.\n\n" +
                   "For every character, I updated a frequency table—an array with 256 slots (one for each possible ASCII character, 0–255).\n\n" +
                   "Each time a character appeared, I incremented its count in the array.\n\n" +
                   "This frequency table is the foundation of efficient compression!";
        }

        public static string HuffmanTree()
        {
            return
         "🌳 Building the Huffman Tree\n" +
        "\nWith the frequency table ready, I jumped straight into the next phase:\n\n" +
        "I took every character that showed up in your file and made it into a leaf node, each one labeled with its character and how often it appeared.\n\n" +
        "Then, like a matchmaker for bytes, I kept finding the two nodes with the lowest frequencies and merged them into a new parent node. This parent’s frequency is simply the sum of its two children.\n\n" +
        "I repeated this merging process—always picking the two smallest—until only one node remained at the very top: the root of the Huffman tree.\n\n" +
        "Why all this effort? Because this special binary tree (not a binary search tree!) is cleverly structured so that the most frequent characters end up closer to the root, which means their paths (and codes) are shorter and compression is more efficient!";
        }


        // Add more methods for other steps as you build out your narration
    }
}
