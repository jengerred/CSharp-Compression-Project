using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CompressionProject.GUI
{
    // The BehindTheScenesNarration class is responsible for updating the text instructions,
    // explanations, and visual feedback in the GUI. It helps users understand what is happening
    // at each step of the compression and decompression process.
    // 
    // All methods here update a TextBlock with rich, easy-to-read messages, often with emojis
    // and color for emphasis. This makes the app more engaging and educational for all users!
    public static class BehindTheScenesNarration
    {
        // Helper method to create a colored emoji for use in the narration.
        // Makes the UI more fun and visually appealing.
        private static Run Emoji(string emoji, Color color)
        {
            return new Run(emoji)
            {
                Foreground = new SolidColorBrush(color),
                FontSize = 18
            };
        }

        // Shows a welcome message and basic instructions for getting started.
        // Explains how to upload a file or use the demo files.
        public static void SetWaitingMessage(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(new Run("GETTING STARTED\n") { FontWeight = FontWeights.Bold, FontSize = 18 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("You can "));
            tb.Inlines.Add(new Run("upload your own ASCII text file") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(" using the ");
            tb.Inlines.Add(new Run("\"Browse...\"") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGoldenrod });
            tb.Inlines.Add(" button, then click ");
            tb.Inlines.Add(new Run("\"Compress\"") { FontWeight = FontWeights.Bold, Foreground = Brushes.SteelBlue });
            tb.Inlines.Add(" to start.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("Or, simply click the ");
            tb.Inlines.Add(new Run("\"Small Demo\"") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateGray });
            tb.Inlines.Add(" or ");
            tb.Inlines.Add(new Run("\"Large Demo\"") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateGray });
            tb.Inlines.Add(" button to automatically load and compress a sample file.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("When you're ready to see the decompression, click the ");
            tb.Inlines.Add(new Run("\"Decompress\"") { FontWeight = FontWeights.Bold, Foreground = Brushes.ForestGreen });
            tb.Inlines.Add(" button.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("If the visualization is too large for the page, you can make it full screen for the best experience.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("I look forward to compressing whichever file you choose!");
        }

        // Explains what happened during the compression phase.
        // Describes how the program reads the file, counts character frequencies,
        // and prepares for building the Huffman tree.
        public static void SetCompressionIntro(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("⚡", Colors.Gold));
            tb.Inlines.Add(new Run(" DONE WITH COMPRESSION!\n") { FontWeight = FontWeights.Bold, FontSize = 18 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Although that was incredibly fast, here's what I was actually doing in the background to make your results both quick and accurate:") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("As soon as you clicked "));
            tb.Inlines.Add(new Run("Compress") { FontWeight = FontWeights.Bold, Foreground = Brushes.SteelBlue });
            tb.Inlines.Add(", I sprang into action:");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("• I read your file byte by byte.\n");
            tb.Inlines.Add("• For every character, I updated a frequency table—an array with 256 slots (one for each possible ASCII character, 0–255).\n");
            tb.Inlines.Add("• Each time a character appeared, I incremented its count in the array.\n");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("This frequency table is the foundation of efficient compression!") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(" This character frequency chart displays every character and its frequency from your input file, providing a complete overview of your data.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("💡", Colors.CornflowerBlue));
            tb.Inlines.Add(new Run(" For clarity and readability in the steps ahead, I’ll be showcasing only the first 20 characters — so we won't be overwhelmed by a massive wall of text!") { FontStyle = FontStyles.Italic });
        }

        // Shows the first 20 raw characters from the file, explaining
        // that some may not be readable and why.
        public static void SetFirst20RawCharacters(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("👀", Colors.OrangeRed));
            tb.Inlines.Add(new Run(" PREVIEW: THE FIRST 20 RAW CHARACTERS\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Here are the first 20 characters from your file, shown exactly as they appear in the raw data. "));
            tb.Inlines.Add(new Run("Depending on your file, you might see only normal, readable text—or you might notice some 'weird' or unreadable symbols.") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("These unusual symbols can appear if your file contains special control characters, formatting codes, or other non-printable bytes that aren't meant to be displayed as regular text.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("To keep things clear and easy to understand, I'll focus only on the ");
            tb.Inlines.Add(new Run("first 20 readable characters") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(" (letters, numbers, punctuation, and spaces) in the steps ahead. ");
            tb.Inlines.Add("This way, you'll always get a preview that's meaningful and easy to follow, no matter what kind of file you use!");
        }

        // Explains how the Huffman tree is built from the frequency table.
        // Details the merging process and why the tree structure is important.
        public static void SetHuffmanTree(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("🌳", Colors.ForestGreen));
            tb.Inlines.Add(new Run(" BUILDING THE HUFFMAN TREE\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("With the frequency table ready, I jumped straight into the next phase:") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("• I took every character that showed up in your file and made it into a leaf node, each one labeled with its character and how often it appeared.\n");
            tb.Inlines.Add("• Then, like a matchmaker for bytes, I kept finding the two nodes with the lowest frequencies and merged them into a new parent node. This parent’s frequency is simply the sum of its two children.\n");
            tb.Inlines.Add("• I repeated this merging process—always picking the two smallest—until only one node remained at the very top: the root of the Huffman tree.\n");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Why all this effort?") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(" Because this special binary tree (not a binary search tree!) is cleverly structured so that the most frequent characters end up closer to the root, which means their paths (and codes) are shorter and compression is more efficient!");
        }

        // Explains how binary codes are assigned to each character using the Huffman tree.
        public static void SetEncodeData(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("💾", Colors.DodgerBlue));
            tb.Inlines.Add(new Run(" CREATING THE BINARY CODE\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("With the Huffman tree built, I assigned a unique binary code to each character in your file:") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("For every character in your file, I traced a path from the root of the tree down to that character’s leaf node. Every time I moved left, I added a ‘0’ to the code; every time I moved right, I added a ‘1’.");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("This traversal produces a distinct sequence of bits (0s and 1s) for each character, based on its path through the tree. By replacing every character in your data with its binary code, I transformed your original text into a long, compressed stream of bits—ready for the next stage!");
        }

        // Explains how the binary codes are grouped into bytes and saved as a compressed file.
        public static void SetPackBinaryData(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("📦", Colors.DarkOrange));
            tb.Inlines.Add(new Run(" GROUPING THE BINARY CODE INTO BYTES\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Now that your entire message is represented as a continuous stream of 0s and 1s, I prepared it for storage as a file:") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("• I grouped the binary digits into chunks of 8—each chunk forming a byte. If the last group was less than 8 bits, I padded it with zeros to fill the byte.\n");
            tb.Inlines.Add("• Each byte was then written to the compressed file. For display and debugging, I also converted these bytes into hexadecimal numbers: that’s why you might see letters like A–F in the output—they’re just part of the standard hex notation for numbers above 9.\n");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("By storing your data as a sequence of bytes, I turned your efficiently encoded bitstream into a real compressed file, ready for saving, sharing, or future decompression!");
        }

        // Shows the results of compression, including file sizes and compression ratio.
        public static void SetCompressionResults(TextBlock tb, string originalFile, long originalSize, long compressedSize, string compressedFileName)
        {
            double ratio = originalSize == 0 ? 0 : (double)compressedSize / originalSize;

            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("📦", Colors.DarkViolet));
            tb.Inlines.Add(new Run(" COMPRESSED OUTPUT (HEX): ACTUAL FILE RESULT\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run($"The original file, {originalFile}, that you uploaded had a size of {originalSize} bytes. After compressing it, the size is now {compressedSize} bytes.\n\n"));
            tb.Inlines.Add(new Run($"That means the compression ratio is {(originalSize == 0 ? "N/A" : $"{ratio:P2}")}.\n") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(new Run($"Your compressed file has been automatically saved as {compressedFileName} in your project folder.\n\n"));
            tb.Inlines.Add("Below is the actual compressed output for your entire file, shown in hexadecimal—this is the true content of your file, not just the first 20 characters used for animation.");
        }

        // Informs the user that they can now decompress the file.
        public static void SetReadyToRestore(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(new Run("READY TO RESTORE YOUR FILE!?\n") { FontWeight = FontWeights.Bold, FontSize = 16, Foreground = Brushes.DarkGreen });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("⬅️", Colors.DarkOrange));
            tb.Inlines.Add(new Run(" Click the \"Decompress\" button to begin! ") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(Emoji("⬅️", Colors.DarkOrange));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("⬇️", Colors.CornflowerBlue));
            tb.Inlines.Add(new Run(" Or click the \"Next\" button to continue with just the first 20 characters.") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(Emoji("⬇️", Colors.CornflowerBlue));
        }

        // Announces that decompression is complete and the file has been restored.
        public static void SetDecompressionComplete(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("✅", Colors.ForestGreen));
            tb.Inlines.Add(new Run(" DECOMPRESSION COMPLETE! ") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(Emoji("✅", Colors.ForestGreen));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("🎉", Colors.Gold));
            tb.Inlines.Add(new Run(" Your file has been fully restored. Every letter, space, and symbol is now back in its original place—thanks to the magic of Huffman coding! ") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(Emoji("✨", Colors.MediumVioletRed));
        }

        // Explains how the Huffman tree is used to decode the compressed file.
        public static void SetDecompressionTreeExplanation(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("🌳", Colors.ForestGreen));
            tb.Inlines.Add(new Run(" REMEMBER THAT HUFFMAN TREE WE BUILT DURING COMPRESSION?\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("I just put it to work! Here’s how your file was restored:");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("• I read the compressed file bit by bit, following each '0' and '1' through the Huffman tree.\n");
            tb.Inlines.Add("• Every time I reached a leaf node, I uncovered one of your original characters and added it to the output.\n");
            tb.Inlines.Add("• This process continued until your entire file was rebuilt, character by character, exactly as it was before compression.\n");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("✨", Colors.MediumVioletRed));
            tb.Inlines.Add(new Run(" Thanks to the power of Huffman coding, not a single letter, space, or symbol was lost or changed. Your data is safe and sound!") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("You can now open your restored file and see your original content, perfectly reconstructed.");
        }

        // Shows the final decompression results and lets the user know where to find their restored file.
        public static void SetDecompressionResults(TextBlock tb, string decompressedFile)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("🔓", Colors.DeepSkyBlue));
            tb.Inlines.Add(new Run(" DECOMPRESSION RESULTS: YOUR FILE IS BACK!\n") { FontWeight = FontWeights.Bold, FontSize = 16 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run($"Your file has been fully restored and saved as \"{decompressedFile}\" in your project files.\n\n"));
            tb.Inlines.Add(new Run("Every letter, space, and symbol is back in its original place—no data lost, no changes made!\n") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add("You can now open this new file to see your original content, perfectly reconstructed and ready to use.");
        }

        // Wraps up the entire process with a friendly, encouraging message.
        public static void SetFinalConclusion(TextBlock tb)
        {
            tb.Inlines.Clear();
            tb.Inlines.Add(Emoji("🏁", Colors.Gold));
            tb.Inlines.Add(new Run(" ALL DONE! YOU'VE MASTERED HUFFMAN COMPRESSION\n") { FontWeight = FontWeights.Bold, FontSize = 18 });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("🤝", Colors.CornflowerBlue));
            tb.Inlines.Add(new Run(" What a journey! Together, we:\n") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add("• Explored your file’s characters and counted how often each one appeared.\n");
            tb.Inlines.Add("• Built a Huffman tree, merging nodes step by step until just one remained at the top.\n");
            tb.Inlines.Add("• Created unique binary codes for each character, making your data as compact as possible.\n");
            tb.Inlines.Add("• Packed all those bits into real bytes, ready for storage or sharing.\n");
            tb.Inlines.Add("• And finally, decompressed your file—restoring every letter, space, and symbol, perfectly intact!\n");
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("💡", Colors.Gold));
            tb.Inlines.Add(new Run(" Huffman coding is a powerful tool, and now you’ve seen it in action—from start to finish!") { FontStyle = FontStyles.Italic });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Thank you for exploring data compression with me. If you want to try again, just upload or click on a new file to replay the animation!") { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(Emoji("👋", Colors.DarkOrange));
            tb.Inlines.Add(new Run(" This is the end of our journey—for now. Until next time, happy compressing!") { FontStyle = FontStyles.Italic });
        }

    }
}
