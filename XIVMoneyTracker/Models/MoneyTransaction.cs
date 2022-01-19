using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMoneyTracker.Models
{
    public class MoneyTransaction
    {
        public DateTime TimeStamp { get; set; }
        public long Change { get; set; }
        public long NewTotal { get; set; }

        public override string ToString()
        {
            return $"{TimeStamp} \t {NewTotal.ToString("#,##0")} \t {Change.ToString("+ #,##0;- #,##0;0")}";  
        }
    }
}
