using FFXIVMoneyTracker.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMoneyTracker.Windows
{
    public class MoneyLog : Window
    {
        int clusterSize;

        public MoneyLog(Plugin plugin, PluginUI pluginUI) : base(plugin, pluginUI)
        {
            clusterSize = plugin.Configuration.ClusterSizeInMinutes;
        }

        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }


            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Money log", ref this.visible))
            {
                if (ImGui.Button("Export to CSV"))
                {
                    this.plugin.ExportToFile();
                }
                ImGui.SameLine();
                bool isInverted = plugin.Configuration.InverseSort;
                if (ImGui.Checkbox("Inverse Sort", ref isInverted))
                {
                    plugin.Configuration.InverseSort = isInverted;
                    plugin.Configuration.Save();
                    plugin.GetCurrentCharacter()?.LoadAllTransactions(isInverted);
                }

                ImGui.SetNextItemWidth(150);

                if (ImGui.InputInt("Minute Group Size", ref clusterSize, 5, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    plugin.Configuration.ClusterSizeInMinutes = clusterSize;
                    plugin.Configuration.Save();
                    plugin.GetCurrentCharacter()?.LoadAllTransactions(isInverted);
                }

                Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);
                if (ImGui.BeginChildFrame(1, childScale, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {

                    if (plugin.CurrentCharacter != null)
                    {
                        foreach (var transaction in plugin.CurrentCharacter.Transactions)
                        {
                            ImGui.Text(transaction.ToString());
                        }
                    }
                }
                ImGui.EndChildFrame();



            }
            ImGui.End();
        }
    }
}
