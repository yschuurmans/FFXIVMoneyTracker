using FFXIVMoneyTracker.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FFXIVMoneyTracker
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        private Plugin plugin;

        public Settings SettingsWindow { get; set; }

        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;

            SettingsWindow = new Settings(plugin, this);
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            SettingsWindow.Draw();
        }
    }
}
