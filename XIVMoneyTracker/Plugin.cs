using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVMoneyTracker.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FFXIVMoneyTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "MoneyTrack";

        public static Plugin Instance;

        public IDalamudPluginInterface PluginInterface { get; init; }
        public ICommandManager CommandManager { get; init; }
        public IChatGui ChatGui { get; init; }
        public IClientState ClientState { get; init; }
        public Configuration Configuration { get; init; }
        public PluginUI PluginUI { get; init; }
        public InventoryHelper Inventory { get; set; }

        public CharacterModel? CurrentCharacter { get; set; }

        public DateTime LastUpdate { get; set; }

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IChatGui chatGui,
            IClientState clientState)
        {
            //FFXIVClientStructs.Interop.Resolver.GetInstance.Resolve();

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

            this.CommandManager.AddHandler("/mtrack", new CommandInfo(OnWindowCommand)
            {
                HelpMessage = "View the moneytracker"
            });
            this.CommandManager.AddHandler("/mtrack graph", new CommandInfo(OnGraphCommand)
            {
                HelpMessage = "View the moneytracker graph"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.ChatGui.ChatMessage += Chat_OnChatMessage;

            clientState.TerritoryChanged += Player_TerritoryChanged;
            clientState.Login += Player_Login;
            clientState.Logout += Player_Logout;


#if DEBUG
            CurrentCharacter = this.Configuration.Characters
                                .FirstOrDefault();
#endif
        }

        private void Player_Login()
        {
            ClearCache();
        }
        private void Player_Logout(int type, int code)
        {
            ClearCache();
        }
        private void Player_TerritoryChanged(ushort e)
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
            if (this.ClientState.LocalPlayer?.Name.TextValue == null || this.ClientState.LocalPlayer?.HomeWorld.Value.Name == null) return null;


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
                World = this.ClientState.LocalPlayer!.HomeWorld.Value.Name.ToString(),
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

        private void Chat_OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
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
            this.CommandManager.RemoveHandler("/mtrack");
            this.CommandManager.RemoveHandler("/mtrack graph");
        }

        private void OnWindowCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            GetCurrentCharacter()?.LoadAllTransactions();
            this.PluginUI.MoneyLogWindow.Visible = true;
        }

        private void OnGraphCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            GetCurrentCharacter()?.LoadAllTransactions();
            this.PluginUI.MoneyGraphWindow.Visible = true;
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
