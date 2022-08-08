using System;
using System.Collections.Generic;

namespace Jarvis.API.Lang
{
    /// <summary>
    /// Contains a literal sentence string and also the components of the sentence.
    /// </summary>
    public class Sentence
    {
        /// <summary>
        /// Sentences in English are one of four types.
        /// </summary>
        public enum Type
        {
            Statement, Question, Command, Exclamation
        }

        /// <summary>
        /// The sentence text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The phrases in the sentence.
        /// </summary>
        public Phrase[] Phrases { get; }

        /// <summary>
        /// The punctuation marks in the sentence.
        /// </summary>
        public Punctuation[] PunctuationMarks { get; }

        /// <summary>
        /// The words in the sentence.
        /// </summary>
        public Word[] Words { get; }

        /// <summary>
        /// The type of sentence this sentence is.
        /// </summary>
        public Type type;

        /// <summary>
        /// Create a setence object and compute the parts of the sentence.
        /// </summary>
        /// <param name="sentence">The sentence to break down</param>
        public Sentence(string sentence)
        {
            Text = sentence;
            switch (sentence[sentence.Length - 1])
            {
                case '.':
                    type = Type.Statement;
                    break;
                case '?':
                    type = Type.Question;
                    break;
                case '!':
                    type = Type.Exclamation;
                    break;
                default:
                    type = Type.Statement;
                    break;
            }

            string[] split = sentence.Split(Phrase.separators, StringSplitOptions.RemoveEmptyEntries);
            Phrases = new Phrase[split.Length];
            List<Word> wordList = new List<Word>();
            for (int i = 0; i < split.Length; i++)
            {
                Phrases[i] = new Phrase(split[i]);
                wordList.AddRange(Phrases[i].Words);
            }
            Words = wordList.ToArray();

            List<Punctuation> punctuationList = new List<Punctuation>();
            for (int i = 0; i < sentence.Length; i++)
            {
                for (int p = 0; p < punctuationMarks.Length; p++)
                {
                    if (sentence[i] == punctuationMarks[p])
                    {
                        Punctuation.Location loc = Punctuation.Location.Beginning;
                        if (i == sentence.Length - 1) loc = Punctuation.Location.End;
                        else if (i != 0) loc = Punctuation.Location.Middle;
                        punctuationList.Add(new Punctuation(sentence[i], Punctuation.Usage.Intentional, loc));
                    }
                }
            }
            PunctuationMarks = punctuationList.ToArray();
        }

        /// <summary>
        /// The separator characters used to define where new sentences start.
        /// </summary>
        public readonly static char[] separators = { '.', '?', '!', '\n' };
        private readonly static char[] punctuationMarks = { '.', '!', '?', ',', '\'', '\"', ';', ':', '`' };
    }
}
