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
        static List<string> wordList = new List<string>();
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
            // Analisi delle parole
            // 
            // ------------------------------------------------------------------------------------------------------------
            if (manualInput)
            {
                while (true)
                {
                    Console.WriteLine("Inserire una frase");

                    string sentence = Console.ReadLine();

                    int sentenceValue = EvaluateSentence(sentence, false);
                    if (sentenceValue != 99999)
                    {
                        Console.WriteLine("Valutazione: " + sentenceValue.ToString());
                    }
                    
                
                }
            }

            if (twitterInput)
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
                    string textResponse = t.Request(string.Format("https://api.twitter.com/1.1/search/tweets.json?q={0}-filter:retweets&count=50&lang=it",searchString), "response.txt"); // result_type=popular&
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

        static int EvaluateSentence(string sentence, bool silent)
        {
            if (!(sentence.StartsWith("[[") && sentence.EndsWith("]]") && sentence.Contains("=") &&
                (sentence.Contains("+") || sentence.Contains("-") || sentence.Contains("i"))))
            {
                wordList.Clear();
                string[] words = sentence.Split(new char[] { ' ', ',', '\'', '.', ';' });
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
                            if (val is PositiveWord)
                            {
                                if (!silent)
                                {
                                    Console.Write("[+] ");
                                }
                                wordValue = val.Rating;
                            }
                            if (val is NegativeWord)
                            {
                                if (!silent)
                                {
                                    Console.Write("[-] ");
                                }
                                wordValue = 0 - val.Rating;
                            }
                            if (val is IgnoreWord)
                            {
                                if (!silent)
                                {
                                    Console.Write("[i] ");
                                }
                                wordValue = 0;
                            }
                            if (!silent)
                            {
                                Console.WriteLine(wordList.Count.ToString() + " > " + word + "\t" + val.Word + "\t" + wordValue.ToString());
                            }
                            wordList.Add(word);
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
            else
            {
                try
                {
                    // Se inizia per [[ e finisce per ]]
                    sentence = sentence.Replace("[[", "");
                    sentence = sentence.Replace("]]", "");
                    int index = int.Parse(sentence.Split('=')[0]);
                    string listType = sentence.Split('=')[1];

                    if (listType == "i")
                    {
                        System.IO.File.AppendAllLines("ignore.txt", new string[] { wordList[index] });
                    }
                    else if (listType == "+")
                    {
                        System.IO.File.AppendAllLines("positive.txt", new string[] { wordList[index] });
                    }
                    else if (listType == "-")
                    {
                        System.IO.File.AppendAllLines("negative.txt", new string[] { wordList[index] });
                    }
                    Console.WriteLine("Dizionari aggiornati");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Selezione non riconosciuta..");
                }

                return 0;
            }
        }
    }
}
