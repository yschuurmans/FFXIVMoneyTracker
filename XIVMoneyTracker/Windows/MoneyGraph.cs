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
            if (true) //!Visible)
            {
                return;
            }

            if (plugin.CurrentCharacter == null)
            {
                ImGui.Text("No character loaded");
                return;
            }
            try
            {
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

                    ImGui.InputInt("Amount of days shown", ref daysShown, 5, 30, default, ImGuiInputTextFlags.EnterReturnsTrue);


                    Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);

                    var test1 = plugin.CurrentCharacter.Transactions.ToArray();
                    var test2 = plugin.CurrentCharacter.Transactions.Where(x => x.TimeStamp > DateTime.Now.AddDays(-daysShown)).ToArray();


                    float[] graphData = plugin.CurrentCharacter.Transactions
                        .Where(x => x.TimeStamp > DateTime.Now.AddDays(-daysShown))
                        .Select(x => (float)Math.Round(x.Total / divisionFactor, 3)).ToArray();

                    if (graphData.Length > 1 && childScale.X > 0 && childScale.Y > 0)
                    {
                        var overlay = new ImU8String(unitName);
                        ImGui.PlotHistogram(
                            ImU8String.Empty,
                            graphData,
                            0,
                            overlay,
                            float.MaxValue,
                            float.MaxValue,
                            childScale
                        );
                    }
                    else
                    {
                        ImGui.Text("Not enough data points or invalid graph size to show a graph. Please wait until more data is collected.");
                    }
                }
                ImGui.End();
            }
            catch (Exception e)
            {
                if (ImGui.Begin("Money graph", ref this.visible))
                {
                    ImGui.Text("Error drawing graph:");
                    ImGui.Text(e.Message);
                }


            }
        }
    }
}
