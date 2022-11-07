using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace FFXIVMoneyTracker.Windows
{
    public class MoneyGraph : Window
    {
        int daysShown = 90;

        public MoneyGraph(Plugin plugin, PluginUI pluginUI) : base(plugin, pluginUI)
        {
        }

        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            int average = (int)plugin.CurrentCharacter.Transactions.Average(x => x.Total);
            string unitName = "gil";
            float divisionFactor = 1;
            if (average > 1000 && average < 1000000)
            {
                divisionFactor = 1000;
                unitName = "thousand gil";
            }
            if (average > 1000000)
            {
                divisionFactor = 1000000;
                unitName = "million gil";
            }


            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Money graph", ref this.visible))
            {
                if (ImGui.InputInt("Amount of days shown", ref daysShown, 5, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                }

                Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);
                //Some testing regarding graphs, don't mind this
                float[] graphData = plugin.CurrentCharacter.Transactions
                    .Where(x => x.TimeStamp > DateTime.Now.AddDays(-daysShown))
                    .Select(x => (float)Math.Round(x.Total / divisionFactor, 3)).ToArray();

                ImGui.PlotHistogram("", ref graphData[0], graphData.Length, 0, unitName, 0, graphData.Max(), childScale);

            }
            ImGui.End();
        }
    }
}
