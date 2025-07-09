using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CompressionProject
{
    public partial class MainWindow : Window
    {
        private HuffmanTreeAnimation _treeAnimation;
        private NarrationSteps _narrationSteps;
        private HuffmanTree lastTree; // Store for decompression

        public MainWindow()
        {
            InitializeComponent();

            BrowseInputButton.Click += BrowseInputButton_Click;
            CompressButton.Click += CompressButton_Click;
            DecompressButton.Click += DecompressButton_Click;

            NextStepButton.Click += (s, e) => _narrationSteps?.Next();
            PreviousStepButton.Click += (s, e) => _narrationSteps?.Previous();

            BehindTheScenesTextBlock.Text = BehindTheScenesNarration.GetWaitingMessage();
        }

        private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();
            if (openDialog.ShowDialog() == true)
                InputFileTextBox.Text = openDialog.FileName;
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            CharacterFrequency[] frequencies = new CharacterFrequency[256];

            string inputFile = InputFileTextBox.Text;
            if (string.IsNullOrEmpty(inputFile))
            {
                MessageBox.Show("Please select an input file.");
                return;
            }

            CompressionResultsListBox.Items.Clear();
            FrequencyResultsListBox.Items.Clear();
            DecompressionResultsListBox.Items.Clear();

            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            string compressedFile = Path.Combine(projectFolder, "Compression.bin");

            try
            {
                // 1. Count character frequencies
                using (FileStream fs = File.OpenRead(inputFile))
                {
                    int b;
                    while ((b = fs.ReadByte()) != -1)
                    {
                        char c = (char)b;
                        if (frequencies[b] == null)
                            frequencies[b] = new CharacterFrequency(c);
                        else
                            frequencies[b].Increment();
                    }
                }

                // 2. Build Huffman tree and code table
                HuffmanTree tree = new HuffmanTree();
                tree.Build(frequencies);
                Dictionary<char, string> codeTable = tree.CodeTable;

                // 3. Compress file
                CompressFile(inputFile, compressedFile, codeTable);

                // 4. Show actual compressed output (hex preview)
                using (FileStream fs = File.OpenRead(compressedFile))
                {
                    byte[] preview = new byte[16];
                    int bytesRead = fs.Read(preview, 0, preview.Length);
                    string hex = BitConverter.ToString(preview, 0, bytesRead).Replace("-", " ");
                    CompressionResultsListBox.Items.Add(hex + (fs.Length > 16 ? " ..." : ""));
                }

                // 5. Show compression stats
                long originalSize = new FileInfo(inputFile).Length;
                long compressedSize = new FileInfo(compressedFile).Length;
                CompressionResultsListBox.Items.Add("-------------------------------------");
                CompressionResultsListBox.Items.Add($"\nOriginal Size: {originalSize} bytes");
                CompressionResultsListBox.Items.Add($"Compressed Size: {compressedSize} bytes");
                CompressionResultsListBox.Items.Add($"Compression Ratio: {(originalSize == 0 ? 0 : (double)compressedSize / originalSize):P2}");
                CompressionResultsListBox.Items.Add($"Compressed file: {compressedFile}");

                // 6. Show frequency table on the right
                for (int ascii = 0; ascii < 256; ascii++)
                {
                    if (frequencies[ascii] != null)
                    {
                        string charDisplay = (ascii < 32 || ascii == 127) ? "" : frequencies[ascii].Character.ToString();
                        string line = (charDisplay == "")
                            ? $"({ascii})\t{frequencies[ascii].Frequency}"
                            : $"{charDisplay}({ascii})\t{frequencies[ascii].Frequency}";
                        FrequencyResultsListBox.Items.Add(line);
                    }
                }

                MessageBox.Show($"File compressed and saved as {compressedFile} in the project folder.");

                lastTree = tree; // Save the tree for decompression

                // ----------- MANUAL NARRATION/ANIMATION STEPS SETUP -----------

                // Prepare narration for each step (customize as needed)
                List<string> narrationList = new List<string>
                {
                    BehindTheScenesNarration.GetCompressionIntro(),
                    BehindTheScenesNarration.HuffmanTree(),
                    // Optionally add more narration strings for each merge step
                };

                // Build animation object (needed for step count)
                _treeAnimation = new HuffmanTreeAnimation(TreeAnimationCanvas, frequencies);

                // Build steps to synchronize narration and animation
                var steps = new List<Action>();

                // Step 0: Show intro and frequency table
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[0];
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                });

                // Step 1: Show tree animation and initial narration
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[1];
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    _treeAnimation.StartAnimation();
                });


                // Create the manual stepper and start it
                _narrationSteps = new NarrationSteps(steps);
                _narrationSteps.Start(); // Show step 0

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void DecompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastTree == null)
            {
                MessageBox.Show("Please compress a file first.");
                return;
            }

            DecompressionResultsListBox.Items.Clear();

            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            string compressedFile = Path.Combine(projectFolder, "Compression.bin");
            string decompressedFile = Path.Combine(projectFolder, "Decompressed.txt");

            try
            {
                DecompressFile(compressedFile, decompressedFile, lastTree.Root);

                // Show decompressed text preview first
                using (StreamReader sr = new StreamReader(decompressedFile))
                {
                    for (int i = 0; i < 10 && !sr.EndOfStream; i++)
                        DecompressionResultsListBox.Items.Add(sr.ReadLine());
                }
                DecompressionResultsListBox.Items.Add("-------------------------------------");
                DecompressionResultsListBox.Items.Add($"Decompressed file: {decompressedFile}");

                MessageBox.Show($"File decompressed to {decompressedFile}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void CompressFile(string inputFile, string compressedFile, Dictionary<char, string> codeTable)
        {
            using (var input = new FileStream(inputFile, FileMode.Open))
            using (var output = new FileStream(compressedFile, FileMode.Create))
            {
                int bitBuffer = 0, bitCount = 0;
                int b;
                while ((b = input.ReadByte()) != -1)
                {
                    char c = (char)b;
                    if (!codeTable.ContainsKey(c))
                        throw new Exception($"No Huffman code for character '{c}' (ASCII {(int)c})");

                    string code = codeTable[c];
                    foreach (char bit in code)
                    {
                        bitBuffer = (bitBuffer << 1) | (bit == '1' ? 1 : 0);
                        bitCount++;
                        if (bitCount == 8)
                        {
                            output.WriteByte((byte)bitBuffer);
                            bitBuffer = 0;
                            bitCount = 0;
                        }
                    }
                }
                if (bitCount > 0)
                {
                    bitBuffer <<= (8 - bitCount);
                    output.WriteByte((byte)bitBuffer);
                }
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
