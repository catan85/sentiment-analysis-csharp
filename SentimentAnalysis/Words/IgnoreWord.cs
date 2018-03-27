using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class IgnoreWord : WordRate
    {
        public IgnoreWord(WordRate word)
        {
            this.CopyData(word);
        }
    }
}
