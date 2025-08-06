using Dalamud.Bindings.ImGui;
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

            if (plugin.CurrentCharacter == null)
            {
                ImGui.Text("No character loaded");
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
                if (ImGui.InputInt("Amount of days shown", ref daysShown, 5, 30, default, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                }

                Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);

                float[] graphData = plugin.CurrentCharacter.Transactions
                    .Where(x => x.TimeStamp > DateTime.Now.AddDays(-daysShown))
                    .Select(x => (float)Math.Round(x.Total / divisionFactor, 3)).ToArray();

                ImGui.PlotHistogram(ImU8String.Empty, graphData, 0, new ImU8String(unitName), float.MaxValue, float.MaxValue, childScale);
            }
            ImGui.End();
        }
    }
}
