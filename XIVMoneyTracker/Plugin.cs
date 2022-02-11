using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVMoneyTracker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FFXIVMoneyTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "MoneyTrack";

        private const string commandName = "/mtrack";

        public static Plugin Instance;

        public DalamudPluginInterface PluginInterface { get; init; }
        public CommandManager CommandManager { get; init; }
        public ChatGui ChatGui { get; init; }
        public ClientState ClientState { get; init; }
        public Configuration Configuration { get; init; }
        public PluginUI PluginUI { get; init; }
        public InventoryHelper Inventory { get; set; }

        public CharacterModel? CurrentCharacter { get; set; }

        public DateTime LastUpdate { get; set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] Dalamud.Game.Gui.ChatGui chatGui,
            [RequiredVersion("1.0")] Dalamud.Game.ClientState.ClientState clientState)
        {
            FFXIVClientStructs.Resolver.Initialize();

            Instance = this;
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ChatGui = chatGui;
            this.ClientState = clientState;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            Inventory = new InventoryHelper();

            pluginInterface.Create<Service>();

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.PluginUI = new PluginUI(this);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "View the moneytracker"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.ChatGui.ChatMessage += Chat_OnChatMessage;

            clientState.TerritoryChanged += Player_TerritoryChanged;
            clientState.Login += Player_Login;
            clientState.Logout += Player_Logout;
        }

        private void Player_Login(object? sender, System.EventArgs e)
        {
            ClearCache();
        }
        private void Player_Logout(object? sender, System.EventArgs e)
        {
            ClearCache();
        }
        private void Player_TerritoryChanged(object? sender, ushort e)
        {
            ClearCache();
        }

        private void ClearCache()
        {
            Configuration.Save();
            CurrentCharacter = null;
        }

        public CharacterModel? GetCurrentCharacter()
        {
            if (CurrentCharacter != null) return CurrentCharacter;
            if (this.ClientState.LocalPlayer?.Name.TextValue == null || this.ClientState.LocalPlayer?.HomeWorld?.GameData?.Name == null) return null;


            CurrentCharacter = this.Configuration.Characters
                                .FirstOrDefault(x => x.Name == this.ClientState.LocalPlayer?.Name.TextValue);

            if (CurrentCharacter != null)
            {
                Configuration.Save();
                return CurrentCharacter;
            }

            var gil = Inventory.GetGil();
            CurrentCharacter = new CharacterModel()
            {
                Name = this.ClientState.LocalPlayer!.Name.TextValue,
                World = this.ClientState.LocalPlayer!.HomeWorld.GameData.Name.RawString,
                CurrentAmount = gil
            };
            CurrentCharacter.AddTransaction(
                    new MoneyTransaction()
                    {
                        Change = gil,
                        Total = gil,
                        TimeStamp = DateTime.Now
                    });

            this.Configuration.Characters.Add(CurrentCharacter);
            Configuration.Save();

            return CurrentCharacter;
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (type != XivChatType.SystemMessage) return;
            if (LastUpdate.AddSeconds(5) > DateTime.Now) return;
            LastUpdate = DateTime.Now;
            UpdateGil();
        }

        private void UpdateGil()
        {
            var player = GetCurrentCharacter();
            var currentGil = Inventory.GetGil();
            if (player == null || player?.CurrentAmount == currentGil)
                return;

            player!.AddTransaction(new MoneyTransaction
            {
                TimeStamp = DateTime.Now,
                Total = currentGil,
                Change = currentGil - player.CurrentAmount
            });
            player.CurrentAmount = currentGil;
            Configuration.Save();
        }

        public void Dispose()
        {
            this.PluginInterface.UiBuilder.Draw -= DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            this.ChatGui.ChatMessage -= Chat_OnChatMessage;

            this.PluginUI.Dispose();
            this.CommandManager.RemoveHandler(commandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            GetCurrentCharacter()?.LoadAllTransactions();
            this.PluginUI.MoneyLogWindow.Visible = true;
        }

        private void DrawUI()
        {
            this.PluginUI.Draw();
        }

        private void DrawConfigUI()
        {
            GetCurrentCharacter()?.LoadAllTransactions();

            this.PluginUI.MoneyLogWindow.Visible = true;
        }

        public void ExportToFile()
        {
            var character = GetCurrentCharacter();
            //before your loop
            var csv = new StringBuilder();

            foreach (var transaction in character.Transactions)
            {
                csv.AppendLine($"{transaction.TimeStamp},{transaction.Total},{transaction.Change}");
            }

            try
            {
                string file = Path.Join(this.PluginInterface.ConfigDirectory.FullName, "export.csv");
                if (File.Exists(file))
                    File.Delete(file);
                File.WriteAllText(file, csv.ToString());
                ChatGui.Print($"Saved the file to: {Path.Join(this.PluginInterface.ConfigDirectory.FullName, "export.csv")}");
            }
            catch (Exception e)
            {
                ChatGui.PrintError($"Could not save the file! {Path.Join(this.PluginInterface.ConfigDirectory.FullName, "export.csv")}");
                ChatGui.PrintError(e.Message);
            }
        }
    }
}
