using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace SentimentAnalysis
{
    class Program
    {


        static void Main(string[] args)
        {
            string acquisitionType = "";
            
            while (acquisitionType != "I" && acquisitionType != "i" &&
                    acquisitionType != "T" && acquisitionType != "t")
            {
                Console.WriteLine("Inputazione manuale [I] o Acquisizione dati da Twitter [T] ?");
                acquisitionType = Console.ReadLine();
            }

            bool manualInput = (acquisitionType == "I" || acquisitionType == "i");
            bool twitterInput = (acquisitionType == "T" || acquisitionType == "t");


            // ------------------------------------------------------------------------------------------------------------
            // 
            // Analisi delle parole manuale
            // 
            // ------------------------------------------------------------------------------------------------------------
            if (manualInput)
            {
                ManualInputAnalysis();
            }
            
            // ------------------------------------------------------------------------------------------------------------
            // 
            // Analisi dei tweet
            // 
            // ------------------------------------------------------------------------------------------------------------
            if (twitterInput)
            {
                TwitterAnalysis();
            }

        }

       
        static void ManualInputAnalysis()
        {
            while (true)
            {
                Console.WriteLine("Inserire una frase da valutare o aggiornare i dizionari \"parola==>[0/+/-]\"");

                string sentence = Console.ReadLine();

                CheckInput(sentence, false);
      
            }
        }


        static void CheckInput(string input, bool silent)
        {
            bool editVocabulary = input.Contains("==>");

            if (!editVocabulary)
            {
                int inputEvaluation =  EvaluateSentence(input, silent);
                Console.WriteLine("Input evaluation: " + inputEvaluation.ToString());
            }
            else
            {
                ChangeVocabulary(input);
            }
        }

        private static int EvaluateSentence(string sentence, bool silent)
        {
            string[] words = sentence.Split(new char[] { ' ', ',', '\'', '.', ';' , '\"'});
            SentimentAnalyzer sentiment = new SentimentAnalyzer();
            int sentenceRating = 0;
            bool negationFound = false;

            foreach (string word in words)
            {
                if (word != "")
                {
                    WordRate val = sentiment.Evaluate(word);

                    if (!(val is NegationWord))
                    {
                        int wordValue = 0;
                        string meaning = "";
                        if (val is PositiveWord)
                        {
                            meaning = "[+]";
                            wordValue = val.Rating;
                        }
                        if (val is NegativeWord)
                        {
                            meaning = "[-]";
                            wordValue = 0 - val.Rating;
                        }
                        if (val is IgnoreWord)
                        {
                            meaning = "[0]";
                            wordValue = 0;
                        }
                        if (!silent)
                        {
                            Console.WriteLine(meaning + " " + word + "\t" + val.Word + "\t" + wordValue.ToString());
                        }
                        sentenceRating += wordValue;
                    }

                    else
                    {
                        negationFound = true;
                    }
                }
            }
            if (negationFound) { sentenceRating = 0 - sentenceRating; }
            return sentenceRating;
        }

        private static void ChangeVocabulary(string input)
        {
            try
            {
                string word = input.Split(new string[] { "==>" }, StringSplitOptions.None)[0];
                string meaning = input.Split(new string[] { "==>" }, StringSplitOptions.None)[1];

                if (meaning == "0")
                {
                    AddToVocabulary(word, Constants.IGNORE_FILE);
                    DeleteFromVocabulary(word, Constants.POSITIVE_FILE);
                    DeleteFromVocabulary(word, Constants.NEGATIVE_FILE);
                    Console.WriteLine("Dizionari aggiornati");
                }
                else if (meaning == "+")
                {
                    AddToVocabulary(word, Constants.POSITIVE_FILE);
                    DeleteFromVocabulary(word, Constants.IGNORE_FILE);
                    DeleteFromVocabulary(word, Constants.NEGATIVE_FILE);
                    Console.WriteLine("Dizionari aggiornati");
                }
                else if (meaning == "-")
                {
                    AddToVocabulary(word, Constants.NEGATIVE_FILE);
                    DeleteFromVocabulary(word, Constants.IGNORE_FILE);
                    DeleteFromVocabulary(word, Constants.POSITIVE_FILE);
                    Console.WriteLine("Dizionari aggiornati");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Selezione non riconosciuta..");
            }
        }

        private static void AddToVocabulary(string word, string filename)
        {
            StreamReader reader = new StreamReader(filename);
            StreamWriter writer = new StreamWriter(filename + ".tmp");

            writer.WriteLine(word);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != word)
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
            File.Delete(filename);
            File.Move(filename + ".tmp", filename);
        }

        private static void DeleteFromVocabulary(string word, string filename)
        {
            StreamReader reader = new StreamReader(filename);
            StreamWriter writer = new StreamWriter(filename +".tmp");
            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != word)
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
            File.Delete(filename);
            File.Move( filename + ".tmp", filename);
        }

        private static void TwitterAnalysis()
        {
            while (true)
            {
                Console.WriteLine("Inserire il termine da cercare");

                string searchString = Console.ReadLine();

                // ------------------------------------------------------------------------------------------------------------
                // 
                // Parte che preleva dati da twitter se serve..
                // 
                // ------------------------------------------------------------------------------------------------------------
                // GENERATE DA base64 partendo da: q5JfavTk8AIP26ccqClibgl6v:xCX3eIifQQCocceviML4TEoSDRndANbXSewXXp5gtjo92M9QO1
                string credentials = "cTVKZmF2VGs4QUlQMjZjY3FDbGliZ2w2djp4Q1gzZUlpZlFRQ29jY2V2aU1MNFRFb1NEUm5kQU5iWFNld1hYcDVndGpvOTJNOVFPMQ==";
                Twitter t = new Twitter();
                t.Authorize(credentials);
                // filtra i retweet, filtra solo roba positiva, cerca "politica"
                //t.Request("https://api.twitter.com/1.1/search/tweets.json?q=politica%20:)%20-filter:retweets&count=50&lang=it", "response.txt"); // result_type=popular&
                string textResponse = t.Request(string.Format("https://api.twitter.com/1.1/search/tweets.json?q={0}-filter:retweets&count=50&lang=it", searchString), "response.txt"); // result_type=popular&
                dynamic responseObj = JsonConvert.DeserializeObject<dynamic>(textResponse);

                double average = 0;
                int counter = 0;
                foreach (dynamic status in responseObj.statuses)
                {
                    Console.WriteLine(status.text);
                    string messageText = status.text;
                    int evaluation = EvaluateSentence(messageText, true);
                    Console.WriteLine("VALUTAZIONE FRASE: " + evaluation.ToString());
                    average += evaluation;
                    counter++;
                }

                average = average / counter;
                Console.WriteLine("VALUTAZIONE MEDIA: " + average.ToString());

                Console.ReadLine();

            }
        }
    }
}
