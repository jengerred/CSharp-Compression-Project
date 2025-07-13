# Huffman Compression Project

Welcome to the **Huffman Compression Project**!  
This project demonstrates how Huffman coding can be used to compress and decompress text files, with a user-friendly interface and step-by-step visualizations to help anyone understand the process—even if you're new to coding.

## Table of Contents

- [What Is Huffman Coding?](#what-is-huffman-coding)
- [How the Project Works](#how-the-project-works)
  - [Compression Steps](#compression-steps)
  - [Decompression Steps](#decompression-steps)
- [Project Structure](#project-structure)
- [How to Use the Application](#how-to-use-the-application)
- [File and Code Overview](#file-and-code-overview)
- [Customization & Extending](#customization--extending)
- [Credits](#credits)

## What Is Huffman Coding?

**Huffman coding** is a popular algorithm for lossless data compression.  
It works by assigning shorter binary codes to more frequent characters and longer codes to less frequent ones, creating a binary tree (the "Huffman tree") that enables efficient encoding and decoding. This method is widely used in file formats like ZIP, JPEG, and MP3.

## How the Project Works

This project provides both the core logic for Huffman compression/decompression and a graphical interface that visually explains each step.

### Compression Steps

1. **File Selection:**  
   Choose a text file to compress, or use one of the provided demo files.

2. **Character Frequency Analysis:**  
   The program scans your file and counts how often each character appears.

3. **Building the Huffman Tree:**  
   Using the frequency data, the program constructs a binary tree where each leaf node represents a character.

4. **Code Table Generation:**  
   Each character is assigned a unique binary code based on its position in the tree.

5. **Encoding:**  
   The original text is replaced with the corresponding Huffman codes, producing a compressed bitstream.

6. **Output:**  
   The compressed data is saved as a binary file, and you can view its contents in hexadecimal.

### Decompression Steps

1. **Tree Restoration:**  
   The same Huffman tree used for compression is used to decode the data.

2. **Bitstream Traversal:**  
   The program reads the compressed file bit by bit, traversing the tree to recover each character.

3. **Output:**  
   The decompressed text is written to a new file, restoring your original data exactly.


## Project Structure

| Layer/Folder                | Key Files / Classes                                                                                                                                                                    | Purpose                                                                                      |
|-----------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| **Core Logic**              | `Compressor.cs`, `Decompressor.cs`, `HuffmanTree.cs`, `CharacterFrequency.cs`, `HuffmanNode.cs`  | Handles all compression and decompression logic, narration step control, and hex output utility |
| **GUI**                     | `UIFileCompressor.cs`, `UIFileDecompressor.cs`, `MainWindow.xaml.cs`, `BehindTheScenesNarration.cs`, `NarrationSteps.cs`,  `ShowCompressedHexOutput.cs`                                                                              | Manages user interface, step-by-step narration, and connects to core logic                   |
| **Animation & Narration**   | `HuffmanTreeAnimation.cs`, `Raw20CharactersAnimation.cs`, `HuffmanCodeAnimation.cs`, `HuffmanBitPackingAnimation.cs`, `DrawFirst20ReadableCharacters.cs`, `DecompressedTreeAnimation.cs` | Provides visual explanations, tree/code/bit animations, and step-by-step walkthroughs        |


## How to Use the Application

#### 1. Launch the Application
- Open the executable file or run the project directly from your IDE.

#### 2. Select a File
- **Browse for Your Own File:**  
  Click the **"Browse"** button to choose any ASCII text file from your computer.
- **Use a Sample File:**  
  Click **"Small Demo"** or **"Large Demo"** to quickly load one of the included sample files.

#### 3. Compress the File
- Click **"Compress"** to begin.
- The application will:
  - Analyze your file and display a frequency table showing how often each character appears.
  - Build and animate the Huffman tree so you can see how the compression structure is formed.
  - Show the compressed file output as hexadecimal values for transparency.

#### 4. Decompress
- Click **"Decompress"** to restore your file from its compressed form.
- The application will:
  - Animate the decompression process, showing how each bit is translated back to the original text.
  - Display the fully recovered text, demonstrating that the process is lossless.

### Tips for Beginners

#### Instant Results
- Compression and decompression happen almost instantly, even with large files.
- To better understand each stage, use the **"Next"** and **"Previous"** buttons to move through the process step by step at your own pace.

#### Learn by Watching
- The built-in animations and narration help you visualize how Huffman coding works.
- Feel free to repeat steps or move back and forth to reinforce your understanding.

#### Try Different Files
- Experiment with a variety of text files to see how character frequency affects compression.
- Files with more repeated characters will usually compress better.

#### No Coding Needed
- You do not need any programming experience to use this tool.
- The interface is designed to be intuitive and accessible for everyone.

#### If You Get Stuck
- If you encounter an error, make sure your file is a plain ASCII text file.
- For best results, try the included demo files first.

Enjoy exploring Huffman coding and discover how data can be compressed and restored—one step at a time!


## File and Code Overview

This project is organized into three main layers: **Core Logic**, **GUI**, and **Animation & Narration**. Each layer is responsible for a distinct aspect of the application, making the codebase easier to understand, maintain, and extend.

---

### Core Logic

This layer contains all the fundamental classes and algorithms responsible for Huffman compression and decompression. These files are independent of any user interface, making them reusable and testable in any context.

- **Compressor.cs**
  - Counts how many times each character appears in the input file.
  - Builds the Huffman tree and generates the code table (mapping characters to binary codes).
  - Handles writing the compressed file to disk.
  - Exposes the frequency table and code table so the GUI can display them.

- **Decompressor.cs**
  - Reads a compressed file and uses the provided Huffman tree to decode and reconstruct the original text.
  - Contains no user interface code, ensuring it can be reused in other projects or tested independently.

- **HuffmanTree.cs**
  - Implements the structure and logic for building and traversing the Huffman tree.
  - Assigns binary codes to each character based on their frequency.

- **HuffmanNode.cs**
  - Represents a single node in the Huffman tree, storing a character and its frequency.
  - Supports tree navigation and sorting by frequency.

- **CharacterFrequency.cs**
  - Tracks how many times each character appears in the input data.
  - Provides the foundational data needed to build the Huffman tree.

---

### GUI

This layer manages all user interactions, visual updates, and the connection between the user interface and the core logic.

- **UIFileCompressor.cs**
  - Coordinates the entire compression process from the user's perspective.
  - Connects UI controls (buttons, text boxes, list boxes, canvas) to the core logic.
  - Handles step-by-step narration and triggers visual animations.

- **UIFileDecompressor.cs**
  - Manages the decompression workflow within the user interface.
  - Displays decompressed results and any error messages to the user.

- **MainWindow.xaml.cs**
  - The main entry point for the application's user interface.
  - Sets up all controls and event handlers, and creates instances of the UI compressor and decompressor.

- **BehindTheScenesNarration.cs**
  - Provides plain-English, step-by-step explanations for each stage of the compression and decompression process.
  - Updates narration text blocks to guide and educate the user.

- **NarrationSteps.cs**
  - Manages the sequence of narration steps, allowing users to move forward and backward through the process.

- **ShowCompressedHexOutput.cs**
  - Displays the compressed file as a hex dump in the UI, giving users a clear view of the actual compressed data.

---

### Animation & Narration

This layer brings the compression process to life with visual explanations, making complex concepts easy to grasp.

- **HuffmanTreeAnimation.cs**
  - Animates the construction of the Huffman tree.
  - Highlights which nodes are merged at each step, visually demonstrating how the tree evolves.

- **Raw20CharactersAnimation.cs**
  - Visually displays the first 20 characters from the input file on a canvas.
  - Helps users preview the file contents, including non-printable or unusual characters.

- **HuffmanCodeAnimation.cs**
  - Animates the assignment of binary codes to each character.
  - Shows how each path through the tree translates to a unique bit sequence.

- **HuffmanBitPackingAnimation.cs**
  - Demonstrates how the binary codes are packed into bytes for file storage.

- **DrawFirst20ReadableCharacters.cs**
  - Animates the display of the first 20 readable characters after decompression, confirming the process worked correctly.

- **DecompressedTreeAnimation.cs**
  - Visualizes the use of the Huffman tree during decompression, showing how the original text is restored step by step.


## Customization & Extending

- **Add More Visualizations:**  
  Extend the animation classes to visualize more steps or add new effects.

- **Support More File Types:**  
  Adapt the core logic to handle Unicode or binary files.

- **Automated Testing:**  
  The core logic is separated from the UI, making it easy to write unit tests.

## Credits

- **Developed by:**  
  Jennifer Gerred

- **Inspired by:**  
  Classic Huffman coding algorithms and educational visualization tools.

**Enjoy exploring data compression with Huffman coding!**  
If you have questions or want to contribute, feel free to open an issue or submit a pull request.
