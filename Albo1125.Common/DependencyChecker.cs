using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Rage;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Albo1125.Common.CommonLibrary;
using Rage.Native;

namespace Albo1125.Common
{
    /// <summary>
    /// The DependencyChecker class provides functionality for registering plugins for dependency checks and checking dependencies for a plugin.
    /// </summary>
    public static class DependencyChecker
    {
        private static readonly List<string> RegisteredPluginsForDependencyChecks = new List<string>();

        /// <summary>
        /// Registers a plugin for dependency checks.
        /// </summary>
        /// <param name="callingPlugin">The name of the plugin that is calling this method.</param>
        public static void RegisterPluginForDependencyChecks(string callingPlugin)
        {
            if (!RegisteredPluginsForDependencyChecks.Contains(callingPlugin))
            {
                RegisteredPluginsForDependencyChecks.Add(callingPlugin);
            }
        }

        private static readonly TupleList<string, string, string> PluginsUrlErrors = new TupleList<string, string, string>();
        private static string _scid;

        /// <summary>
        /// Checks the dependencies for a plugin and returns whether the check is successful or not.
        /// </summary>
        /// <param name="callingPlugin">The name of the plugin that is calling this method.</param>
        /// <param name="albo1125CommonVer">The version of Albo1125.Common.dll.</param>
        /// <param name="minimumRphVersion">The minimum required version of RPH (RagePluginHook).</param>
        /// <param name="madeForGtaVersion">The version of GTA the plugin is made for (optional).</param>
        /// <param name="madeForLspdfrVersion">The version of LSPDFR the plugin is made for (optional).</param>
        /// <param name="rageNativeUiVersion">The version of RageNativeUI the plugin is made for (optional).</param>
        /// <param name="audioFilesToCheckFor">An array of audio files required by the plugin (optional).</param>
        /// <param name="otherRequiredFilesToCheckFor">An array of other required files by the plugin (optional).</param>
        /// <returns>True if all dependencies are met, otherwise False.</returns>
        public static bool DependencyCheckMain(string callingPlugin, Version albo1125CommonVer, float minimumRphVersion, Version madeForGtaVersion = null, Version madeForLspdfrVersion = null, Version rageNativeUiVersion = null, string[] audioFilesToCheckFor = null, string[] otherRequiredFilesToCheckFor = null)
        {
            return DependencyCheckMain(callingPlugin, albo1125CommonVer, minimumRphVersion, "https://youtu.be/af434m72rIo?list=PLEKypmos74W8PMP4k6xmVxpTKdebvJpFb", madeForGtaVersion, madeForLspdfrVersion, rageNativeUiVersion, audioFilesToCheckFor, otherRequiredFilesToCheckFor);
        }

        /// <summary>
        /// Performs a dependency check for a given plugin.
        /// </summary>
        /// <param name="callingPlugin">The name of the calling plugin.</param>
        /// <param name="albo1125CommonVer">The required version of Albo1125.Common.</param>
        /// <param name="minimumRphVersion">The minimum required version of RAGE Plugin Hook.</param>
        /// <param name="installationVideoUrl">The URL of the installation video for the plugin.</param>
        /// <param name="madeForGtaVersion">The specific version of GTA the plugin is made for (optional).</param>
        /// <param name="madeForLspdfrVersion">The specific version of LSPDFR the plugin is made for (optional).</param>
        /// <param name="rageNativeUiVersion">The specific version of RAGENativeUI the plugin is made for (optional).</param>
        /// <param name="audioFilesToCheckFor">An array of audio files to check if they exist (optional).</param>
        /// <param name="otherRequiredFilesToCheckFor">An array of other required files to check if they exist (optional).</param>
        /// <returns>
        /// True if all dependency checks pass, false otherwise.
        /// </returns>
        public static bool DependencyCheckMain(string callingPlugin, Version albo1125CommonVer, float minimumRphVersion,
            string installationVideoUrl, Version madeForGtaVersion = null, Version madeForLspdfrVersion = null,
            Version rageNativeUiVersion = null, string[] audioFilesToCheckFor = null,
            string[] otherRequiredFilesToCheckFor = null)
        {
            var checkPassedSuccessfully = true;
            Game.LogTrivial("Albo1125.Common.dll " + Assembly.GetExecutingAssembly().GetName().Version +
                            " starting standard dependency check for " + callingPlugin);
            if (_scid == null)
            {
                _scid = NativeFunction.Natives.GET_PLAYER_NAME<string>(Game.LocalPlayer);
                Game.LogTrivial("SCID:/" + _scid + "/");
            }

            if (File.Exists("Albo1125.Common.dll"))
            {
                var installedCommonVer =
                    new Version(FileVersionInfo.GetVersionInfo("Albo1125.Common.dll").ProductVersion);
                if (installedCommonVer.CompareTo(albo1125CommonVer) >= 0)
                {
                    if (madeForGtaVersion != null)
                    {
                        Game.LogTrivial("GAME VERSION: " + Game.ProductVersion);
                        var compare = Game.ProductVersion.CompareTo(madeForGtaVersion);
                        if (compare > 0)
                        {
                            Game.LogTrivial(callingPlugin + " compatibility warning: The current game version is newer than " + madeForGtaVersion + " and may or may not be incompatible due to RPH changes. Use at own risk.");
                        }
                    }

                    if (madeForLspdfrVersion != null)
                    {
                        if (File.Exists("Plugins/LSPD First Response.dll"))
                        {
                            var installedLspdfrVer = new Version(FileVersionInfo.GetVersionInfo("Plugins/LSPD First Response.dll").ProductVersion);
                            if (installedLspdfrVer.CompareTo(madeForLspdfrVersion) != 0)
                            {
                                Game.LogTrivial(callingPlugin + " compatibility warning: Different LSPD First Response.dll version detected, use at your own risk! This mod was made for LSPDFR " + madeForLspdfrVersion);
                                Game.DisplayNotification(callingPlugin + " compatibility warning: Different LSPD First Response.dll version detected, use at your own risk! This mod was made for LSPDFR " + madeForLspdfrVersion);
                            }
                        }
                        else
                        {
                            Game.LogTrivial("LSPD First Response.dll not installed.");
                            PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "Couldn't detect required LSPD First Response.dll. You must install it.");
                            checkPassedSuccessfully = false;
                        }

                    }
                    if (rageNativeUiVersion != null)
                    {
                        if (File.Exists("RAGENativeUI.dll"))
                        {
                            var installedNativeUiVer = new Version(FileVersionInfo.GetVersionInfo("RAGENativeUI.dll").ProductVersion);
                            if (installedNativeUiVer.CompareTo(rageNativeUiVersion) < 0)
                            {
                                Game.LogTrivial("RAGENativeUI.dll out of date. Required version of RAGENativeUI to run this mod: " + rageNativeUiVersion);
                                PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "RAGENativeUI.dll out of date. Required version of RAGENativeUI to run this mod: " + rageNativeUiVersion);
                                checkPassedSuccessfully = false;
                            }
                        }
                        else
                        {
                            Game.LogTrivial("RAGENativeUI.dll is not installed. You must install it to run this mod.");
                            PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "RAGENativeUI.dll is not installed. You must install it to run this mod.");
                            checkPassedSuccessfully = false;
                        }
                    }
                    if (audioFilesToCheckFor != null)
                    {
                        foreach (var fileString in audioFilesToCheckFor)
                        {
                            if (File.Exists(fileString)) continue;
                            
                            Game.LogTrivial("Couldn't find the required audio file at " + fileString);
                            PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "You are missing required (new) audio files. Path is: " + fileString);

                            checkPassedSuccessfully = false;
                        }
                    }
                    if (otherRequiredFilesToCheckFor != null)
                    {
                        foreach (var fileString in otherRequiredFilesToCheckFor)
                        {
                            if (File.Exists(fileString)) continue;
                            
                            Game.LogTrivial("Couldn't find the required file at " + fileString);
                            PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "You are missing required (new) files. Path is: " + fileString);
                            checkPassedSuccessfully = false;
                        }

                    }
                    if (!CheckForRageVersion(minimumRphVersion))
                    {
                        checkPassedSuccessfully = false;
                        PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "RAGEPluginHook is out of date. This mod requires RPH " + minimumRphVersion);
                    }

                }
                else
                {
                    Game.LogTrivial("Albo1125.Common.dll is out of date. This mod requires Albo1125.Common " + albo1125CommonVer);
                    PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "Albo1125.Common.dll is out of date. This mod requires Albo1125.Common " + albo1125CommonVer);
                    checkPassedSuccessfully = false;
                }
            }
            else
            {
                checkPassedSuccessfully = false;
                Game.LogTrivial("Albo1125.Common.dll is not installed. This mod requires Albo1125.Common to be installed. You've successfully run this without actually having it on your PC...spooky.");
                PluginsUrlErrors.Add(callingPlugin, installationVideoUrl, "Albo1125.Common.dll is not installed. This mod requires Albo1125.Common to be installed. You've successfully run this without actually having it on your PC...spooky.");
            }
            if (RegisteredPluginsForDependencyChecks.Contains(callingPlugin)) { RegisteredPluginsForDependencyChecks.Remove(callingPlugin); }
            if (RegisteredPluginsForDependencyChecks.Count == 0 && PluginsUrlErrors.Count > 0) { DisplayDependencyErrors(); }

            Game.LogTrivial("Dependency check for " + callingPlugin + " successful: " + checkPassedSuccessfully);
            return checkPassedSuccessfully;

        }

        /// <summary>
        /// Checks if there are no conflicting files for a given calling module.
        /// </summary>
        /// <param name="callingMod">The name of the calling module.</param>
        /// <param name="filesToCheckFor">The collection of files to check for conflicts.</param>
        /// <returns>True if there are no conflicting files, otherwise false.</returns>
        public static bool CheckIfThereAreNoConflictingFiles(string callingMod, IEnumerable<string> filesToCheckFor)
        {
            var noConflicts = true;
            foreach (var file in filesToCheckFor)
            {
                if (!File.Exists(file)) continue;
                
                ExtensionMethods.DisplayPopupTextBoxWithConfirmation(callingMod + " detected a conflicting file",
                    "The following file can cause conflicts with " + callingMod + ": " + file + ". You are advised to remove it when running " + callingMod + ".", true);
                noConflicts = false;
            }
            return noConflicts;
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="file">The path of the file to check.</param>
        /// <param name="minVer">Optional. The minimum required version of the file.</param>
        /// <returns>True if the file exists and its version is greater than or equal to <paramref name="minVer"/>, otherwise false.</returns>
        public static bool CheckIfFileExists(string file, Version minVer = null)
        {
            if (!File.Exists(file)) return false;
            
            
            var installedVer = new Version(FileVersionInfo.GetVersionInfo(file).ProductVersion);
            
            return minVer == null || installedVer.CompareTo(minVer) >= 0;
        }

        private static int _index = -1;

        /// <summary>
        /// Displays the dependency errors for registered plugins.
        /// </summary>
        public static void DisplayDependencyErrors()
        {
            var modsWithErrors = PluginsUrlErrors.Select(x => x.Item1).Distinct().ToList().Aggregate("", (current, errorString) => current + (errorString + ","));

            var popup = new Popup("Albo1125.Common detected errors", "Errors were detected in your installation of the following of my modifications, so they will not load: " + modsWithErrors, new List<string> { "Continue" }, false, false, NextDependencyErrorCallback);
            popup.Display();
        }

        /// <summary>
        /// Callback method for handling dependency error popups.
        /// </summary>
        /// <param name="errorPopup">The Popup object representing the error message popup.</param>
        private static void NextDependencyErrorCallback(Popup errorPopup)
        {
            switch (errorPopup.IndexOfGivenAnswer)
            {
                case 0:
                {
                    Game.LogTrivial("Continue pressed");
                    _index++;
                    if (PluginsUrlErrors.Count > _index)
                    {
                        var popup = new Popup("Error " + (_index + 1) + ": " + PluginsUrlErrors[_index].Item1, PluginsUrlErrors[_index].Item3, new List<string>() { "Continue", "Open installation video tutorial for this plugin" },
                            false, false, NextDependencyErrorCallback);
                        popup.Display();
                    }
                    else
                    {
                        var popup = new Popup("Albo1125.Common detected errors", "To fix these installation errors, you should read the appropriate ReadMe or documentation files, watch the installation video tutorial or use my Troubleshooter (link in video description).",
                            new List<string>() { "Continue", "Open installation/troubleshooting video tutorial", "Exit" }, false, false, NextDependencyErrorCallback);
                        popup.Display();
                    }

                    break;
                }
                case 1:
                {
                    Game.LogTrivial("GoToVideo pressed.");
                    Process.Start(PluginsUrlErrors.Count > _index
                        ? PluginsUrlErrors[_index].Item2
                        : "https://youtu.be/af434m72rIo?list=PLEKypmos74W8PMP4k6xmVxpTKdebvJpFb");
                    errorPopup.Display();
                    break;
                }
                case 2:
                    Game.LogTrivial("ExitButton pressed.");
                    break;
            }
        }

        private static bool _correctRphVersion;

        /// <summary>
        /// Checks if the RAGEPluginHook version meets the minimum requirement.
        /// </summary>
        /// <param name="minimumVersion">The minimum required RAGEPluginHook version.</param>
        /// <returns>Returns true if the RAGEPluginHook version is equal or greater than the minimum required version, otherwise false.</returns>
        private static bool CheckForRageVersion(float minimumVersion)
        {
            var rphFile = "RAGEPluginHook.exe";
            var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "*.exe", SearchOption.TopDirectoryOnly);
            
            foreach (var fileString in files)
            {
                if (fileString == string.Empty)
                    continue;
                
                if (Path.GetFileName(fileString).ToLower() != "ragepluginhook.exe") continue;
                
                rphFile = fileString;
                break;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(rphFile);
            try
            {
                var rageVersion = float.Parse(versionInfo.ProductVersion.Substring(0, 4), CultureInfo.InvariantCulture);
                Game.LogTrivial("Albo1125.Common detected RAGEPluginHook version: " + rageVersion);

                //Set to false If user's RPH version is older than the minimum, true if user's RPH version is (above) the specified minimum
                _correctRphVersion = !(rageVersion < minimumVersion);
                
            }
            catch (Exception e)
            {
                //If for whatever reason the version couldn't be found.
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Unable to detect your Rage installation.");
                Game.LogTrivial(File.Exists("RAGEPluginHook.exe") ? "RAGEPluginHook.exe exists" : "RAGEPluginHook doesn't exist.");
                Game.LogTrivial("Rage Version: " + versionInfo.ProductVersion);
                Game.DisplayNotification("Albo1125.Common was unable to detect RPH installation. Please send me your logfile.");
                _correctRphVersion = false;

            }

            return _correctRphVersion;
        }
    }
}
