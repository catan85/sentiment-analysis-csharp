﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    class NegationWord : WordRate
    {

        public NegationWord(WordRate word)
        {
            this.CopyData(word);
        }
    }
}
