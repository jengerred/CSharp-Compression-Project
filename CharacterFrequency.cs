using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionProject
{
    // CharacterFrequency keeps track of how many times a specific character appears in the data.
    // This information is essential for building the Huffman tree, which relies on character frequencies
    // to create efficient codes for compression and decompression.
    public class CharacterFrequency
    {
        // The character that this object is tracking.
        public char Character { get; }

        // How many times this character has appeared so far.
        // This value increases as we scan through the data.
        public int Frequency { get; private set; }

        // When we first see a character, we create a CharacterFrequency for it
        // and set its count to 1.
        public CharacterFrequency(char character)
        {
            Character = character;
            Frequency = 1;
        }

        // Call this method each time the character is found again in the data.
        // It simply adds one to the count.
        public void Increment() => Frequency++;
    }
}
