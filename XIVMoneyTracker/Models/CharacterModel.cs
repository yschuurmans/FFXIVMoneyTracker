using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FFXIVMoneyTracker.Models
{
    public class CharacterModel
    {
        public string Name { get; set; }
        public string World { get; set; }
        public long CurrentAmount { get; set; }

        //[NonSerialized]
        public List<MoneyTransaction> Transactions = new List<MoneyTransaction>();

        internal void AddTransaction(MoneyTransaction moneyTransaction)
        {
            using (StreamWriter w = File.AppendText(Path.Join(Plugin.Instance.PluginInterface.ConfigDirectory.FullName, $"{Name}_{World}.txt")))
            {
                w.WriteLine(moneyTransaction.ToFileLine());
            }
        }

        public void ExportToCsv()
        {
            LoadAllTransactions(false);

            var csv = new StringBuilder();

            foreach (var transaction in Transactions)
            {
                csv.AppendLine($"{transaction.TimeStamp},{transaction.Total},{transaction.Change}");
            }
            try
            {
                string file = Path.Join(Plugin.Instance.PluginInterface.ConfigDirectory.FullName, "export.csv");
                if (File.Exists(file))
                    File.Delete(file);
                File.WriteAllText(file, csv.ToString());
                Plugin.Instance.ChatGui.Print($"Saved the file to: {Path.Join(Plugin.Instance.PluginInterface.ConfigDirectory.FullName, "export.csv")}");
            }
            catch (Exception e)
            {
                Plugin.Instance.ChatGui.PrintError($"Could not save the file! {Path.Join(Plugin.Instance.PluginInterface.ConfigDirectory.FullName, "export.csv")}");
                Plugin.Instance.ChatGui.PrintError(e.Message);
            }
        }

        public void LoadAllTransactions(bool isInverted)
        {
            string[] lines = File.ReadAllLines(Path.Join(Plugin.Instance.PluginInterface.ConfigDirectory.FullName, $"{Name}_{World}.txt"));
            Transactions = new List<MoneyTransaction>();

            MoneyTransaction? currentTransaction = null;
            DateTime clusterEnd = DateTime.MinValue;

            foreach (string line in lines)
            {
                var transaction = MoneyTransaction.FromFileLine(line);

                if (currentTransaction == null || transaction.TimeStamp > clusterEnd)
                {
                    currentTransaction = transaction;
                    if (isInverted)
                        Transactions.Insert(0, currentTransaction);
                    else
                        Transactions.Add(currentTransaction);
                    clusterEnd = currentTransaction.TimeStamp.AddMinutes(Plugin.Instance.Configuration.ClusterSizeInMinutes);
                }
                else
                {
                    currentTransaction.Change += transaction.Change;
                    currentTransaction.TimeStamp = transaction.TimeStamp;
                    currentTransaction.Total = transaction.Total;
                }
            }

        }
    }
}
