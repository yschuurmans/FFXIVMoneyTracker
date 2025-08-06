using Dalamud.Bindings.ImGui;
using System.Linq;
using System.Numerics;

namespace FFXIVMoneyTracker.Windows
{
    public class MoneyLog : Window
    {
        int clusterSize;

        public MoneyLog(Plugin plugin, PluginUI pluginUI) : base(plugin, pluginUI)
        {
            clusterSize = plugin.Configuration.ClusterSizeInHours;
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
                if (ImGui.Button("Open Graph"))
                {
                    this.pluginUI.MoneyGraphWindow.visible = true;
                }
                ImGui.SameLine();
                bool isInverted = plugin.Configuration.InverseSort;
                if (ImGui.Checkbox("Inverse Sort", ref isInverted))
                {
                    plugin.Configuration.InverseSort = isInverted;
                    plugin.Configuration.Save();
                    plugin.GetCurrentCharacter()?.LoadAllTransactions();
                }

                ImGui.SetNextItemWidth(150);

                if (ImGui.InputInt(new ImU8String("Hour Group Size"), ref clusterSize, 5, 30, ImU8String.Empty, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    plugin.Configuration.ClusterSizeInHours = clusterSize;
                    plugin.Configuration.Save();
                    plugin.GetCurrentCharacter()?.LoadAllTransactions();
                }

                Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);
                if (ImGui.BeginChildFrame(1, childScale, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {

                    if (plugin.CurrentCharacter != null)
                    {
                        var transactionEnumerable = isInverted
                            ? plugin.CurrentCharacter.Transactions.AsEnumerable().Reverse()
                            : plugin.CurrentCharacter.Transactions.AsEnumerable();

                        foreach (var transaction in transactionEnumerable)
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
