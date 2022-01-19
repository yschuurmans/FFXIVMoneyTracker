using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVMoneyTracker.Models;
using System;
using System.Collections.Generic;

namespace FFXIVMoneyTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<CharacterModel> Characters { get; set; } = new List<CharacterModel>();


        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}
