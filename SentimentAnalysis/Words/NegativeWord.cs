using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class NegativeWord : WordRate
    {
        public NegativeWord(WordRate word)
        {
            this.CopyData(word);
        }
    }
}
