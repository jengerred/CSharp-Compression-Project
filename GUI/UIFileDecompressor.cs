using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CompressionProject;

namespace CompressionProject.GUI
{
    // UIFileDecompressor handles the user interface for decompressing files.
    // It displays the decompressed output in a ListBox and shows error messages if needed.
    // The actual decompression logic is delegated to the Decompressor class in the core project.
    public class UIFileDecompressor
    {
        private readonly ListBox _resultsListBox;
        private readonly Decompressor _decompressor;

        public UIFileDecompressor(ListBox resultsListBox)
        {
            _resultsListBox = resultsListBox;
            _decompressor = new Decompressor();
        }

        /// <summary>
        /// Decompresses a file and displays the output in the UI.
        /// </summary>
        /// <param name="compressedFile">Path to the compressed file.</param>
        /// <param name="decompressedFile">Path to write the decompressed output.</param>
        /// <param name="root">The root node of the Huffman tree for decompression.</param>
        /// <returns>True if decompression was successful, false otherwise.</returns>
        public bool Decompress(string compressedFile, string decompressedFile, HuffmanNode root)
        {
            _resultsListBox.Items.Clear();

            try
            {
                // Use the core logic to decompress the file.
                _decompressor.DecompressFile(compressedFile, decompressedFile, root);

                // Read the decompressed output and display each line in the ListBox.
                using (StreamReader sr = new StreamReader(decompressedFile))
                {
                    while (!sr.EndOfStream)
                    {
                        _resultsListBox.Items.Add(sr.ReadLine());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // If something goes wrong, show an error message to the user.
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
