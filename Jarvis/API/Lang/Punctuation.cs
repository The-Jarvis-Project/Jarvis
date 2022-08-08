namespace Jarvis.API.Lang
{
    /// <summary>
    /// Class to represent a punctuation mark
    /// </summary>
    public class Punctuation
    {
        /// <summary>
        /// How the punctuation mark is used in a sentence.
        /// </summary>
        public enum Usage
        {
            Intentional, Accidental
        }

        /// <summary>
        /// Where the punctuation mark is in a sentence.
        /// </summary>
        public enum Location
        {
            Beginning, Middle, End
        }

        /// <summary>
        /// The punctuation mark.
        /// </summary>
        public char Mark { get; }

        /// <summary>
        /// How this punctuation mark is used in the sentence.
        /// </summary>
        public Usage Use { get; }

        /// <summary>
        /// Where the punctuation mark is in the sentence.
        /// </summary>
        public Location Loc { get; }

        /// <summary>
        /// Creates a new punctuation object.
        /// </summary>
        /// <param name="mark">The punctuation mark character</param>
        /// <param name="usage">How the punctuation mark is used</param>
        /// <param name="location">Where the punctuation mark is</param>
        public Punctuation(char mark, Usage usage, Location location)
        {
            Mark = mark;
            Use = usage;
            Loc = location;
        }
    }
}
