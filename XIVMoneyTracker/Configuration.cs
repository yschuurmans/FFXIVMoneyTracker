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
        public int ClusterSizeInHours { get; set; } = 24;

        public List<CharacterModel> Characters { get; set; } = new List<CharacterModel>();
        public bool InverseSort { get; internal set; } = true;


        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}
