using CompressionProject.GUI.Animation;
using CompressionProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CompressionProject.GUI
{
    // This class manages the user interface for compressing files with Huffman coding.
    // It connects the GUI controls, handles all animation and narration steps,
    // and delegates the actual compression work to the core Compressor class.
    public class UIFileCompressor
    {
        // --- UI Controls and Animation References ---

        // Where the user enters or selects the file to compress.
        private TextBox InputFileTextBox;

        // Shows the results of compression (e.g., compressed hex output).
        private ListBox CompressionResultsListBox;

        // Shows the frequency table or other step results.
        private ListBox FrequencyResultsListBox;

        // Where we draw and animate the Huffman tree and related visuals.
        private Canvas TreeAnimationCanvas;

        // Where we show step-by-step narration and explanations.
        private TextBlock BehindTheScenesTextBlock;

        // Lets us set up the step-by-step narration (walkthrough).
        private Action<NarrationSteps> SetNarrationSteps;

        // Buttons for stepping through the narration and decompression.
        private Button StepDecompressButton;
        private Button NextStepButton;
        private Button PreviousStepButton;

        // --- Animation and State ---

        // Stores the Huffman tree animation instance for UI use.
        private HuffmanTreeAnimation treeAnimation;

        // Stores the first 20 readable characters from the file for preview and animation.
        private List<char> first20ReadableChars;

        // The Huffman code table (character to code mapping) for the last compression.
        private Dictionary<char, string> codeTable;

        // The last Huffman tree built, for use in decompression and animation.
        public HuffmanTree LastTree { get; private set; }

        // The path to the last compressed file.
        public string LastCompressedFilePath { get; private set; }

        // The path to the last input file compressed.
        public string lastInputFilePath;

        // --- Core Compression Logic ---
        // The Compressor instance that does the actual Huffman compression.
        private Compressor compressor;

        // --- Constructor: Sets up the UIFileCompressor with all needed UI controls and delegates. ---
        public UIFileCompressor(
            TextBox inputFileTextBox,
            ListBox compressionResultsListBox,
            ListBox frequencyResultsListBox,
            Canvas treeAnimationCanvas,
            TextBlock behindTheScenesTextBlock,
            Action<NarrationSteps> setNarrationSteps,
            Button stepDecompressButton,
            Button nextStepButton,
            Button previousStepButton
        )
        {
            InputFileTextBox = inputFileTextBox;
            CompressionResultsListBox = compressionResultsListBox;
            FrequencyResultsListBox = frequencyResultsListBox;
            TreeAnimationCanvas = treeAnimationCanvas;
            BehindTheScenesTextBlock = behindTheScenesTextBlock;
            SetNarrationSteps = setNarrationSteps;

            StepDecompressButton = stepDecompressButton;
            NextStepButton = nextStepButton;
            PreviousStepButton = previousStepButton;

            compressor = new Compressor();

            // Hide the decompression step button until it's needed in the UI flow.
            if (StepDecompressButton != null)
                StepDecompressButton.Visibility = Visibility.Collapsed;
        }

        // Helper method to show or hide the "Next" and "Previous" narration buttons.
        // These are shown/hidden depending on whether the user can go forward/back in the steps.
        private void SetStepButtonVisibility(bool showNext, bool showPrevious)
        {
            if (NextStepButton != null)
                NextStepButton.Visibility = showNext ? Visibility.Visible : Visibility.Collapsed;
            if (PreviousStepButton != null)
                PreviousStepButton.Visibility = showPrevious ? Visibility.Visible : Visibility.Collapsed;
        }

        // --- Main Compression Method ---
        // This method is called to compress the selected file, update the UI, and start narration/animation.
        public void Compress(bool isDecompressed)
        {
            // Set up the paths for input and output files.
            string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
            string decompressedFile = Path.Combine(projectFolder, "Decompressed.txt");
            if (File.Exists(decompressedFile))
            {
                try { File.Delete(decompressedFile); }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete previous Decompressed.txt: {ex.Message}");
                }
            }

            string inputFile = null;
            string compressedFile = null;

            // --- Step 0: Figure out which file to compress ---
            // If the user picked a file, use that. Otherwise, use a demo file.
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

            // Clear previous results in the UI so we start fresh.
            CompressionResultsListBox.Items.Clear();
            FrequencyResultsListBox.Items.Clear();

            // --- 1. Preview the first 20 raw characters from the file (for animation/preview) ---
            // This helps users see what kind of data is in their file, even if it's unreadable.
            List<char> first20RawChars = new List<char>();
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1 && first20RawChars.Count < 20)
                    first20RawChars.Add((char)b);
            }

            // --- 2. Collect the first 20 readable (printable) characters for display ---
            // This is used for a more user-friendly preview and for the animation steps.
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
                // --- 3. Compress the file using the core logic (Compressor) ---
                // This runs the actual Huffman coding algorithm and saves the results.
                compressor.CompressFile(inputFile, compressedFile);
                LastTree = compressor.LastTree;
                codeTable = compressor.LastCodeTable;

                // --- 4. Prepare frequencies for animation (just for the first 20 readable chars) ---
                // This is for animating the tree and showing how codes are assigned.
                CharacterFrequency[] animFrequencies = new CharacterFrequency[256];
                foreach (char c in first20ReadableChars)
                {
                    int idx = c;
                    if (animFrequencies[idx] == null)
                        animFrequencies[idx] = new CharacterFrequency(c);
                    else
                        animFrequencies[idx].Increment();
                }

                // --- 5. Show the compressed file as hex output in the UI ---
                // This lets users see the actual bytes that make up the compressed file.
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

                // --- 6. Show compression statistics (original vs. compressed size) ---
                long originalSize = new FileInfo(inputFile).Length;
                long compressedSize = new FileInfo(compressedFile).Length;

                // --- 7. Show the frequency table in the UI ---
                // This table shows how often each character appeared in the file.
                // It's important for understanding how Huffman coding works.
                FrequencyResultsListBox.Items.Clear();
                var freqTable = compressor.LastFrequencies;
                foreach (var freq in freqTable)
                {
                    if (freq != null)
                    {
                        char ch = freq.Character;
                        int f = freq.Frequency;
                        string charDisplay = ch >= 32 && ch <= 126 ? ch.ToString() : $"(0x{(int)ch:X2})";
                        string line = $"{charDisplay}\t{f}";
                        FrequencyResultsListBox.Items.Add(line);
                    }
                }

                // --- 8. Set up and run the step-by-step narration and animation ---
                // Each step below controls what is visible in the UI and what is being explained.
                // We hide or show the FrequencyResultsListBox and TreeAnimationCanvas to avoid clutter,
                // and to focus the user's attention on either the table or the animation.

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

                // Step 1: Show the intro narration and the frequency table.
                // Only the "Next" button is visible to move forward.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, false); // Show Next, hide Previous
                    BehindTheScenesNarration.SetCompressionIntro(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Visible; // Show table
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;   // Hide animation
                });

                // Step 2: Show the first 20 raw characters as an animation.
                // Both "Next" and "Previous" buttons are visible so users can go back or forward.
                // We hide the frequency table to focus on the animation.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetFirst20RawCharacters(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed; // Hide table
                    TreeAnimationCanvas.Visibility = Visibility.Visible;       // Show animation
                    Raw20CharactersAnimation.DrawFirst20RawCharacters(TreeAnimationCanvas, first20RawChars);
                });

                // Step 3: Show the Huffman tree being built as an animation.
                // Again, both navigation buttons are visible.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetHuffmanTree(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    treeAnimation.StartAnimation();
                });

                // Step 4: Show how each character gets its binary code (animation).
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetEncodeData(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    codeAnimation.StartCodeAnimation();
                });

                // Step 5: Show how the binary codes are packed into bytes (animation).
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetPackBinaryData(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    bitPackingAnimation.StartBitPackingAnimation();
                });

                // Step 6: Show the final compressed output and stats.
                // Show the frequency table (now used for hex output) and hide the animation.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetCompressionResults(
                        BehindTheScenesTextBlock,
                        Path.GetFileName(inputFile),
                        originalSize,
                        compressedSize,
                        Path.GetFileName(compressedFile)
                    );
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                    var compressedHexOutput = new ShowCompressedHexOutput(FrequencyResultsListBox);
                    compressedHexOutput.Display(compressedFile);
                });

                // Step 7: Tell the user they're ready to decompress.
                // Hide both the table and animation to focus on the message.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetReadyToRestore(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                });

                // Step 8: Show the decompression complete animation.
                // Show the tree animation with the decoded characters.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetDecompressionComplete(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    DrawFirst20ReadableCharacters.DrawFirst20ReadableChars(TreeAnimationCanvas, first20ReadableChars);
                });

                // Step 9: Explain how the tree is used to decode the file.
                // Show the animation for the decompression process.
                var decompressionAnimation = new DecompressedTreeAnimation(TreeAnimationCanvas, lastInputFilePath);
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    BehindTheScenesNarration.SetDecompressionTreeExplanation(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Visible;
                    decompressionAnimation.StartDecompressionAnimation();
                });

                // Step 10: Show the actual decompressed file contents.
                // Show the frequency table with the decompressed file's content.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(true, true);
                    string decompressedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decompressed.txt");
                    BehindTheScenesNarration.SetDecompressionResults(
                        BehindTheScenesTextBlock,
                        Path.GetFileName(decompressedFilePath)
                    );
                    FrequencyResultsListBox.Visibility = Visibility.Visible;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;

                    FrequencyResultsListBox.Items.Clear();
                    if (File.Exists(decompressedFilePath))
                    {
                        using (StreamReader sr = new StreamReader(decompressedFilePath))
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

                // Step 11: Show the final conclusion narration.
                // Hide both table and animation to focus on the wrap-up message.
                steps.Add(() =>
                {
                    BehindTheScenesTextBlock.Visibility = Visibility.Visible;
                    SetStepButtonVisibility(false, true);
                    BehindTheScenesNarration.SetFinalConclusion(BehindTheScenesTextBlock);
                    FrequencyResultsListBox.Visibility = Visibility.Collapsed;
                    TreeAnimationCanvas.Visibility = Visibility.Collapsed;
                });

                // Start the narration/animation sequence.
                var narrationSteps = new NarrationSteps(steps);
                SetNarrationSteps(narrationSteps);
                narrationSteps.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
