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

            // Raw First 20 Characters
            List<char> first20RawChars = new List<char>();
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1 && first20RawChars.Count < 20)
                    first20RawChars.Add((char)b);
            }

            // --- 1. Extract the first 20 *readable* (printable) characters ---
            first20ReadableChars = new List<char>();
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1 && first20ReadableChars.Count < 20)
                {
                    char ch = (char)b;
                    if (ch >= 32 && ch <= 126)
                        first20ReadableChars.Add(ch);
                }
            }

            try
            {
                // --- 2. Count character frequencies for ALL characters (0-255) ---
                using (FileStream fs = File.OpenRead(inputFile))
                {
                    int b;
                    while ((b = fs.ReadByte()) != -1)
                    {
                        if (frequencies[b] == null)
                            frequencies[b] = new CharacterFrequency((char)b);
                        else
                            frequencies[b].Increment();
                    }
                }

                // --- 3. Prepare animFrequencies for animation (using first 20 chars) ---
                CharacterFrequency[] animFrequencies = new CharacterFrequency[256];
                foreach (char c in first20ReadableChars)
                {
                    int idx = (int)c;
                    if (animFrequencies[idx] == null)
                        animFrequencies[idx] = new CharacterFrequency(c);
                    else
                        animFrequencies[idx].Increment();
                }

                // --- 4. Build Huffman tree and code table ---
                HuffmanTree tree = new HuffmanTree();
                tree.Build(frequencies);
                codeTable = tree.CodeTable;

                // --- 5. Compress file ---
                CompressFile(inputFile, compressedFile, codeTable);

                // --- 6. Show actual compressed output (full hex output) ---
                using (FileStream fs = File.OpenRead(compressedFile))
                {
                    byte[] allBytes = new byte[fs.Length];
                    int bytesRead = fs.Read(allBytes, 0, allBytes.Length);
                    string[] hexBytes = BitConverter.ToString(allBytes, 0, bytesRead).Split('-');

                    int bytesPerLine = 16;
                    for (int i = 0; i < hexBytes.Length; i += bytesPerLine)
                    {
                        string line = string.Join(" ", hexBytes, i, Math.Min(bytesPerLine, hexBytes.Length - i));
                        CompressionResultsListBox.Items.Add(line);
                    }
                }

                // --- 7. Show compression stats ---
                long originalSize = new FileInfo(inputFile).Length;
                long compressedSize = new FileInfo(compressedFile).Length;

                // --- 8. Show frequency table on the right ---
                FrequencyResultsListBox.Items.Clear();
                for (int ascii = 0; ascii < 256; ascii++)
                {
                    if (frequencies[ascii] != null)
                    {
                        char ch = frequencies[ascii].Character;
                        int freq = frequencies[ascii].Frequency;
                        string charDisplay = (ch >= 32 && ch <= 126) ? ch.ToString() : $"(0x{ascii:X2})";
                        string line = $"{charDisplay}\t{freq}";
                        FrequencyResultsListBox.Items.Add(line);
                    }
                }

                lastTree = tree; // Save the tree for decompression

                // ----------- MANUAL NARRATION/ANIMATION STEPS SETUP -----------

                byte[] compressedBytes = File.ReadAllBytes(compressedFile);
                string compressedHex = BitConverter.ToString(compressedBytes).Replace("-", " ");
                string compressedFileName = Path.GetFileName(compressedFile);

                List<string> narrationList = new List<string>
                {
                    BehindTheScenesNarration.GetCompressionIntro(),
                    BehindTheScenesNarration.ShowFirst20RawCharacters(),
                    BehindTheScenesNarration.HuffmanTree(),
                    BehindTheScenesNarration.EncodeData(),
                    BehindTheScenesNarration.PackBinaryData(),
                    BehindTheScenesNarration.ShowCompressionResults(
                        originalFile: Path.GetFileName(inputFile),
                        originalSize: originalSize,
                        compressedSize: compressedSize,
                        compressedFileName: compressedFileName
                    )
                };

                _treeAnimation = new HuffmanTreeAnimation(TreeAnimationCanvas, animFrequencies, first20ReadableChars);

                var allNodes = _treeAnimation.AllNodes;
                var nodePositions = _treeAnimation.GetNodePositions();
                var leaves = _treeAnimation.GetLeaves();

                var codeAnimation = new HuffmanCodeAnimation(
                    TreeAnimationCanvas,
                    _treeAnimation.Root,
                    nodePositions,
                    leaves,
                    allNodes,
                    HuffmanTreeAnimation.RowColors,
                    first20ReadableChars,
                    codeTable
                );

                var bitPackingAnimation = new HuffmanBitPackingAnimation(
                    TreeAnimationCanvas,
                    first20ReadableChars,
                    codeAnimation.GetFinalCodes()
                );


                // 1. Get the root node of the Huffman tree
                var root = tree.Root;

                // 2. Get the row colors (assuming HuffmanTreeAnimation.RowColors is public/static)
                var rowColors = HuffmanTreeAnimation.RowColors;

                // 3. Generate the bitStream for the first 20 readable characters
                string bitStream = string.Concat(first20ReadableChars.Select(c => codeTable.ContainsKey(c) ? codeTable[c] : ""));

                // 4. Set the number of characters to restore
                int numCharsToRestore = first20ReadableChars.Count;


                var decompressionAnimation = new DecompressedTreeAnimation(TreeAnimationCanvas, inputFile);



                var steps = new List<Action>();

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[0];
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[1];
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    Raw20CharactersAnimation.DrawFirst20RawCharacters(TreeAnimationCanvas, first20RawChars);
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[2];
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    _treeAnimation.StartAnimation();
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[3];
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    codeAnimation.StartCodeAnimation();
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[4];
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    bitPackingAnimation.StartBitPackingAnimation();
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[5];
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                    var compressedHexOutput = new ShowCompressedHexOutput(FrequencyResultsListBox);
                    compressedHexOutput.Display(compressedFile);
                });

                // Add decompression narration steps
                steps.Add(() =>
                {
                    if (!isDecompressed)
                    {
                        BehindTheScenesTextBlock.Text = BehindTheScenesNarration.ReadyToRestore();
                        BehindTheScenesTextBlock.FontWeight = FontWeights.Bold;
                        FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                        TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BehindTheScenesTextBlock.Text = BehindTheScenesNarration.DecompressionComplete();
                        BehindTheScenesTextBlock.FontWeight = FontWeights.Bold;
                        FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                        TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                    }
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = BehindTheScenesNarration.DecompressionTreeExplanation();
                    BehindTheScenesTextBlock.FontSize = 14;
                    BehindTheScenesTextBlock.FontWeight = FontWeights.Normal;
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;

                   
                    decompressionAnimation.StartDecompressionAnimation();


                });
                steps.Add(() =>
                {
                    // Define the decompressed file path at the start of the step
                    string decompressedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decompressed.txt");

                    BehindTheScenesTextBlock.Text = BehindTheScenesNarration.ShowDecompressionResults(
                        Path.GetFileName(decompressedFile)
                    );
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;

                    FrequencyResultsListBox.Items.Clear();
                    if (File.Exists(decompressedFile))
                    {
                        using (StreamReader sr = new StreamReader(decompressedFile))
                        {
                            while (!sr.EndOfStream)
                            {
                                FrequencyResultsListBox.Items.Add(sr.ReadLine());
                            }
                        }
                    }
                    else
                    {
                        FrequencyResultsListBox.Items.Add("Decompressed file not found.");
                    }
                });



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

                using (StreamReader sr = new StreamReader(decompressedFile))
                {
                    while (!sr.EndOfStream)
                    {
                        DecompressionResultsListBox.Items.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

            isDecompressed = true;
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
