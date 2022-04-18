using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMoneyTracker.Models
{
    public class MoneyTransaction
    {
        public DateTime TimeStamp { get; set; }
        public long Change { get; set; }
        public long Total { get; set; }

        public override string ToString()
        {
            return $"{TimeStamp} \t {Total.ToString("#,##0")} \t {Change.ToString("+ #,##0;- #,##0;0")}";  
        }

        public string ToFileLine()
        {
            return $"{TimeStamp.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)};{Total};{Change}";
        }
        public static MoneyTransaction FromFileLine(string line)
        {
            string[] parts = line.Split(";");
            return new MoneyTransaction
            {
                TimeStamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
                Total = Convert.ToInt64(parts[1]),
                Change = Convert.ToInt64(parts[2])
            };
        }
    }
}
