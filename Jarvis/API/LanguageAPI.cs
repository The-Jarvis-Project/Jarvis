using System;
using System.Collections.Generic;

namespace Jarvis.API
{
    /// <summary>
    /// A broken down representation of a JarvisRequest's data into logical pieces.
    /// </summary>
    public class DecompiledRequest
    {
        /// <summary>
        /// The original request object this decompiled request is linked to.
        /// </summary>
        public JarvisRequest Request { get; }

        /// <summary>
        /// The sentences in this request's text.
        /// </summary>
        public Sentence[] Sentences { get; }

        /// <summary>
        /// The phrases in this request's text.
        /// </summary>
        public Phrase[] Phrases { get; }

        /// <summary>
        /// The words in this request's text.
        /// </summary>
        public Word[] Words { get; }

        /// <summary>
        /// Create a new Decompiled Request object.  Finds sentences, phrases, words, and punctuation.
        /// </summary>
        /// <param name="request">The request to decompile</param>
        public DecompiledRequest(JarvisRequest request)
        {
            Request = request;
            string polished = Requests.Polish(request).Request.Trim();
            string[] split = polished.Split(Sentence.separators, StringSplitOptions.RemoveEmptyEntries);
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
        /// <param name="sentence">The sentence to decompile</param>
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
        /// Where the punctuation mark is in thi sentence.
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
