using System;
using System.Collections.Generic;

namespace Jarvis.API.Lang
{
    /// <summary>
    /// A broken down representation of a string's data into logical pieces.
    /// </summary>
    public class StringComp
    {
        /// <summary>
        /// The original text of the string.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The sentences in this string's text.
        /// </summary>
        public Sentence[] Sentences { get; }

        /// <summary>
        /// The phrases in this string's text.
        /// </summary>
        public Phrase[] Phrases { get; }

        /// <summary>
        /// The words in this string's text.
        /// </summary>
        public Word[] Words { get; }

        /// <summary>
        /// Create a new String Composition object.  Finds sentences, phrases, words, and punctuation.
        /// </summary>
        /// <param name="text">The text to break down</param>
        public StringComp(string text)
        {
            Text = text;
            string[] split = text.Split(Sentence.separators, StringSplitOptions.RemoveEmptyEntries);
            Sentences = new Sentence[split.Length];
            List<Phrase> phraseList = new List<Phrase>();
            List<Word> wordList = new List<Word>();
            for (int i = 0; i < split.Length; i++)
            {
                Sentences[i] = new Sentence(split[i]);
                phraseList.AddRange(Sentences[i].Phrases);
                wordList.AddRange(Sentences[i].Words);
            }
            Phrases = phraseList.ToArray();
            Words = wordList.ToArray();
        }
    }
}
