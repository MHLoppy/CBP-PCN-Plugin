//license here

using System;
using System.IO;
using System.Windows;
using System.Xml;
using CBPSDK;

namespace CBP_PCN_Plugin
{
    public class PlayerColorNumber : IPluginCBP
    {
        public string PluginTitle => "Player Color/Number Overlay";
        public string PluginVersion => "0.1.1";
        public string PluginAuthor => "MHLoppy";
        public bool CBPCompatible => true;
        public bool DefaultMultiplayerCompatible => true;
        public string PluginDescription => "Adds an overlay to the in-game UI that shows the taunt numbers for each player color."
            + " Note that this plugin's UI edit will apply to the interface.xml file which is CURRENTLY loaded."
            + "\n\nSource code: https://github.com/MHLoppy/CBP-PCN-Plugin";
        public bool IsSimpleMod => true;
        public string LoadResult { get; set; }

        private string workshopPCN;
        private readonly string InterfaceXML = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "interface.xml");
        private readonly string PCNFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CBP", "PCN");
        private readonly string PCNTexture = "RoN color numbers player overlay 2.tga";
        private string loadedPCN;
        private string texPathWorkshop;
        private string texPathLocal;

        private XmlDocument doc = new XmlDocument();

        public void DoSomething(string workshopModsPath, string localModsPath)
        {
            workshopPCN = Path.Combine(workshopModsPath, "2724439438");
            loadedPCN = Path.Combine(PCNFolder, "playercolorsnumbersplugin.txt");
            texPathWorkshop = Path.Combine(workshopPCN, PCNTexture);
            texPathLocal = Path.Combine(PCNFolder, PCNTexture);

            //if folder doesn't exist, make it
            if (!Directory.Exists(PCNFolder))
            {
                try
                {
                    Directory.CreateDirectory(PCNFolder);
                    LoadResult = (PluginTitle + " detected for first time. Doing first-time setup.");
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + ": error creating folder:\n\n" + ex);
                }
            }
            else
            {
                LoadResult = (PCNFolder + " already exists; no action taken.");
            }

            //if file doesn't exist, make one
            if (!File.Exists(loadedPCN))
            {
                try
                {
                    File.WriteAllText(loadedPCN, "0");
                    LoadResult = (PluginTitle + " completed first time setup successfully. Created file:\n" + loadedPCN);
                    //MessageBox.Show(PluginTitle + ": Created file:\n" + loadedMTP);//removed to reduce number of popups for first-time CBP users
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + ": error writing first-time file:\n\n" + ex);
                }
            }
            else
            {
                LoadResult = (loadedPCN + " already exists; no action taken.");
            }

            CheckIfLoaded();//this can be important to do here, otherwise the bool might be accessed without a value depending on how other stuff is set up
        }

        public bool CheckIfLoaded()
        {
            if (File.ReadAllText(loadedPCN) != "0")
            {
                if (!LoadResult.Contains("is loaded"))
                {
                    LoadResult += "\n\n" + PluginTitle + " is loaded.";
                }
                return true;
            }
            else
            {
                if (!LoadResult.Contains("is not loaded"))
                {
                    LoadResult += "\n\n" + PluginTitle + " is not loaded.";
                }
                return false;
            }
        }

        public void LoadPlugin(string workshopModsPath, string localModsPath)
        {
            try
            {
                // copy texture if it's not already there - not done in DoSomething to reduce burden on players not using the plugin
                if (!File.Exists(texPathLocal))
                    File.Copy(texPathWorkshop, texPathLocal);

                BackupXML();
                
                // edit XML
                if (CheckXML() == false)
                {
                    AddXML();
                }
                else
                {
                    MessageBox.Show("It looks like the currently loaded interface.xml file already has the player colors/numbers overlay added.\n\nNo action has been taken.");
                }

                File.WriteAllText(loadedPCN, "1");
                CheckIfLoaded();

                LoadResult = (PluginTitle + " was loaded successfully.");
                MessageBox.Show("UI overlay added successfully.");
            }
            catch (Exception ex)
            {
                LoadResult = (PluginTitle + " had an error loading: " + ex);
                MessageBox.Show("Error loading plugin:\n\n" + ex);
            }
        }

        public void UpdatePlugin(string workshopModsPath, string localModsPath)
        {
            //not needed
        }

        public void UnloadPlugin(string workshopModsPath, string localModsPath)
        {
            try
            {
                // remove texture UPDATE: DON'T
                //File.Delete(texPathLocal);

                // edit XML
                if (CheckXML() == true)
                {
                    RemoveXML();
                }

                File.WriteAllText(loadedPCN, "0");
                CheckIfLoaded();

                LoadResult = (PluginTitle + " was unloaded successfully.");
                MessageBox.Show("UI overlay removed successfully.");
            }
            catch (Exception ex)
            {
                LoadResult = (PluginTitle + " errored while unloading: " + ex);
                MessageBox.Show("Error unloading plugin:\n\n" + ex);
            }
        }

        private void AddXML()
        {
            doc.Load(InterfaceXML);
            XmlNode messageWin = doc.SelectSingleNode(@"ROOT/MESSAGEWIN");

            // ifacetexload
            XmlNode chatColors = doc.CreateNode(XmlNodeType.Element, "IFACETEXLOAD", null);
            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = "TEX_CHATCOLORS";
            XmlAttribute file = doc.CreateAttribute("file");
            file.Value = @"CBP\PCN\RoN color numbers player overlay 2.tga";

            chatColors.Attributes.Append(id);
            chatColors.Attributes.Append(file);

            // button, with subparts ifacetex and fontcolor
            XmlNode button = doc.CreateNode(XmlNodeType.Element, "BUTTON", null);

            XmlAttribute bName = doc.CreateAttribute("name");
            bName.Value = "player_colors_numbers";
            XmlAttribute bDisabled = doc.CreateAttribute("disabled");
            bDisabled.Value = "0";
            XmlAttribute bLeft = doc.CreateAttribute("left");
            bLeft.Value = "330";
            XmlAttribute bTop = doc.CreateAttribute("top");
            bTop.Value = "14";
            XmlAttribute bWidth = doc.CreateAttribute("width");
            bWidth.Value = "180";
            XmlAttribute bHeight = doc.CreateAttribute("height");
            bHeight.Value = "60";

            button.Attributes.Append(bName);
            button.Attributes.Append(bDisabled);
            button.Attributes.Append(bLeft);
            button.Attributes.Append(bTop);
            button.Attributes.Append(bWidth);
            button.Attributes.Append(bHeight);
            
                // subpart ifacetex
                XmlNode ifacetex = doc.CreateNode(XmlNodeType.Element, "IFACETEX", null);

                XmlAttribute iName = doc.CreateAttribute("name");
                iName.Value = "tex";
                XmlAttribute iTexture = doc.CreateAttribute("texture");
                iTexture.Value = "TEX_CHATCOLORS";
                XmlAttribute iLeft = doc.CreateAttribute("left");
                iLeft.Value = "0";
                XmlAttribute iTop = doc.CreateAttribute("top");
                iTop.Value = "0";
                XmlAttribute iWidth = doc.CreateAttribute("width");
                iWidth.Value = "180";
                XmlAttribute iHeight = doc.CreateAttribute("height");
                iHeight.Value = "60";

                ifacetex.Attributes.Append(iName);
                ifacetex.Attributes.Append(iTexture);
                ifacetex.Attributes.Append(iLeft);
                ifacetex.Attributes.Append(iTop);
                ifacetex.Attributes.Append(iWidth);
                ifacetex.Attributes.Append(iHeight);

                // subpart fontcolor
                XmlNode fontColor = doc.CreateNode(XmlNodeType.Element, "FONTCOLOR", null);

                // don't actually remember why I chose this color or even if it matters at all (could just be an easy-to-see color to see if anything went wrong, a bit like blank textures being pink)
                XmlAttribute red = doc.CreateAttribute("r");
                red.Value = "55";
                XmlAttribute green = doc.CreateAttribute("g");
                green.Value = "255";
                XmlAttribute blue = doc.CreateAttribute("b");
                blue.Value = "55";

                fontColor.Attributes.Append(red);
                fontColor.Attributes.Append(green);
                fontColor.Attributes.Append(blue);

                // append the subparts of button
                button.AppendChild(ifacetex);
                button.AppendChild(fontColor);
            
            // append the nodes we just made
            messageWin.AppendChild(chatColors);
            messageWin.AppendChild(button);

            doc.Save(InterfaceXML);
            CleanXML();
        }

        private void RemoveXML()
        {
            doc.Load(InterfaceXML);
            XmlNode messageWin = doc.SelectSingleNode(@"ROOT/MESSAGEWIN");
            XmlNode node1 = doc.SelectSingleNode(@"ROOT/MESSAGEWIN/IFACETEXLOAD[@id='TEX_CHATCOLORS']");
            XmlNode node2 = doc.SelectSingleNode(@"ROOT/MESSAGEWIN/BUTTON[@name='player_colors_numbers']");
            messageWin.RemoveChild(node1);
            messageWin.RemoveChild(node2);

            doc.Save(InterfaceXML);
        }

        private bool CheckXML()
        {
            doc.Load(InterfaceXML);
            XmlNode check1 = doc.SelectSingleNode(@"ROOT/MESSAGEWIN/IFACETEXLOAD[@id='TEX_CHATCOLORS']");
            XmlNode check2 = doc.SelectSingleNode(@"ROOT/MESSAGEWIN/BUTTON[@name='player_colors_numbers']");

            if ((check1 != null) && (check2 != null))
                return true;
            else if ((check1 != null) || (check2 != null))// this should never happen, but let's make sure we know if it ever does
            {
                MessageBox.Show("Unexpected result: only one node was found.", "Unexpected Result", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
                return false;
        }

        private void BackupXML()//for manual recovery in case something doesn't work
        {
            string backup = Path.Combine(PCNFolder, "interface.xml");

            if (!File.Exists(backup))
                File.Copy(InterfaceXML, backup);
        }

        private void CleanXML()
        {
            // makes file compares easier
            string file = File.ReadAllText(InterfaceXML);
            file = file.Replace("/ >", "/>");
            file = file.Replace(" />", "/>");
            File.WriteAllText(InterfaceXML, file);
        }
    }
}
