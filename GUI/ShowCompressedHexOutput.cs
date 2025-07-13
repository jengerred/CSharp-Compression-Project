using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompressionProject.GUI
{
    // ShowCompressedHexOutput is responsible for displaying the compressed file
    // as a "hex dump" in the GUI. This means showing the actual bytes of the compressed
    // file, written out as hexadecimal numbers, so users can see what the compressed data looks like.
    public class ShowCompressedHexOutput
    {
        // The ListBox in the user interface where the hex output will be displayed.
        private readonly ListBox _frequencyResultsListBox;

        // Font size for the label at the top ("Compressed Output (Hex):")
        private readonly double labelFontSize = 14;

        // Font size for the hex lines themselves
        private readonly double hexFontSize = 14;

        // How many hex values to show per line for readability
        private readonly int hexPerLine = 45;

        // Constructor: takes the ListBox where the output will be shown.
        public ShowCompressedHexOutput(ListBox frequencyResultsListBox)
        {
            _frequencyResultsListBox = frequencyResultsListBox;
        }

        /// <summary>
        /// Displays the compressed file as a hex dump (not a frequency table).
        /// This lets users see the actual contents of the compressed file in a readable way.
        /// </summary>
        /// <param name="compressedFilePath">Path to the compressed binary file (e.g., "Compression.bin")</param>
        public void Display(string compressedFilePath)
        {
            // Clear any previous output from the ListBox.
            _frequencyResultsListBox.Items.Clear();

            // 1. Read all bytes from the compressed file into an array.
            byte[] compressedBytes = File.ReadAllBytes(compressedFilePath);

            // 2. Add a bold label at the top to explain what is being shown.
            _frequencyResultsListBox.Items.Add(new TextBlock
            {
                Text = "Compressed Output (Hex):",
                FontSize = labelFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 10, 0, 10)
            });

            // 3. Loop through the bytes and display them as hexadecimal values,
            // grouping them into lines for easier reading.
            for (int i = 0; i < compressedBytes.Length; i += hexPerLine)
            {
                // Figure out how many bytes to show on this line (could be less than hexPerLine at the end)
                int count = Math.Min(hexPerLine, compressedBytes.Length - i);
                string line = "";
                for (int j = 0; j < count; j++)
                {
                    // Convert each byte to a two-digit hex value (e.g., "0A", "FF")
                    line += compressedBytes[i + j].ToString("X2") + " ";
                }

                // Add this line of hex values to the ListBox as a TextBlock.
                _frequencyResultsListBox.Items.Add(new TextBlock
                {
                    Text = line.TrimEnd(),
                    FontSize = hexFontSize,
                    FontWeight = FontWeights.Normal,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0, 2, 0, 2)
                });
            }
        }
    }
}
