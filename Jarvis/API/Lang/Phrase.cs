using System;

namespace Jarvis.API.Lang
{
    /// <summary>
    /// Class that represents a phrase of a sentence and its parts.
    /// </summary>
    public class Phrase
    {
        /// <summary>
        /// The text of the phrase.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The words in the phrase.
        /// </summary>
        public Word[] Words { get; }

        /// <summary>
        /// Creates a new phrase object and decompiles the words.
        /// </summary>
        /// <param name="phrase">The actual phrase string</param>
        public Phrase(string phrase)
        {
            Text = phrase;
            string[] split = phrase.Split(Word.separators, StringSplitOptions.RemoveEmptyEntries);
            Words = new Word[split.Length];
            for (int i = 0; i < split.Length; i++) Words[i] = new Word(split[i]);
        }

        /// <summary>
        /// The separator characters used to define where new phrases start.
        /// </summary>
        public readonly static char[] separators = { ',', ';', '(', ')', ':' };
    }
}
