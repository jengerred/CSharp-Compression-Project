using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CompressionProject.GUI;
using CompressionProject.GUI.Animation;

namespace CompressionProject
{
    // MainWindow is the main user interface for the compression project.
    // It lets users select files, compress or decompress them, and see visualizations and explanations.
    public partial class MainWindow : Window
    {
        // Handles the animation of the Huffman tree on the screen.
        private HuffmanTreeAnimation _treeAnimation;

        // Manages the step-by-step narration/explanation for the user.
        private NarrationSteps _narrationSteps;

        // Stores the last Huffman tree built, so we can use it for decompression.
        private HuffmanTree lastTree;

        // Tracks if the last file was decompressed, so we can update the UI and logic.
        private bool isDecompressed = false;

        // Stores the first 20 readable characters from the file for display or analysis.
        private List<char> first20ReadableChars;

        // Stores the Huffman code table (character to code mapping) for the last compression.
        private Dictionary<char, string> codeTable;

        // Handles the logic and UI updates for compressing files.
        private UIFileCompressor fileCompressor;

        // Handles the logic and UI updates for decompressing files.
        private UIFileDecompressor uiDecompressor;

        // The constructor sets up the user interface and connects button clicks to their actions.
        public MainWindow()
        {
            InitializeComponent();

            // Connect each button in the UI to its corresponding method.
            BrowseInputButton.Click += BrowseInputButton_Click;
            CompressButton.Click += CompressButton_Click;
            CompressSmallButton.Click += CompressSmallButton_Click;
            CompressLargeButton.Click += CompressLargeButton_Click;
            DecompressButton.Click += DecompressButton_Click;

            // Connect the step-by-step narration buttons.
            NextStepButton.Click += (s, e) => _narrationSteps?.Next();
            PreviousStepButton.Click += (s, e) => _narrationSteps?.Previous();

            // Set up the narration area and hide step buttons until needed.
            BehindTheScenesNarration.SetWaitingMessage(BehindTheScenesTextBlock);
            NextStepButton.Visibility = Visibility.Collapsed;
            PreviousStepButton.Visibility = Visibility.Collapsed;

            // Set up the file compressor, passing in all the UI elements it needs to update.
            fileCompressor = new UIFileCompressor(
                InputFileTextBox,
                CompressionResultsListBox,
                FrequencyResultsListBox,
                TreeAnimationCanvas,
                BehindTheScenesTextBlock,
                steps => _narrationSteps = steps,
                StepDecompressButton,
                NextStepButton,
                PreviousStepButton
            );

            // Set up the decompressor with its output area.
            uiDecompressor = new UIFileDecompressor(DecompressionResultsListBox);

            // Hide the step-by-step decompression button until needed.
            StepDecompressButton.Visibility = Visibility.Collapsed;
        }

        // Lets the user pick a file from their computer to compress.
        private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();
            if (openDialog.ShowDialog() == true)
                InputFileTextBox.Text = openDialog.FileName;
        }

        // Compresses the selected file using Huffman coding.
        // Only works if a valid file has been chosen.
        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputFileTextBox.Text) || !File.Exists(InputFileTextBox.Text))
            {
                MessageBox.Show("Please select a valid file to compress using the Browse button.");
                return;
            }

            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree; // Save the tree for later decompression
        }

        // Quickly compresses a small demo file included with the project.
        private void CompressSmallButton_Click(object sender, RoutedEventArgs e)
        {
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            InputFileTextBox.Text = Path.Combine(projectFolder, "SmallDemo.txt");
            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree;
        }

        // Quickly compresses a large demo file included with the project.
        private void CompressLargeButton_Click(object sender, RoutedEventArgs e)
        {
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            InputFileTextBox.Text = Path.Combine(projectFolder, "wap.txt");
            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree;
        }

        // Decompresses the last compressed file, using the last built Huffman tree.
        // Only works if a file has already been compressed in this session.
        private void DecompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastTree == null)
            {
                MessageBox.Show("Please compress a file first.");
                return;
            }

            string decompressedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decompressed.txt");

            // Decompress the file and update the UI.
            isDecompressed = uiDecompressor.Decompress(
                fileCompressor.LastCompressedFilePath,
                decompressedFile,
                lastTree.Root
            );
        }

        // Additional logic and UI event handlers can be added below as needed.

    }
}
