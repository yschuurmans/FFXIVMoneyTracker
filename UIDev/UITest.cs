//using ImGuiNET;
//using ImGuiScene;
//using SamplePlugin;
//using SamplePlugin.Windows;
//using System.Collections.Generic;
//using System.Numerics;

//namespace UIDev
//{
//    class UITest : IPluginUIMock
//    {

//        public ChatList ChatListWindow { get; set; }
//        public Settings SettingsWindow { get; set; }
//        public List<Chat> ChatWindows { get; set; }


//        public static void Main(string[] args)
//        {
            
//            UIBootstrap.Inititalize(new UITest(new Plugin()));
//        }

//        private TextureWrap? goatImage;
//        private SimpleImGuiScene? scene;

//        public void Initialize(SimpleImGuiScene scene)
//        {
//            // scene is a little different from what you have access to in dalamud
//            // but it can accomplish the same things, and is really only used for initial setup here

//            // eg, to load an image resource for use with ImGui 
//            this.goatImage = scene.LoadImage("goat.png");

//            scene.OnBuildUI += Draw;

//            this.Visible = true;

//            // saving this only so we can kill the test application by closing the window
//            // (instead of just by hitting escape)
//            this.scene = scene;
//        }

//        public void Dispose()
//        {
//            this.goatImage?.Dispose();
//        }

//        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
//        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
//        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
//        // don't want to imply that is easy or the best way to go usually, so it's not done here either
//        private void Draw()
//        {
//            DrawMainWindow();
//            DrawSettingsWindow();

//            if (!Visible)
//            {
//                this.scene!.ShouldQuit = true;
//            }
//        }

//        #region Nearly a copy/paste of PluginUI
//private Plugin plugin;

//        public ChatList ChatListWindow { get; set; }
//        public Settings SettingsWindow { get; set; }
//        public List<Chat> ChatWindows { get; set; }

//        public PluginUI(Plugin plugin, ImGuiScene.TextureWrap goatImage)
//        {
//            this.plugin = plugin;
//            this.goatImage = goatImage;

//            ChatListWindow = new ChatList(plugin, this);
//            SettingsWindow = new Settings(plugin, this);
//            ChatWindows = new List<Chat>();
//        }

//        public void Dispose()
//        {
//            this.goatImage.Dispose();
//        }

//        public void Draw()
//        {
//            // This is our only draw handler attached to UIBuilder, so it needs to be
//            // able to draw any windows we might have open.
//            // Each method checks its own visibility/state to ensure it only draws when
//            // it actually makes sense.
//            // There are other ways to do this, but it is generally best to keep the number of
//            // draw delegates as low as possible.

//            ChatListWindow.Draw();
//            SettingsWindow.Draw();
//            foreach (var chatWindow in ChatWindows.ToArray())
//            {
//                chatWindow.Draw();
//            }
//        }

//        public void OpenChatWindow(string chatTarget)
//        {
//            var chatWindow = ChatWindows.FirstOrDefault(x => x.ChatTarget == chatTarget);
//            if(chatWindow == null)
//            {
//                var chat = new Chat(plugin, this, chatTarget);
//                ChatWindows.Add(chat);
//                return;
//            }
//            chatWindow.Visible = true;
//        }
//        #endregion
//    }
//}
