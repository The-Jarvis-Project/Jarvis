namespace Jarvis.API.Lang
{
    /// <summary>
    /// Class that represents a word in a phrase.
    /// </summary>
    public class Word
    {
        /// <summary>
        /// The word.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Whether or not this word has an uppercase letter in the beginning.
        /// </summary>
        public bool Uppercase { get; }

        /// <summary>
        /// Whether or not the entire word is capitalized.
        /// </summary>
        public bool AllCaps { get; }

        /// <summary>
        /// Creates a new word object and finds attributes.
        /// </summary>
        /// <param name="word">The word as a string</param>
        public Word(string word)
        {
            Text = word;
            if (word.Length > 0) Uppercase = char.IsUpper(word[0]);
            else Uppercase = false;
            string caps = word.ToUpper();
            if (caps == word) AllCaps = true;
            else AllCaps = false;
        }

        /// <summary>
        /// The separator characters used to define where new words start.
        /// </summary>
        public readonly static char[] separators = { ' ',
        '.',
        ',',
        '?',
        '!',
        '\'',
        '\"',
        '<',
        '>',
        ':',
        ';',
        '[',
        ']',
        '{',
        '}',
        '~',
        '`',
        '+',
        '|',
        '=',
        '(',
        ')',
        '$',
        '@',
        '#',
        '%',
        '*',
        '\\',
        '/' };
    }
}
