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
            return "I'm ready when you are! You can upload your own ASCII text file using the \"Browse...\" button, then click \"Compress\" to start. Or, simply click the \"Small Demo\" or \"Large Demo\" button to automatically load and compress a sample file. When you're ready to see the decompression, click the \"Decompress\" button. If the visualization is too large for the page, you can make it full screen for the best experience. I look forward to compressing whichever file you choose!";
        }



        public static string GetCompressionIntro()
        {
            return "⚡ Done with Compression!\n" +
                           "\nAlthough that was incredibly fast, here's what I was actually doing in the background to make your results both quick and accurate:\n\n" +
                           "As soon as you clicked Compress, I sprang into action:\n\n" +
                           "I read your file byte by byte.\n\n" +
                           "For every character, I updated a frequency table—an array with 256 slots (one for each possible ASCII character, 0–255).\n\n" +
                           "Each time a character appeared, I incremented its count in the array.\n\n" +
                           "This frequency table is the foundation of efficient compression! This character frequency chart displays every character and its frequency from your input file, providing a complete overview of your data.\n\n" +
                           "💡 For clarity and readability in the steps ahead, I’ll be showcasing only the first 20 readable characters — so we wont be overwhelmed by a massive wall of text!";
        }

        public static string ShowFirst20RawCharacters()
        {
            return
                "👀 Preview: The First 20 Raw Characters\n\n" +
                "Here are the first 20 characters from your file, shown exactly as they appear in the raw data. " +
                "Depending on your file, you might see only normal, readable text—or you might notice some 'weird' or unreadable symbols. " +
                "These unusual symbols can appear if your file contains special control characters, formatting codes, or other non-printable bytes that aren't meant to be displayed as regular text.\n\n" +
                "To keep things clear and easy to understand, I'll focus only on the first 20 readable characters (letters, numbers, punctuation, and spaces) in the steps ahead. " +
                "This way, you'll always get a preview that's meaningful and easy to follow, no matter what kind of file you use!";
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
          public static string EncodeData()
        {
            return
            "💾 Creating the Binary Code\n" +
            "\nWith the Huffman tree built, I assigned a unique binary code to each character in your file:\n\n" +
            "For every character in your file, I traced a path from the root of the tree down to that character’s leaf node (traversed).Every time I moved left, I added a ‘0’ to the code; every time I moved right, I added a ‘1’.\n\n" +
            "This traversal produces a distinct sequence of bits (0s and 1s) for each character, based on its path through the tree. By replacing every character in your data with its binary code, I transformed your original text into a long, compressed stream of bits—ready for the next stage!";
        }

        public static string PackBinaryData()
        {
            return
            "📦 Grouping the Binary Code into Bytes\n" +
            "\nNow that your entire message is represented as a continuous stream of 0s and 1s, I prepared it for storage as a file:\n\n" +
            "I grouped the binary digits into chunks of 8—each chunk forming a byte. If the last group was less than 8 bits, I padded it with zeros to fill the byte.\n\n" +
            "Each byte was then written to the compressed file. For display and debugging, I also converted these bytes into hexadecimal numbers: that’s why you might see letters like A–F in the output—they’re just part of the standard hex notation for numbers above 9.\n\n" +
            "By storing your data as a sequence of bytes, I turned your efficiently encoded bitstream into a real compressed file, ready for saving, sharing, or future decompression!";
        }

        public static string ShowCompressionResults(string originalFile, long originalSize, long compressedSize, string compressedFileName)
        {
            double ratio = (originalSize == 0) ? 0 : (double)compressedSize / originalSize;

            return
            "\n📦 Compressed Output (Hex): Actual File Result\n\n" +
           $"\nThe original file, {originalFile}, that you uploaded had a size of {originalSize} bytes. After compressing it, the size is now {compressedSize} bytes.\n\n" +
           $"That means the compression ratio is {(originalSize == 0 ? "N/A" : $"{ratio:P2}")}.\n\n" +
           $"Your compressed file has been automatically saved as {compressedFileName} in your project folder.\n\n" +
           "Below is the actual compressed output for your entire file, shown in hexadecimal—this is the true content of your file, not just the first 20 characters used for animation.\n";
        }
        public static string ReadyToRestore()
        {
            return "READY TO RESTORE YOUR FILE!?\n\n" +
                  "⬅️ Click the \"Decompress\" button to begin! ⬅️\n\n" +
                  "Or click the ⬇️ \"Next\" button ⬇️ to continue with just the first 20 characters.";
        }

        public static string DecompressionComplete()
        {
            return "✅ DECOMPRESSION COMPLETE! ✅\n\n" +
                   "🎉 Your file has been fully restored. Every letter, space, and symbol is now back in its original place—thanks to the magic of Huffman coding! ✨";
        }


        public static string DecompressionTreeExplanation()
        {
            return
                "🌳 Remember that Huffman tree we built during compression?\n\n" +
                "I just put it to work! Here’s how your file was restored:\n\n" +
                "• I read the compressed file bit by bit, following each '0' and '1' through the Huffman tree.\n" +
                "• Every time I reached a leaf node, I uncovered one of your original characters and added it to the output.\n" +
                "• This process continued until your entire file was rebuilt, character by character, exactly as it was before compression.\n\n" +
                "✨ Thanks to the power of Huffman coding, not a single letter, space, or symbol was lost or changed. Your data is safe and sound!\n\n" +
                "You can now open your restored file and see your original content, perfectly reconstructed.";
        }

        public static string ShowDecompressionResults(string decompressedFile)
        {
            return
                "🔓 Decompression Results: Your File is Back!\n\n" +
                $"Your file has been fully restored and saved as \"{decompressedFile}\" in your project files.\n\n" +
                "Every letter, space, and symbol is back in its original place—no data lost, no changes made!\n\n" +
                "You can now open this new file to see your original content, perfectly reconstructed and ready to use.";
        }




        // Add more methods for other steps as you build out your narration
    }
}
