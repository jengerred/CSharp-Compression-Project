using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompressionProject
{
    public class ShowCompressedHexOutput
    {
        private readonly Canvas _canvas;
        private readonly double startX = 20;
        private readonly double hexY = 132;
        private readonly double hexSpacing = 32; // Space between hex values
        private readonly double labelFontSize = 14;
        private readonly double hexFontSize = 14;

        public ShowCompressedHexOutput(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Displays the actual compressed output as hex, using black text and matching the style of the bit animation.
        /// </summary>
        /// <param name="compressedFilePath">Path to the compressed binary file (e.g., "Compression.bin")</param>
        public void Display(string compressedFilePath)
        {
            _canvas.Children.Clear();

            // 1. Read and format the compressed file as hex
            byte[] compressedBytes = File.ReadAllBytes(compressedFilePath);
            string[] hexValues = BitConverter.ToString(compressedBytes).Split('-');

            // 2. Draw the label
            var hexLabel = new TextBlock
            {
                Text = "Compressed Output (Hex):",
                FontSize = labelFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(hexLabel, startX);
            Canvas.SetTop(hexLabel, hexY);
            _canvas.Children.Add(hexLabel);

            // 3. Draw the hex values in black, wrapping as needed
            double hexSummaryY = hexY + 30;
            double hexSummaryX = startX;
            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800; // fallback

            for (int i = 0; i < hexValues.Length; i++)
            {
                var hexBlock = new TextBlock
                {
                    Text = hexValues[i] + " ",
                    FontSize = hexFontSize,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(hexBlock, hexSummaryX);
                Canvas.SetTop(hexBlock, hexSummaryY);
                _canvas.Children.Add(hexBlock);

                hexSummaryX += hexSpacing;
                // Wrap to next line if needed
                if (hexSummaryX + hexSpacing > canvasWidth)
                {
                    hexSummaryX = startX;
                    hexSummaryY += 26;
                }
            }
        }
    }
}
