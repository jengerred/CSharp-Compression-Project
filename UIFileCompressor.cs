// UIFileCompressor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CompressionProject
{
    public class UIFileCompressor
    {
        // UI and animation references
        private TextBox InputFileTextBox;
        private ListBox CompressionResultsListBox;
        private ListBox FrequencyResultsListBox;
        private Canvas TreeAnimationCanvas;
        private TextBlock BehindTheScenesTextBlock;
        private Action<NarrationSteps> SetNarrationSteps;

        // Animation and state
        private HuffmanTreeAnimation treeAnimation;
        private List<char> first20ReadableChars;
        private Dictionary<char, string> codeTable;
        public HuffmanTree LastTree { get; private set; }
        public string LastCompressedFilePath { get; private set; }
        public string lastInputFilePath;
        private Button StepDecompressButton;

        public UIFileCompressor(
            TextBox inputFileTextBox,
            ListBox compressionResultsListBox,
            ListBox frequencyResultsListBox,
            Canvas treeAnimationCanvas,
            TextBlock behindTheScenesTextBlock,
            Action<NarrationSteps> setNarrationSteps,
            Button stepDecompressButton
        )
        {
            InputFileTextBox = inputFileTextBox;
            CompressionResultsListBox = compressionResultsListBox;
            FrequencyResultsListBox = frequencyResultsListBox;
            TreeAnimationCanvas = treeAnimationCanvas;
            BehindTheScenesTextBlock = behindTheScenesTextBlock;
            SetNarrationSteps = setNarrationSteps;

            StepDecompressButton = stepDecompressButton;
            if (StepDecompressButton != null)
                StepDecompressButton.Visibility = Visibility.Collapsed;


        }

        public void Compress(bool isDecompressed)
        {
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
             string decompressedFile = Path.Combine(projectFolder, "Decompressed.txt");
            if (File.Exists(decompressedFile))
            {
                try
                {
                    File.Delete(decompressedFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete previous Decompressed.txt: {ex.Message}");
                }
            }

            string inputFile = null;
            string compressedFile = null;

            // Determine which file is being compressed
            if (!string.IsNullOrEmpty(InputFileTextBox.Text) && File.Exists(InputFileTextBox.Text))
            {
                inputFile = InputFileTextBox.Text;
                compressedFile = Path.Combine(projectFolder, "Compression.bin");
            }
            else if (File.Exists(Path.Combine(projectFolder, "wap.txt")))
            {
                inputFile = Path.Combine(projectFolder, "wap.txt");
                compressedFile = Path.Combine(projectFolder, "CompressionLarge.bin");
            }
            else if (File.Exists(Path.Combine(projectFolder, "SmallDemo.txt")))
            {
                inputFile = Path.Combine(projectFolder, "SmallDemo.txt");
                compressedFile = Path.Combine(projectFolder, "CompressionSmall.bin");
            }
            else
            {
                MessageBox.Show("No valid file found to compress.");
                return;
            }
            lastInputFilePath = inputFile;
            LastCompressedFilePath = compressedFile;

            CompressionResultsListBox.Items.Clear();
            FrequencyResultsListBox.Items.Clear();

            // --- 1. Raw First 20 Characters ---
            List<char> first20RawChars = new List<char>();
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1 && first20RawChars.Count < 20)
                    first20RawChars.Add((char)b);
            }

            // --- 2. First 20 Readable Characters ---
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
                // --- 3. Count Character Frequencies ---
                CharacterFrequency[] frequencies = new CharacterFrequency[256];
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

                // --- 4. Prepare animFrequencies ---
                CharacterFrequency[] animFrequencies = new CharacterFrequency[256];
                foreach (char c in first20ReadableChars)
                {
                    int idx = (int)c;
                    if (animFrequencies[idx] == null)
                        animFrequencies[idx] = new CharacterFrequency(c);
                    else
                        animFrequencies[idx].Increment();
                }

                // --- 5. Build Huffman Tree and Code Table ---
                HuffmanTree tree = new HuffmanTree();
                tree.Build(frequencies);
                codeTable = tree.CodeTable;
                LastTree = tree;

                // --- 6. Compress File ---
                CompressFile(inputFile, compressedFile, codeTable);

                // --- 7. Show Compressed Output (Hex) ---
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
     


        // --- 8. Show Compression Stats ---
        long originalSize = new FileInfo(inputFile).Length;
                long compressedSize = new FileInfo(compressedFile).Length;

                // --- 9. Show Frequency Table ---
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

                // --- 10. Narration and Animation Steps ---
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
                        compressedFileName: Path.GetFileName(compressedFile)
                    ),
                        BehindTheScenesNarration.ReadyToRestore(),
                        BehindTheScenesNarration.DecompressionComplete(),

                };

                treeAnimation = new HuffmanTreeAnimation(TreeAnimationCanvas, animFrequencies, first20ReadableChars);

                var codeAnimation = new HuffmanCodeAnimation(
                    TreeAnimationCanvas,
                    treeAnimation.Root,
                    treeAnimation.GetNodePositions(),
                    treeAnimation.GetLeaves(),
                    treeAnimation.AllNodes,
                    HuffmanTreeAnimation.RowColors,
                    first20ReadableChars,
                    codeTable
                );

                var bitPackingAnimation = new HuffmanBitPackingAnimation(
                    TreeAnimationCanvas,
                    first20ReadableChars,
                    codeAnimation.GetFinalCodes()
                );

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
                    treeAnimation.StartAnimation();
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

                // --- 12. Decompression Narration Step ---
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[6];
                    BehindTheScenesTextBlock.FontWeight = FontWeights.Bold;
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                   // StepDecompressButton.Visibility = Visibility.Visible;
                });

                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = narrationList[7];
                    BehindTheScenesTextBlock.FontWeight = FontWeights.Bold;
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;

                    // Draw the first 20 readable characters in the canvas
                   DrawFirst20ReadableCharacters.DrawFirst20ReadableChars(TreeAnimationCanvas, first20ReadableChars);
                });

                // --- 13. Decompression Animation Step ---
                var decompressionAnimation = new DecompressedTreeAnimation(TreeAnimationCanvas, lastInputFilePath);
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Text = BehindTheScenesNarration.DecompressionTreeExplanation();
                    BehindTheScenesTextBlock.FontWeight = FontWeights.Normal;
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    decompressionAnimation.StartDecompressionAnimation();
                });

                // --- 14. Show Decompressed File Results ---
                steps.Add(() =>
                {
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
                        FrequencyResultsListBox.Items.Add("Decompressed file not found.\nPlease click the \"Decompress\" button to display the full results of your decompressed file.");
                    }

                });

                var narrationSteps = new NarrationSteps(steps);
                SetNarrationSteps(narrationSteps);
                narrationSteps.Start();
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
    }
}
