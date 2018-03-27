using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class PositiveWord : WordRate
    {
        public PositiveWord(WordRate word)
        {
            this.CopyData(word);
        }
    }
}
