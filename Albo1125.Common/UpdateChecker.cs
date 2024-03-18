using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Net;
using System.Threading;
using Albo1125.Common.CommonLibrary;
using System.Xml.Linq;
using static Albo1125.Common.UpdateEntry;

namespace Albo1125.Common
{
    /// <summary>
    /// Provides functionality to check for updates and display update information.
    /// </summary>
    public class UpdateChecker
    {

        private static int _index = -1;

        /// <summary>
        /// Displays update information to the user.
        /// </summary>
        /// <remarks>
        /// This method will display a popup dialog to the user,
        /// showing information about available updates for modifications.
        /// </remarks>
        public static void DisplayUpdates()
        {
            var modsWithErrors = PluginsDownloadLink.Select(x => x.Item1).Distinct().ToList().Aggregate("", (current, s) => current + (s + ","));
            var popup = new Popup("Albo1125.Common Update Check", "Updates are available for the following modifications: " + modsWithErrors, new List<string>() { "Continue" }, false, false, NextUpdateCallback);
            popup.Display();
        }


        /// <summary>
        /// Callback method for handling the next update in the popup.
        /// </summary>
        /// <param name="p">The Popup instance associated with the callback.</param>
        private static void NextUpdateCallback(Popup p)
        {
            switch (p.IndexOfGivenAnswer)
            {
                case 0:
                {
                    Game.LogTrivial("Continue pressed");
                    _index++;
                    if (PluginsDownloadLink.Count > _index)
                    {
                        var popup = new Popup("Albo1125.Common Update Check", "Update " + (_index + 1) + ": " + PluginsDownloadLink[_index].Item1, new List<string>() { "Continue", "Go to download page" },
                            false, false, NextUpdateCallback);
                        popup.Display();
                    }
                    else
                    {
                        var popup = new Popup("Albo1125.Common Update Check", "Please install updates to maintain stability and don't request support for old versions.",
                            new List<string>() { "-", "Open installation/troubleshooting video tutorial", "Continue to game.", "Delay next update check by a week.", "Delay next update check by a month.",
                                "Fully disable update checks (not recommended)." }, false, false, NextUpdateCallback);
                        popup.Display();
                    }

                    break;
                }
                case 1:
                {
                    Game.LogTrivial("GoToDownload pressed.");
                    if (PluginsDownloadLink.Count > _index && _index >= 0)
                    {
                        Process.Start(PluginsDownloadLink[_index].Item2);
                    }
                    else
                    {
                        Process.Start("https://youtu.be/af434m72rIo?list=PLEKypmos74W8PMP4k6xmVxpTKdebvJpFb");
                    }
                    p.Display();
                    break;
                }
                case 2:
                    Game.LogTrivial("ExitButton pressed.");
                    break;
                case 3:
                {
                    Game.LogTrivial("Delay by week pressed");
                    var nextUpdateCheckDateTime = DateTime.Now.AddDays(6);
                    var commonVariablesDoc = XDocument.Load("Albo1125.Common/CommonVariables.xml");

                    if (commonVariablesDoc.Root != null && commonVariablesDoc.Root.Element("NextUpdateCheckDT") == null)
                    {
                        commonVariablesDoc.Root.Add(new XElement("NextUpdateCheckDT"));
                    }
                    
                    commonVariablesDoc.Root.Element("NextUpdateCheckDT").Value = nextUpdateCheckDateTime.ToBinary().ToString();
                    commonVariablesDoc.Save("Albo1125.Common/CommonVariables.xml");
                    break;
                }
                case 4:
                {
                    Game.LogTrivial("Delay by month pressed");
                    var nextUpdateCheckDateTime = DateTime.Now.AddMonths(1);
                    var commonVariablesDoc = XDocument.Load("Albo1125.Common/CommonVariables.xml");

                    if (commonVariablesDoc.Root != null && commonVariablesDoc.Root.Element("NextUpdateCheckDT") == null)
                    {
                        commonVariablesDoc.Root.Add(new XElement("NextUpdateCheckDT"));
                    }
                    
                    commonVariablesDoc.Root.Element("NextUpdateCheckDT").Value = nextUpdateCheckDateTime.ToBinary().ToString();
                    commonVariablesDoc.Save("Albo1125.Common/CommonVariables.xml");
                    break;
                }
                case 5:
                {
                    Game.LogTrivial("Disable Update Checks pressed.");
                    var commonVariablesDoc = XDocument.Load("Albo1125.Common/CommonVariables.xml");
                    
                    if (commonVariablesDoc.Root != null && commonVariablesDoc.Root.Element("NextUpdateCheckDT") == null)
                    {
                        commonVariablesDoc.Root.Add(new XElement("NextUpdateCheckDT"));
                    }
                    commonVariablesDoc.Root.Element("NextUpdateCheckDT").Value = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    
                    commonVariablesDoc.Save("Albo1125.Common/CommonVariables.xml");
                    var popup = new Popup("Albo1125.Common Update Check", "Update checking has been disabled for this version of Albo1125.Common." +
                                                                        "To re-enable it, delete the Albo1125.Common folder from your Grand Theft Auto V folder. Please do not request support for old versions.", false, true);
                    popup.Display();
                    break;
                }
            }
        }
     
        private static readonly TupleList<string, string> PluginsDownloadLink = new TupleList<string, string>();


        /// <summary>
        /// Checks for modification updates.
        /// </summary>
        /// <param name="modificationName">The name of the modification.</param>
        /// <param name="curVersion">The current version of the modification.</param>
        /// <param name="versionCheckLink">The link to check for updates.</param>
        /// <param name="downloadLink">The link to download the updates.</param>
        private static void CheckForModificationUpdates(string modificationName, Version curVersion, string versionCheckLink, string downloadLink)
        {
            if (_lspdfrUpdateApiRunning)
            {
                new UpdateChecker(modificationName, curVersion, versionCheckLink, downloadLink);
                if (_albo1125CommonCheckedForUpdates) return;
                
                _albo1125CommonCheckedForUpdates = true;
                new UpdateChecker("Albo1125.Common", Assembly.GetExecutingAssembly().GetName().Version, "10294", "https://www.lcpdfr.com/files/file/10294-albo1125common/");
            }
            else
            {
                Game.LogTrivial("LSPDFR Update API down. Not starting checks.");
            }
        }
        private Version _newVersion = new Version();
        private static bool _lspdfrUpdateApiRunning = true;

        /// <summary>
        /// Provides functionality to check for updates and display update information.
        /// </summary>
        private UpdateChecker(string modificationName, Version curVersion, string fileId, string downloadLink)
        {

            try
            {
                Game.LogTrivial("Albo1125.Common " + Assembly.GetExecutingAssembly().GetName().Version + ", developed by Albo1125. Checking for " + modificationName + " updates.");

                var fetchVersionThread = new Thread(() =>
                {

                    using (var client = new WebClient())
                    {
                        try
                        {
                            var downloadString = client.DownloadString("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=" + fileId + "&textOnly=1");

                            _newVersion = new Version(downloadString);
                        }
                        catch (Exception) { _lspdfrUpdateApiRunning = false; Game.LogTrivial("LSPDFR Update API down. Aborting checks."); }
                    }
                });
                fetchVersionThread.Start();
                while (fetchVersionThread.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    GameFiber.Yield();
                }

                // compare the versions  
                if (curVersion.CompareTo(_newVersion) >= 0) return;
                
                // ask the user if he would like  
                // to download the new version  
                PluginsDownloadLink.Add(modificationName, downloadLink);
                Game.LogTrivial("Update available for " + modificationName);

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception)
            {
                Game.LogTrivial("Error while checking " + modificationName + " for updates.");
            }
        }


        private static bool _albo1125CommonCheckedForUpdates = false;

        /// <summary>
        /// Initialize the update checking process.
        /// </summary>
        public static void InitialiseUpdateCheckingProcess()
        {
            Game.LogTrivial("Albo1125.Common " + Assembly.GetExecutingAssembly().GetName().Version + ", developed by Albo1125. Starting update checks.");
            Directory.CreateDirectory("Albo1125.Common/UpdateInfo");
            if (!File.Exists("Albo1125.Common/CommonVariables.xml"))
            {
                new XDocument(
                        new XElement("CommonVariables")
                    )
                    .Save("Albo1125.Common/CommonVariables.xml");
            }
            try
            {
                var commonVariablesDoc = XDocument.Load("Albo1125.Common/CommonVariables.xml");
                if (commonVariablesDoc.Root != null && commonVariablesDoc.Root.Element("NextUpdateCheckDT") == null)
                {
                    commonVariablesDoc.Root.Add(new XElement("NextUpdateCheckDT"));
                }
                
                if (!string.IsNullOrWhiteSpace((string)commonVariablesDoc.Root.Element("NextUpdateCheckDT")))
                {

                    try
                    {
                        if (commonVariablesDoc.Root.Element("NextUpdateCheckDT")?.Value == Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            Game.LogTrivial("Albo1125.Common update checking has been disabled. Skipping checks.");
                            Game.LogTrivial("Albo1125.Common note: please do not request support for old versions.");
                            return;
                        }
                        var updateCheckDateTime = DateTime.FromBinary(long.Parse(commonVariablesDoc.Root.Element("NextUpdateCheckDT").Value));
                        if (DateTime.Now < updateCheckDateTime)
                        {

                            Game.LogTrivial("Albo1125.Common " + Assembly.GetExecutingAssembly().GetName().Version + ", developed by Albo1125. Not checking for updates until " + updateCheckDateTime);
                            return;
                        }
                    }
                    catch (Exception e) { Game.LogTrivial(e.ToString()); Game.LogTrivial("Albo1125.Common handled exception. #1"); }

                }

                
                var nextUpdateCheckDt = DateTime.Now.AddDays(1);
                if (commonVariablesDoc.Root.Element("NextUpdateCheckDT") == null)
                {
                    commonVariablesDoc.Root.Add(new XElement("NextUpdateCheckDT")); 
                    
                }
                
                commonVariablesDoc.Root.Element("NextUpdateCheckDT").Value = nextUpdateCheckDt.ToBinary().ToString();
                commonVariablesDoc.Save("Albo1125.Common/CommonVariables.xml");
                GameFiber.StartNew(delegate
                {


                    GetUpdateNodes();
                    foreach (var entry in AllUpdateEntries.ToArray())
                    {
                        CheckForModificationUpdates(entry.Name, new Version(FileVersionInfo.GetVersionInfo(entry.Path).FileVersion), entry.FileId, entry.DownloadLink);
                    }
                    if (PluginsDownloadLink.Count > 0) { DisplayUpdates(); }
                    Game.LogTrivial("Albo1125.Common " + Assembly.GetExecutingAssembly().GetName().Version + ", developed by Albo1125. Update checks complete.");
                });
            }
            catch (XmlException e)
            {
                Game.LogTrivial(e.ToString());
                Game.DisplayNotification("Error while processing XML files. To fix this, please delete the following folder and its contents: Grand Theft Auto V/Albo1125.Common");
                ExtensionMethods.DisplayPopupTextBoxWithConfirmation("Albo1125.Common", "Error while processing XML files. To fix this, please delete the following folder and its contents: Grand Theft Auto V/Albo1125.Common", false);
                throw;
            }

            



        }

        /// <summary>
        /// Verifies that the specified XML node exists and creates a new entry for it.
        /// </summary>
        /// <param name="name">The name of the XML node.</param>
        /// <param name="fileId">The file ID of the XML node.</param>
        /// <param name="downloadLink">The download link of the XML node.</param>
        /// <param name="path">The path of the XML node.</param>
        public static void VerifyXmlNodeExists(string name, string fileId, string downloadLink, string path)
        {        
            Game.LogTrivial("Albo1125.Common verifying update entry for " + name);
            var doc =

                    new XDocument(
                            new XElement("UpdateEntry")
                        );
                        
            try
            {
                Directory.CreateDirectory("Albo1125.Common/UpdateInfo");

                doc.Root.Add(new XElement("Name"));
                doc.Root.Element("Name").Value = XmlConvert.EncodeName(name);

                doc.Root.Add(new XElement("FileID"));
                doc.Root.Element("FileID").Value = fileId;

                doc.Root.Add(new XElement("DownloadLink"));
                doc.Root.Element("DownloadLink").Value = XmlConvert.EncodeName(downloadLink);

                doc.Root.Add(new XElement("Path"));

                doc.Root.Element("Path").Value = XmlConvert.EncodeName(path);

                doc.Save("Albo1125.Common/UpdateInfo/" + name + ".xml");
                
            }
            catch (XmlException e)
            {
                Game.LogTrivial(e.ToString());
                Game.DisplayNotification("Error while processing XML files. To fix this, please delete the following folder and its contents: Grand Theft Auto V/Albo1125.Common");
                ExtensionMethods.DisplayPopupTextBoxWithConfirmation("Albo1125.Common", "Error while processing XML files. To fix this, please delete the following folder and its contents: Grand Theft Auto V/Albo1125.Common", false);
                throw;
            }

        }

        /// <summary>
        /// GetUpdateNodes method retrieves update nodes from XML files and adds valid entries to the AllUpdateEntries list.
        /// </summary>
        private static void GetUpdateNodes()
        {
            var xmlUpdateFiles = Directory.EnumerateFiles("Albo1125.Common/UpdateInfo", "*.xml");
            foreach (var xmlNode in xmlUpdateFiles)
            {
                var doc = XDocument.Load(xmlNode);
                if (IsUpdateNodeValid(doc.Root))
                {
                    if (File.Exists(XmlConvert.DecodeName(doc.Root.Element("Path").Value.Trim())))
                    {
                        AllUpdateEntries.Add(new UpdateEntry()
                        {
                            Name = XmlConvert.DecodeName(doc.Root.Element("Name").Value.Trim()),
                            FileId = doc.Root.Element("FileID").Value.Trim(),
                            DownloadLink = XmlConvert.DecodeName(doc.Root.Element("DownloadLink").Value.Trim()),
                            Path = XmlConvert.DecodeName(doc.Root.Element("Path").Value.Trim()),

                        });
                    }
                }
            }
            

        }

        /// <summary>
        /// Checks if an update node in an XML document is valid.
        /// </summary>
        /// <param name="root">The root container of the XML document.</param>
        /// <returns>True if the update node is valid, otherwise false.</returns>
        private static bool IsUpdateNodeValid(XContainer root)
        {
            return new[] { "Name", "FileID", "DownloadLink", "Path" }.All(s => root.Elements().Select(x => x.Name.ToString()).Contains(s)) && root.Elements().All(s => !string.IsNullOrWhiteSpace(s.Value));
        }
    }

    /// <summary>
    /// Represents an update entry for a modification.
    /// </summary>
    internal class UpdateEntry
    {
        public static readonly List<UpdateEntry> AllUpdateEntries = new List<UpdateEntry>();

        public string Name;
        public string FileId;
        public string DownloadLink;
        public string Path;
    }
}
