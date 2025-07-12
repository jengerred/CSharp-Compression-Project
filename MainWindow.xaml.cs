using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CompressionProject
{
    public partial class MainWindow : Window
    {
        private HuffmanTreeAnimation _treeAnimation;
        private NarrationSteps _narrationSteps;
        private HuffmanTree lastTree; // Store for decompression
        private bool isDecompressed = false;

        // Make these fields so they persist across steps
        private List<char> first20ReadableChars;
        private Dictionary<char, string> codeTable;

        private UIFileCompressor fileCompressor;

        public MainWindow()
        {
            InitializeComponent();

            BrowseInputButton.Click += BrowseInputButton_Click;
            CompressButton.Click += CompressButton_Click;
            CompressSmallButton.Click += CompressSmallButton_Click;
            CompressLargeButton.Click += CompressLargeButton_Click;
            DecompressButton.Click += DecompressButton_Click;

            NextStepButton.Click += (s, e) => _narrationSteps?.Next();
            PreviousStepButton.Click += (s, e) => _narrationSteps?.Previous();

            BehindTheScenesTextBlock.Text = BehindTheScenesNarration.GetWaitingMessage();

            fileCompressor = new UIFileCompressor(
                InputFileTextBox,
                CompressionResultsListBox,
                FrequencyResultsListBox,
                TreeAnimationCanvas,
                BehindTheScenesTextBlock,
                steps => _narrationSteps = steps,
                 StepDecompressButton
            );

            StepDecompressButton.Visibility = Visibility.Collapsed;

        }


        private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();
            if (openDialog.ShowDialog() == true)
                InputFileTextBox.Text = openDialog.FileName;
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree;
        }

        private void CompressSmallButton_Click(object sender, RoutedEventArgs e)
        {
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            InputFileTextBox.Text = Path.Combine(projectFolder, "SmallDemo.txt");
            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree;
        }

        private void CompressLargeButton_Click(object sender, RoutedEventArgs e)
        {
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            InputFileTextBox.Text = Path.Combine(projectFolder, "wap.txt");
            fileCompressor.Compress(isDecompressed);
            lastTree = fileCompressor.LastTree;
        }

        private void DecompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastTree == null)
            {
                MessageBox.Show("Please compress a file first.");
                return;
            }

            DecompressionResultsListBox.Items.Clear();

            string decompressedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decompressed.txt");

            try
            {
                // Use the correct compressed file!
                DecompressFile(fileCompressor.LastCompressedFilePath, decompressedFile, lastTree.Root);

                using (StreamReader sr = new StreamReader(decompressedFile))
                {
                    while (!sr.EndOfStream)
                    {
                        DecompressionResultsListBox.Items.Add(sr.ReadLine());
                    }
                }

                isDecompressed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                isDecompressed = false;
            }
        }

        private void DecompressFile(string compressedFile, string decompressedFile, HuffmanNode root)
        {
            using (var input = new FileStream(compressedFile, FileMode.Open))
            using (var output = new FileStream(decompressedFile, FileMode.Create))
            {
                HuffmanNode current = root;
                int b;
                while ((b = input.ReadByte()) != -1)
                {
                    for (int i = 7; i >= 0; i--)
                    {
                        bool bit = ((b >> i) & 1) == 1;
                        current = bit ? current.Right : current.Left;

                        if (current.Left == null && current.Right == null)
                        {
                            output.WriteByte((byte)current.Character.Value);
                            current = root;
                        }
                    }
                }
            }
        }
    }
}
