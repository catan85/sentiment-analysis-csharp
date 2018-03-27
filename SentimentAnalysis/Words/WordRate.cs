using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class WordRate
    {
        string word;

        public string Word
        {
            get { return word; }
            set { word = value; }
        }
        int rating;

        public int Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        bool exactMatch;

        public bool ExactMatch
        {
            get { return exactMatch; }
            set { exactMatch = value; }
        }

        internal void CopyData(WordRate data)
        {
            this.Rating = data.Rating;
            this.Word = data.Word;
            this.ExactMatch = data.ExactMatch;
        }

    }
}
