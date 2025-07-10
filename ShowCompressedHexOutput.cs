using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompressionProject
{
    public class ShowCompressedHexOutput
    {
        private readonly ListBox _frequencyResultsListBox;
        private readonly double labelFontSize = 14;
        private readonly double hexFontSize = 14;
        private readonly int hexPerLine = 16;

        public ShowCompressedHexOutput(ListBox frequencyResultsListBox)
        {
            _frequencyResultsListBox = frequencyResultsListBox;
        }

        /// <summary>
        /// Displays the compressed file as a hex dump (not a frequency table).
        /// </summary>
        /// <param name="compressedFilePath">Path to the compressed binary file (e.g., "Compression.bin")</param>
        public void Display(string compressedFilePath)
        {
            _frequencyResultsListBox.Items.Clear();

            // 1. Read all bytes from the compressed file
            byte[] compressedBytes = File.ReadAllBytes(compressedFilePath);

            // 2. Add a label at the top
            _frequencyResultsListBox.Items.Add(new TextBlock
            {
                Text = "Compressed Output (Hex):",
                FontSize = labelFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Margin = new System.Windows.Thickness(0, 10, 0, 10)
            });

            // 3. Add the hex values, wrapping to multiple lines
            for (int i = 0; i < compressedBytes.Length; i += hexPerLine)
            {
                int count = Math.Min(hexPerLine, compressedBytes.Length - i);
                string line = "";
                for (int j = 0; j < count; j++)
                {
                    line += compressedBytes[i + j].ToString("X2") + " ";
                }

                _frequencyResultsListBox.Items.Add(new TextBlock
                {
                    Text = line.TrimEnd(),
                    FontSize = hexFontSize,
                    FontWeight = FontWeights.Normal,
                    Foreground = Brushes.Black,
                    Margin = new System.Windows.Thickness(0, 2, 0, 2)
                });
            }
        }
    }
}
