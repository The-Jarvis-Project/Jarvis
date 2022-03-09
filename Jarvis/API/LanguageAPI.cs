using System;
using System.Collections.Generic;

namespace Jarvis.API
{
    public class DecompiledRequest
    {
        public JarvisRequest request;
        public Sentence[] sentences;

        public DecompiledRequest(JarvisRequest request)
        {
            this.request = request;
            string polished = Requests.Polish(request).Request.Trim();
            string[] split = polished.Split(Sentence.separators, StringSplitOptions.RemoveEmptyEntries);
            sentences = new Sentence[split.Length];
            for (int i = 0; i < split.Length; i++) sentences[i] = new Sentence(split[i]);
        }
    }

    public class Sentence
    {
        public enum Type
        {
            Statement, Question, Command, Exclamation
        }

        public string sentence;
        public Phrase[] phrases;
        public Punctuation[] punctuation;
        public Type type;

        public Sentence(string sentence)
        {
            this.sentence = sentence;
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
            phrases = new Phrase[split.Length];
            for (int i = 0; i < split.Length; i++) phrases[i] = new Phrase(split[i]);

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
            punctuation = punctuationList.ToArray();
        }

        public readonly static char[] separators = { '.', '?', '!', '\n' };
        private readonly static char[] punctuationMarks = { '.', '!', '?', ',', '\'', '\"', ';', ':', '`' };
    }

    public class Phrase
    {
        public string phrase;
        public Word[] words;

        public Phrase(string phrase)
        {
            this.phrase = phrase;
            string[] split = phrase.Split(Word.separators, StringSplitOptions.RemoveEmptyEntries);
            words = new Word[split.Length];
            for (int i = 0; i < split.Length; i++) words[i] = new Word(split[i]);
        }

        public readonly static char[] separators = { ',', ';', '(', ')', ':' };
    }

    public class Word
    {
        public string word;
        public bool uppercase, allcaps;

        public Word(string word)
        {
            this.word = word;
            if (word.Length > 0) uppercase = char.IsUpper(word[0]);
            else uppercase = false;
            string caps = word.ToUpper();
            if (caps == word) allcaps = true;
            else allcaps = false;
        }

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

    public class Punctuation
    {
        public enum Usage
        {
            Intentional, Accidental
        }

        public enum Location
        {
            Beginning, Middle, End
        }

        public char mark;
        public Usage usage;
        public Location location;

        public Punctuation(char mark, Usage usage, Location location)
        {
            this.mark = mark;
            this.usage = usage;
            this.location = location;
        }
    }
}
