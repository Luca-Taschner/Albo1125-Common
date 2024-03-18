using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rage;
using Rage.Native;
using System.Drawing;
using System.Windows.Forms;
using Albo1125.Common.CommonLibrary;
using System.Globalization;
using System.IO;

namespace Albo1125.Common.CommonLibrary
{
    /// <summary>
    /// Represents a list of tuples with two elements.
    /// </summary>
    /// <typeparam name="T1">The type of the first element in the tuple.</typeparam>
    /// <typeparam name="T2">The type of the second element in the tuple.</typeparam>
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        /// <summary>
        /// Represents a list of tuples with two elements.
        /// </summary>
        /// <typeparam name="T1">The type of the first element in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second element in the tuple.</typeparam>
        public TupleList(TupleList<T1, T2> tupleList)
        {
            foreach (var tuple in tupleList)
            {
                Add(tuple);
            }
        }
        public TupleList() { }
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    /// <summary>
    /// Represents a list of tuples.
    /// </summary>
    /// <typeparam name="T1">The type of the first element in the tuple.</typeparam>
    /// <typeparam name="T2">The type of the second element in the tuple.</typeparam>
    /// <typeparam name="T3">The type of the third element in the tuple.</typeparam>
    public class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
    {
        public TupleList() { }
        public TupleList(TupleList<T1, T2, T3> tupleList)
        {
            foreach (var tuple in tupleList)
            {
                Add(tuple);
            }
        }

        /// <summary>
        /// Adds an item to the TupleList.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the Tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the Tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the Tuple.</typeparam>
        /// <param name="item">The first item to add to the TupleList.</param>
        /// <param name="item2">The second item to add to the TupleList.</param>
        /// <param name="item3">The third item to add to the TupleList.</param>
        /// <remarks>
        /// This method adds a new Tuple with the specified items to the TupleList.
        /// </remarks>
        public void Add(T1 item, T2 item2, T3 item3)
        {
            Add(new Tuple<T1, T2, T3>(item, item2, item3));
        }

    }

    /// <summary>
    /// Represents a list of tuples with two elements.
    /// </summary>
    /// <typeparam name="T1">The type of the first element in each tuple.</typeparam>
    /// <typeparam name="T2">The type of the second element in each tuple.</typeparam>
    /// <typeparam name="T3">The type of the third element in each tuple.</typeparam>
    /// <typeparam name="T4">The type of the fourth element in each tuple.</typeparam>
    public class TupleList<T1, T2, T3, T4> : List<Tuple<T1, T2, T3, T4>>
    {
        public TupleList() { }
        
        public TupleList(TupleList<T1, T2, T3, T4> tupleList)
        {
            foreach (var tuple in tupleList)
            {
                Add(tuple);
            }
        }

        /// <summary>
        /// Adds an item to the TupleList.
        /// </summary>
        /// <param name="item">The first item of the Tuple.</param>
        /// <param name="item2">The second item of the Tuple.</param>
        /// <param name="item3">The third item of the Tuple.</param>
        /// <param name="item4">The fourth item of the Tuple.</param>
        public void Add(T1 item, T2 item2, T3 item3, T4 item4)
        {
            Add(new Tuple<T1, T2, T3, T4>(item, item2, item3, item4));
        }

    }

    /// <summary>
    /// This class contains extension methods that can be used on various types.
    /// </summary>
    public static class ExtensionMethods
    {
        public static readonly int[] BlackListedNodeTypes = { 0, 8, 9, 10, 12, 40, 42, 136 };

        /// <summary>
        /// Returns the nearest node type to the specified position.
        /// </summary>
        /// <param name="pos">The position to check for nearest node type.</param>
        /// <returns>The nearest node type to the specified position. Returns -1 if no node type found.</returns>
        public static int GetNearestNodeType(this Vector3 pos)
        {
            bool getPropertySuccess = NativeFunction.Natives.GET_VEHICLE_NODE_PROPERTIES<bool>(pos.X, pos.Y, pos.Z, out uint _, out int foundNodeType);

            if (getPropertySuccess)
            {
                return foundNodeType;
            }
            
            return -1;
        }

        /// <summary>
        /// Determines if a given node is safe.
        /// </summary>
        /// <param name="pos">The position of the node to check.</param>
        /// <returns>True if the node is safe; otherwise, false.</returns>
        public static bool IsNodeSafe(this Vector3 pos)
        {
            return !BlackListedNodeTypes.Contains(GetNearestNodeType(pos));
        }

        /// <summary>
        /// Determines whether a given point is on water.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the point is on water, false otherwise.</returns>
        public static bool IsPointOnWater(this Vector3 position)
        {
            return NativeFunction.Natives.GET_WATER_HEIGHT<bool>(position.X, position.Y, position.Z, out float _);
        }

        /// <summary>
        /// Displays a popup text box with confirmation in the game.
        /// </summary>
        /// <param name="title">The title of the popup.</param>
        /// <param name="text">The text body of the popup.</param>
        /// <param name="pauseGame">Specifies whether the game should be paused while the popup is displayed.</param>
        public static void DisplayPopupTextBoxWithConfirmation(string title, string text, bool pauseGame)
        {
            new Popup(title, text, pauseGame, true).Display();
        }

        /// <summary>
        /// Wraps a text into multiple lines based on the specified width in pixels.
        /// </summary>
        /// <param name="text">The text to wrap.</param>
        /// <param name="pixels">The width in pixels.</param>
        /// <param name="fontFamily">The font family to use.</param>
        /// <param name="emSize">The font size.</param>
        /// <param name="actualHeight">Out parameter to store the actual height of the formatted text.</param>
        /// <returns>A list of wrapped lines.</returns>
        public static List<string> WrapText(this string text, double pixels, string fontFamily, float emSize, out double actualHeight)
        {
            var originalLines = text.Split(new[] { " " },
                StringSplitOptions.None);

            var wrappedLines = new List<string>();

            var actualLine = new StringBuilder();
            double actualWidth = 0;
            actualHeight = 0;
            foreach (var item in originalLines)
            {
                var formatted = new System.Windows.Media.FormattedText(item,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface(fontFamily), emSize, System.Windows.Media.Brushes.Black,2);
                


                actualWidth += formatted.Width;
                actualHeight = formatted.Height;
                
                if (actualWidth > pixels)
                {
                    wrappedLines.Add(actualLine.ToString());
                    actualLine.Clear();
                    actualWidth = 0;
                    actualLine.Append(item + " ");
                    actualWidth += formatted.Width;
                }
                else if (item == Environment.NewLine || item=="\n")
                {
                    wrappedLines.Add(actualLine.ToString());
                    actualLine.Clear();
                    actualWidth = 0;
                }
                else
                {
                    actualLine.Append(item + " ");
                }
            }
            if (actualLine.Length > 0)
                wrappedLines.Add(actualLine.ToString());

            return wrappedLines;
        }

        /// <summary>
        /// Checks if the given Ped is a police Ped.
        /// </summary>
        /// <param name="ped">The Ped to check.</param>
        /// <returns>Returns true if the Ped is a police Ped; otherwise, false.</returns>
        public static bool IsPolicePed(this Ped ped)
        {
            return ped.RelationshipGroup == "COP";
        }

        /// <summary>
        /// Gets the string representation of a key combination.
        /// </summary>
        /// <param name="mainKey">The main key of the combination.</param>
        /// <param name="modifierKey">The modifier key of the combination.</param>
        /// <returns>
        /// The string representation of the key combination.
        /// </returns>
        public static string GetKeyString(Keys mainKey, Keys modifierKey)
        {
            if (modifierKey == Keys.None)
            {
                return mainKey.ToString();
            }

            var strModKey = modifierKey.ToString();

            if (strModKey.EndsWith("ControlKey") | strModKey.EndsWith("ShiftKey"))
            {
                strModKey = strModKey.Replace("Key", "");
            }

            if (strModKey.Contains("ControlKey"))
            {
                strModKey = "CTRL";
            }
            else if (strModKey.Contains("ShiftKey"))
            {
                strModKey = "Shift";
            }
            else if (strModKey.Contains("Menu"))
            {
                strModKey = "ALT";
            }

            return $"{strModKey} + {mainKey.ToString()}";
        }

        /// <summary>
        /// Calculates the heading (in degrees) towards the specified entity from the current entity.
        /// </summary>
        /// <param name="ent">The entity from which the heading needs to be calculated.</param>
        /// <param name="targetEntity">The entity towards which the heading needs to be calculated.</param>
        /// <returns>
        /// The heading value in degrees towards the specified entity from the current entity.
        /// </returns>
        public static float CalculateHeadingTowardsEntity (this Entity ent, Entity targetEntity)
        {
            var directionToTargetEnt = (targetEntity.Position - ent.Position);
            directionToTargetEnt.Normalize();
            return MathHelper.ConvertDirectionToHeading(directionToTargetEnt);
            
        }

        /// <summary>
        /// Calculates the heading angle towards a target position from the start position.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="target">The target position.</param>
        /// <returns>The heading angle towards the target position.</returns>
        public static float CalculateHeadingTowardsPosition(this Vector3 start, Vector3 target)
        {
            var directionToTargetEnt = (target - start);
            directionToTargetEnt.Normalize();
            return MathHelper.ConvertDirectionToHeading(directionToTargetEnt);

        }

        /// <summary>
        /// Checks if a key on the computer is currently being held down.
        /// </summary>
        /// <param name="keyPressed">The key to check if it is being held down.</param>
        /// <returns>True if the specified key is being held down, false otherwise.</returns>
        public static bool IsKeyDownComputerCheck(Keys keyPressed)
        {
            return NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0 && Game.IsKeyDown(keyPressed);
        }

        /// <summary>
        /// Checks if a specific key is currently being pressed on the computer keyboard.
        /// </summary>
        /// <param name="keyPressed">The key to check.</param>
        /// <returns>true if the specified key is currently being pressed on the computer keyboard; otherwise, false.</returns>
        public static bool IsKeyDownRightNowComputerCheck(Keys keyPressed)
        {
            return NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0 && Game.IsKeyDownRightNow(keyPressed);
        }

        /// <summary>
        /// Checks if the specified key combination is currently pressed down on the computer.
        /// </summary>
        /// <param name="mainKey">The main key of the combination.</param>
        /// <param name="modifierKey">The modifier key of the combination.</param>
        /// <returns>
        /// <c>true</c> if both the main key and modifier key are currently pressed down on the computer;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsKeyCombinationDownComputerCheck(Keys mainKey, Keys modifierKey)
        {
            if (mainKey != Keys.None)
            {
                return IsKeyDownComputerCheck(mainKey) && (IsKeyDownRightNowComputerCheck(modifierKey)
                || (modifierKey == Keys.None && !IsKeyDownRightNowComputerCheck(Keys.Shift) && !IsKeyDownRightNowComputerCheck(Keys.Control)
                && !IsKeyDownRightNowComputerCheck(Keys.LControlKey) && !IsKeyDownRightNowComputerCheck(Keys.LShiftKey)));
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified key combination is currently being pressed on the computer.
        /// </summary>
        /// <param name="mainKey">The main key of the combination.</param>
        /// <param name="modifierKey">The modifier key of the combination.</param>
        /// <returns>True if the key combination is currently being pressed, otherwise false.</returns>
        public static bool IsKeyCombinationDownRightNowComputerCheck(Keys mainKey, Keys modifierKey)
        {
            if (mainKey != Keys.None)
            {
                return IsKeyDownRightNowComputerCheck(mainKey) && ((IsKeyDownRightNowComputerCheck(modifierKey)
                    || (modifierKey == Keys.None && !IsKeyDownRightNowComputerCheck(Keys.Shift) && !IsKeyDownRightNowComputerCheck(Keys.Control)
                    && !IsKeyDownRightNowComputerCheck(Keys.LControlKey) && !IsKeyDownRightNowComputerCheck(Keys.LShiftKey))));
            }

            return false;
        }

        /// <summary>
        /// Reverses the characters in a string.
        /// </summary>
        /// <param name="s">The string to be reversed.</param>
        /// <returns>The reversed string.</returns>
        public static string Reverse(this string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Randomly selects an element from the given source collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection to pick random elements from.</param>
        /// <returns>The randomly selected element from the source collection.</returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        /// <summary>
        /// Randomly selects an element from the given source collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection to pick random elements from.</param>
        /// <param name="count">Amount of elements</param>
        /// <returns>The randomly selected element from the source collection.</returns>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return new List<T>(source).Shuffle().Take(count);
        }

        /// <summary>
        /// Shuffles the elements of the given source list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source list.</typeparam>
        /// <param name="sourceList">The source list to shuffle.</param>
        /// <returns>A new list with the elements of the source list in random order.</returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> sourceList)
        {
            var shuffledList = new List<T>(sourceList);
            var listCount = shuffledList.Count;
            while (listCount > 1)
            {
                listCount--;
                var k = CommonVariables.rnd.Next(listCount + 1);
                (shuffledList[k], shuffledList[listCount]) = (shuffledList[listCount], shuffledList[k]);
            }
            return shuffledList;
            
        }

        /// <summary>
        /// Makes the specified <see cref="Ped"/> a mission ped.
        /// </summary>
        /// <param name="ped">The <see cref="Ped"/> to make a mission ped.</param>
        public static void MakeMissionPed(this Ped ped)
        {
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;

        }

        /// <summary>
        /// Clones a Ped object.
        /// </summary>
        /// <param name="oldPed">The original Ped object to clone.</param>
        /// <returns>A new Ped object with the same properties as the original Ped.</returns>
        public static Ped ClonePed(this Ped oldPed)
        {
            var oldPedPosition = oldPed.Position;
            var oldPedHeading = oldPed.Heading;
            var spawnInVehicle = false;
            Vehicle vehicle = null;
            var oldPedSeatIndex = 0;
            var oldPedArmor = oldPed.Armor;
            var oldPedHealth = oldPed.Health;
            if (oldPed.IsInAnyVehicle(false))
            {
                vehicle = oldPed.CurrentVehicle;
                oldPedSeatIndex = oldPed.SeatIndex;
                spawnInVehicle = true;
            }
            Ped newPed = NativeFunction.Natives.ClonePed<Ped>(oldPed, oldPed.Heading, false, true);
            if (oldPed.Exists() && oldPed.IsValid())
            {
                oldPed.Delete();
            }
            newPed.Position = oldPedPosition;
            newPed.Heading = oldPedHeading;

            if (spawnInVehicle)
            {
                newPed.WarpIntoVehicle(vehicle, oldPedSeatIndex);
            }
            newPed.Health = oldPedHealth;
            newPed.Armor = oldPedArmor;
            newPed.BlockPermanentEvents = true;
            newPed.IsPersistent = true;
            return newPed;
        }


        /// <summary>
        /// Toggles the neon light of a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to toggle the neon light for.</param>
        /// <param name="neonLight">The neon light to toggle.</param>
        /// <param name="toggle">True to enable the neon light, false to disable it.</param>
        public static void ToggleNeonLight(this Vehicle vehicle, ENeonLights neonLight, bool toggle)
        {
            const ulong setVehicleNeonLightEnabledHash = 0x2aa720e4287bf269;

            NativeFunction.CallByHash<uint>(setVehicleNeonLightEnabledHash, vehicle, (int)neonLight, toggle);
        }


        /// <summary>
        /// Sets the color of the neon lights on a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to set the neon lights color on.</param>
        /// <param name="color">The color to set the neon lights to.</param>
        public static void SetNeonLightsColor(this Vehicle vehicle, Color color)
        {
            const ulong setVehicleNeonLightsColoursHash = 0x8e0a582209a62695;

            NativeFunction.CallByHash<uint>(setVehicleNeonLightsColoursHash, vehicle, (int)color.R, (int)color.G, (int)color.B);
        }


        /// <summary>
        /// Determines whether the specified neon light of the vehicle is enabled.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="neonLight">The neon light to check.</param>
        /// <returns>Returns <c>true</c> if the neon light is enabled, otherwise returns <c>false</c>.</returns>
        public static bool IsNeonLightEnable(this Vehicle vehicle, ENeonLights neonLight)
        {
            const ulong isVehicleNeonLightEnabledHash = 0x8c4b92553e4766a5;
            return NativeFunction.CallByHash<bool>(isVehicleNeonLightEnabledHash, vehicle, (int)neonLight);
        }


        /// <summary>
        /// Retrieves the current neon lights color of the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to get the neon lights color of.</param>
        /// <returns>The neon lights color of the specified vehicle.</returns>
        public static Color GetNeonLightsColor(this Vehicle vehicle)
        {
            return UnsafeGetNeonLightsColor(vehicle);
        }

        /// <summary>
        /// Retrieves the current neon lights color of the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to get the neon lights color of.</param>
        /// <returns>The neon lights color of the specified vehicle.</returns>
        private static unsafe Color UnsafeGetNeonLightsColor(Vehicle vehicle)
        {
            int red;
            int green;
            int blue;
            const ulong getVehicleNeonLightsColourHash = 0x7619eee8c886757f;
            NativeFunction.CallByHash<uint>(getVehicleNeonLightsColourHash, vehicle, &red, &green, &blue);

            return Color.FromArgb(red, green, blue);
        }


        /// <summary>
        /// Retrieves the primary and secondary colors of a given vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to get the colors from.</param>
        /// <returns>The VehicleColor object representing the primary and secondary colors of the vehicle.</returns>
        public static VehicleColor GetColors(this Vehicle vehicle)
        {
            return UnsafeGetVehicleColors(vehicle);
        }

        /// <summary>
        /// Get the colors of a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to get the colors from.</param>
        /// <returns>The colors of the vehicle.</returns>
        private static unsafe VehicleColor UnsafeGetVehicleColors(Vehicle vehicle)
        {
            int colorPrimaryInt;
            int colorSecondaryInt;

            const ulong getVehicleColorsHash = 0xa19435f193e081ac;
            NativeFunction.CallByHash<uint>(getVehicleColorsHash, vehicle, &colorPrimaryInt, &colorSecondaryInt);

            var vehicleColors = new VehicleColor
            {
                PrimaryColor = (EPaint)colorPrimaryInt,
                SecondaryColor = (EPaint)colorSecondaryInt
            };

            return vehicleColors;
        }

        /// <summary>
        /// Set the primary and secondary colors of a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to set the colors for.</param>
        /// <param name="primaryColor">The primary color to set for the vehicle. It should be of type <see cref="EPaint"/>.</param>
        /// <param name="secondaryColor">The secondary color to set for the vehicle. It should be of type <see cref="EPaint"/>.</param>
        public static void SetColors(this Vehicle vehicle, EPaint primaryColor, EPaint secondaryColor)
        {
            NativeFunction.Natives.SET_VEHICLE_COLOURS(vehicle, (int)primaryColor, (int)secondaryColor);
        }

        /// <summary>
        /// Sets the primary and secondary colors of a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to set the colors for.</param>
        /// <param name="color">The primary color to set.</param>
        public static void SetColors(this Vehicle vehicle, VehicleColor color)
        {
            NativeFunction.Natives.SET_VEHICLE_COLOURS(vehicle, (int)color.PrimaryColor, (int)color.SecondaryColor);
        }


        /// <summary>
        /// Randomizes the license plate of a vehicle.
        /// The license plate is generated by combining random numbers and characters.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate should be randomized.</param>
        public static void RandomiseLicencePlate(this Vehicle vehicle)
        {
            if (!vehicle) return;
            
            vehicle.LicensePlate = MathHelper.GetRandomInteger(9) +
                                   MathHelper.GetRandomInteger(9) +
                                   Convert.ToChar(MathHelper.GetRandomInteger(0, 25) + 65) +
                                   Convert.ToChar(MathHelper.GetRandomInteger(0, 25) + 65) +
                                   Convert.ToChar(MathHelper.GetRandomInteger(0, 25) + 65) +
                                   MathHelper.GetRandomInteger(9) +
                                   MathHelper.GetRandomInteger(9) +
                                   MathHelper.GetRandomInteger(9).ToString();
        }


        private static readonly Dictionary<Model, bool> VehicleModelElsCache = new Dictionary<Model, bool>();


        /// <summary>
        /// Determines if a given vehicle model is ELS-enabled.
        /// </summary>
        /// <param name="vehicleModel">The vehicle model to check.</param>
        /// <returns>True if the vehicle model is ELS-enabled, false otherwise.</returns>
        public static bool VehicleModelIsEls(Model vehicleModel)
        {
            try
            {
                if (VehicleModelElsCache.TryGetValue(vehicleModel, out var vehicleModelIsEls))
                {
                    return vehicleModelIsEls;
                }

                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ELS")))
                {
                    // no ELS installation at all
                    VehicleModelElsCache.Add(vehicleModel, false);
                    return false;
                }

                var elsFiles = Directory.EnumerateFiles(
                    Path.Combine(Directory.GetCurrentDirectory(), "ELS"),
                    $"{vehicleModel.Name}.xml", SearchOption.AllDirectories);

                VehicleModelElsCache.Add(vehicleModel, elsFiles.Any());
                return VehicleModelElsCache[vehicleModel];
            }
            catch (Exception e)
            {
                Game.LogTrivial($"Failed to determine if a vehicle model '{vehicleModel}' was ELS-enabled: {e}");
                return false;
            }
        }

        /// <summary>
        /// Determines whether the given vehicle model is an ELS (Emergency Lighting System) model.
        /// </summary>
        /// <param name="vehicleModel">The model of the vehicle.</param>
        /// <returns>
        /// <c>true</c> if the vehicle model is an ELS model; otherwise, <c>false</c>.
        /// </returns>
        public static bool VehicleModelIsEls(this Vehicle vehicleModel)
        {
            return vehicleModel && VehicleModelIsEls(vehicleModel.Model);
        }

    }


    /// <summary>
    /// Represents the different neon lights of a vehicle.
    /// </summary>
    public enum ENeonLights
    {
        Front = 2,
        Back = 3,
        Left = 0,
        Right = 1,
    }

    /// <summary>
    /// The EPaint enum represents the different paint colors available for vehicles.
    /// </summary>
    public enum EPaint
    {
        /* CLASSIC|METALLIC */
        Black = 0,
        Carbon_Black = 147,
        Graphite = 1,
        Anthracite_Black = 11,
        Black_Steel = 2,
        Dark_Steel = 3,
        Silver = 4,
        Bluish_Silver = 5,
        Rolled_Steel = 6,
        Shadow_Silver = 7,
        Stone_Silver = 8,
        Midnight_Silver = 9,
        Cast_Iron_Silver = 10,
        Red = 27,
        Torino_Red = 28,
        Formula_Red = 29,
        Lava_Red = 150,
        Blaze_Red = 30,
        Grace_Red = 31,
        Garnet_Red = 32,
        Sunset_Red = 33,
        Cabernet_Red = 34,
        Wine_Red = 143,
        Candy_Red = 35,
        Hot_Pink = 135,
        Pfister_Pink = 137,
        Salmon_Pink = 136,
        Sunrise_Orange = 36,
        Orange = 38,
        Bright_Orange = 138,
        Gold = 37,
        Bronze = 90,
        Yellow = 88,
        Race_Yellow = 89,
        Dew_Yellow = 91,
        Green = 139,
        Dark_Green = 49,
        Racing_Green = 50,
        Sea_Green = 51,
        Olive_Green = 52,
        Bright_Green = 53,
        Gasoline_Green = 54,
        Lime_Green = 92,
        Hunter_Green = 144,
        Securiror_Green = 125,
        Midnight_Blue = 141,
        Galaxy_Blue = 61,
        Dark_Blue = 62,
        Saxon_Blue = 63,
        Blue = 64,
        Bright_Blue = 140,
        Mariner_Blue = 65,
        Harbor_Blue = 66,
        Diamond_Blue = 67,
        Surf_Blue = 68,
        Nautical_Blue = 69,
        Racing_Blue = 73,
        Ultra_Blue = 70,
        Light_Blue = 74,
        Police_Car_Blue = 127,
        Epsilon_Blue = 157,
        Chocolate_Brown = 96,
        Bison_Brown = 101,
        Creek_Brown = 95,
        Feltzer_Brown = 94,
        Maple_Brown = 97,
        Beechwood_Brown = 103,
        Sienna_Brown = 104,
        Saddle_Brown = 98,
        Moss_Brown = 100,
        Woodbeech_Brown = 102,
        Straw_Brown = 99,
        Sandy_Brown = 105,
        Bleached_Brown = 106,
        Schafter_Purple = 71,
        Spinnaker_Purple = 72,
        Midnight_Purple = 142,
        Metallic_Midnight_Purple = 146,
        Bright_Purple = 145,
        Cream = 107,
        Ice_White = 111,
        Frost_White = 112,
        Pure_White = 134,
        Default_Alloy = 156,
        Champagne = 93,

        /* MATTE */
        Matte_Black = 12,
        Matte_Gray = 13,
        Matte_Light_Gray = 14,
        Matte_Ice_White = 131,
        Matte_Blue = 83,
        Matte_Dark_Blue = 82,
        Matte_Midnight_Blue = 84,
        Matte_Midnight_Purple = 149,
        Matte_Schafter_Purple = 148,
        Matte_Red = 39,
        Matte_Dark_Red = 40,
        Matte_Orange = 41,
        Matte_Yellow = 42,
        Matte_Lime_Green = 55,
        Matte_Green = 128,
        Matte_Forest_Green = 151,
        Matte_Foliage_Green = 155,
        Matte_Brown = 129,
        Matte_Olive_Darb = 152,
        Matte_Dark_Earth = 153,
        Matte_Desert_Tan = 154,

        /* Util */
        Util_Black = 15,
        Util_Black_Poly = 16,
        Util_Dark_Silver = 17,
        Util_Silver = 18,
        Util_Gun_Metal = 19,
        Util_Shadow_Silver = 20,
        Util_Red = 43,
        Util_Bright_Red = 44,
        Util_Garnet_Red = 45,
        Util_Dark_Green = 56,
        Util_Green = 57,
        Util_Dark_Blue = 75,
        Util_Midnight_Blue = 76,
        Util_Blue = 77,
        Util_Sea_Foam_Blue = 78,
        Util_Lightning_Blue = 79,
        Util_Maui_Blue_Poly = 80,
        Util_Bright_Blue = 81,
        Util_Brown = 108,
        Util_Medium_Brown = 109,
        Util_Light_Brown = 110,
        Util_Off_White = 122,

        /* Worn */
        Worn_Black = 21,
        Worn_Graphite = 22,
        Worn_Silver_Grey = 23,
        Worn_Silver = 24,
        Worn_Blue_Silver = 25,
        Worn_Shadow_Silver = 26,
        Worn_Red = 46,
        Worn_Golden_Red = 47,
        Worn_Dark_Red = 48,
        Worn_Dark_Green = 58,
        Worn_Green = 59,
        Worn_Sea_Wash = 60,
        Worn_Dark_Blue = 85,
        Worn_Blue = 86,
        Worn_Light_Blue = 87,
        Worn_Honey_Beige = 113,
        Worn_Brown = 114,
        Worn_Dark_Brown = 115,
        Worn_Straw_Beige = 116,
        Worn_Off_White = 121,
        Worn_Yellow = 123,
        Worn_Light_Orange = 124,
        Worn_Taxi_Yellow = 126,
        Worn_Orange = 130,
        Worn_White = 132,
        Worn_Olive_Army_Green = 133,

        /* METALS */
        Brushed_Steel = 117,
        Brushed_Black_Steel = 118,
        Brushed_Aluminum = 119,
        Pure_Gold = 158,
        Brushed_Gold = 159,
        Secret_Gold = 160,

        /* CHROME */
        Chrome = 120,
    }

}

/// <summary>
/// Representation of a vehicle color.
/// </summary>
public struct VehicleColor
{
    public EPaint PrimaryColor { get; set; }


    public EPaint SecondaryColor { get; set; }


    /// <summary>
    /// Represents the name of the primary color of a vehicle.
    /// </summary>
    public string PrimaryColorName => GetColorName(PrimaryColor);


    /// <summary>
    /// The name of the secondary color of a vehicle.
    /// </summary>
    /// <value>The name of the secondary color.</value>
    public string SecondaryColorName => GetColorName(SecondaryColor);


    /// <summary>
    /// Gets the name of a color based on the given paint value.
    /// </summary>
    /// <param name="paint">The paint value representing a color.</param>
    /// <returns>The name of the color.</returns>
    public static string GetColorName(EPaint paint)
    {
        var name = Enum.GetName(typeof(EPaint), paint);
        return name?.Replace("_", " ");
    }
}

