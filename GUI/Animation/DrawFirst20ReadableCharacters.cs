using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CompressionProject;

namespace CompressionProject.GUI.Animation
{
    // DrawFirst20ReadableCharacters is a helper class for visually displaying the first 20
    // readable (printable) characters from a file. This is used in the app to help users
    // quickly see a preview of their file's contents, focusing only on characters like letters,
    // numbers, punctuation, and spaces (not control codes or unprintable symbols).
    //
    // By showing these characters on a Canvas, the app provides a friendly, visual way for
    // users to understand what data is being processed—even if they're new to programming.
    class DrawFirst20ReadableCharacters
    {
        /// <summary>
        /// Draws the first 20 readable characters onto the provided Canvas.
        /// Each character is shown in bold green text, spaced evenly across the canvas.
        /// This helps users preview the start of their file in a clear, visual way.
        /// </summary>
        /// <param name="canvas">
        /// The Canvas control where the characters will be drawn.
        /// </param>
        /// <param name="chars">
        /// The list of readable characters to display (usually the first 20 from a file).
        /// </param>
        public static void DrawFirst20ReadableChars(Canvas canvas, List<char> chars)
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
                    Foreground = Brushes.Green        // Use green color for visibility and distinction.
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
