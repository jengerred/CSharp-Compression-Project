using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompressionProject.GUI.Animation
{
    // Raw20CharactersAnimation is a helper class for visualizing the first 20 characters of a file.
    // It draws each character onto a Canvas, spacing them out so users can easily see what data is present.
    // This is especially helpful for beginners to understand what their file contains,
    // and to visualize even "weird" or non-printable characters in a friendly way.
    public static class Raw20CharactersAnimation
    {
        /// <summary>
        /// Draws the first 20 raw characters onto the provided Canvas.
        /// Each character is shown in bold blue text, spaced evenly across the canvas.
        /// This helps users preview the start of their file in a clear, visual way.
        /// </summary>
        /// <param name="canvas">
        /// The Canvas control where the characters will be drawn.
        /// </param>
        /// <param name="chars">
        /// The list of characters to display (usually the first 20 from a file).
        /// </param>
        public static void DrawFirst20RawCharacters(Canvas canvas, List<char> chars)
        {
            // Clear any previous drawings from the canvas, so we start fresh.
            canvas.Children.Clear();

            // Set up how far apart each character will be placed (in pixels).
            double charSpacing = 40;
            // Where the first character will start on the X axis.
            double startX = 10;
            // The Y position for all characters (they'll be in a straight line).
            double y = 20;

            // Loop through each character and add it to the canvas.
            for (int i = 0; i < chars.Count; i++)
            {
                // Create a TextBlock to display the character.
                var text = new TextBlock
                {
                    Text = chars[i].ToString(),       // Show the character itself.
                    FontSize = 18,                    // Make the text large and easy to read.
                    FontWeight = FontWeights.Bold,    // Make it bold for emphasis.
                    Foreground = Brushes.Blue         // Use blue color for visibility.
                };
                // Position the character horizontally, spaced out evenly.
                Canvas.SetLeft(text, startX + i * charSpacing);
                // All characters are aligned at the same vertical position.
                Canvas.SetTop(text, y);
                // Add the TextBlock to the canvas so it appears on screen.
                canvas.Children.Add(text);
            }
        }
    }
}
