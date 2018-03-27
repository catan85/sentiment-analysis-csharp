using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class SentimentAnalyzer
    {
        private string[] positiveDictionary;
        private string[] negativeDictionary;
        private string[] ignoreDictionary;
        private string[] negationDictionary = new string[] { "non", "Non" };
 
        public WordRate Evaluate(string word)
        {
            InitializeDictionaries();

            WordRate negationWord = new NegationWord(FindWord(word, negationDictionary));

            if (negationWord.ExactMatch)
            {
                return negationWord;
            }

            WordRate ignoreWord = new IgnoreWord(FindWord(word, ignoreDictionary));
            if (ignoreWord.ExactMatch)
            {
                return ignoreWord;
            }

            WordRate positiveWord = new PositiveWord(FindWord(word, positiveDictionary));
            WordRate negativeWord = new NegativeWord(FindWord(word, negativeDictionary));

            // Se la parola coincide al 100% ha la priorità su una più lunga con parziale corrispondenza
            if (positiveWord.ExactMatch)
            {
                return positiveWord;
            }
            if (negativeWord.ExactMatch)
            {
                return negativeWord;
            }

            if (positiveWord.Rating > negativeWord.Rating)
            {
                return positiveWord;
            }
            else
            {
                return negativeWord;
            }


            // aggiungere ignore list (parole neutrali che non modificano il significato della parola
        }
        
        private void InitializeDictionaries()
        {

            positiveDictionary = System.IO.File.ReadAllLines("positive.txt");

            negativeDictionary = System.IO.File.ReadAllLines("negative.txt");
            
            ignoreDictionary = System.IO.File.ReadAllLines("ignore.txt");
 
        }

        private WordRate FindWord(string input, string[] words)
        {
       
            char[] inputChars = input.ToLower().ToCharArray();

            string bestWord = "";
            int bestWordCorrespondingChars  = 0;

            foreach (string word in words)
            {
                char[] wordChars = word.ToCharArray();
                int correspondingChars = 0;
               
                for (int i = 0; i < wordChars.Length; i++)
                {
                    if (i < wordChars.Length && i < inputChars.Length)
                    {
                        if (inputChars[i] == wordChars[i])
                        {
                            correspondingChars++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (Math.Abs(correspondingChars - input.Length) < Math.Abs(bestWordCorrespondingChars - input.Length))
                //if (correspondingChars > bestWordCorrespondingChars)
                {
                    bestWordCorrespondingChars = correspondingChars;
                    bestWord = word;
                }else if (word == input)
                {
                    // la corrispondenza esatta ha la meglio su quelle più lunghe
                    bestWordCorrespondingChars = correspondingChars;
                    bestWord = word;
                    break;
                }
            }
            
            if ((bestWordCorrespondingChars >= input.Length * 0.7) &&   // Spostare questo controllo sulla determinazione della miglior corrispondenza!
                ((bestWord.Length * 0.7 ) <= input.Length))
            {
                return new WordRate()
                {
                    Word = bestWord,
                    Rating = bestWordCorrespondingChars,
                    ExactMatch = (bestWord.ToLower() == input.ToLower())
                };
            }
            else
            {
                return new WordRate()
                {
                    Word = bestWord,
                    Rating = 0,
                    ExactMatch = false
                };
            }
            

        }

    }
}
