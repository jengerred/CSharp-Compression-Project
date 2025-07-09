using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionProject
{
    public class CharacterFrequency
    {
        public char Character { get; }
        public int Frequency { get; private set; }

        public CharacterFrequency(char character)
        {
            Character = character;
            Frequency = 1;
        }

        public void Increment() => Frequency++;
    }

}
