using Rage;
using Rage.Native;
using System;

using System.Windows.Forms;

namespace Albo1125.Common.CommonLibrary
{
    /// <summary>
    /// Represents a spawn point in the game world.
    /// </summary>
    public class SpawnPoint
    {
        public Vector3 Position = Vector3.Zero;
        public readonly float Heading;

        public SpawnPoint() { }
        public SpawnPoint(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        public static implicit operator Vector3(SpawnPoint s)
        {
            return s.Position;
        }

        public static implicit operator float(SpawnPoint s)
        {
            return s.Heading;
        }

    }

    /// <summary>
    /// Provides extension methods for the SpawnPoint class.
    /// </summary>
    public static class SpawnPointExtensions
    {
        /// <summary>
        /// Gets the closest major vehicle node to the given start point.
        /// </summary>
        /// <param name="startPoint">The start point for which to find the closest major vehicle node.</param>
        /// <returns>The closest major vehicle node to the start point.</returns>
        public static Vector3 GetClosestMajorVehicleNode(this Vector3 startPoint)
        {
            NativeFunction.Natives.GET_CLOSEST_MAJOR_VEHICLE_NODE<bool>(startPoint.X, startPoint.Y, startPoint.Z, out Vector3 closestMajorVehicleNode, 3.0f, 0f);
            return closestMajorVehicleNode;
            
        }

        /// <summary>
        /// Gets a safe Vector3 position for a ped.
        /// </summary>
        /// <param name="startPoint">The starting position from which to find a safe location.</param>
        /// <param name="safePedPoint">The safe Vector3 position for the ped.</param>
        /// <returns>True if a safe ped position is found, false otherwise.</returns>
        public static bool GetSafeVector3ForPed(this Vector3 startPoint, out Vector3 safePedPoint)
        {
            if (!NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(startPoint.X, startPoint.Y, startPoint.Z, true, out Vector3 tempSpawn, 0))
            {
                tempSpawn = World.GetNextPositionOnStreet(startPoint);
                var nearbyEntity = World.GetClosestEntity(tempSpawn, 25f, GetEntitiesFlags.ConsiderHumanPeds);
                if (nearbyEntity.Exists())
                {
                    tempSpawn = nearbyEntity.Position;
                    safePedPoint = tempSpawn;
                    return true;
                }

                safePedPoint = tempSpawn;
                return false;
            }
            safePedPoint = tempSpawn;
            return true;
        }

        /// <summary>
        /// Gets the closest vehicle spawn point based on a search point.
        /// </summary>
        /// <param name="searchPoint">The point to search from.</param>
        /// <param name="sp">The closest vehicle spawn point.</param>
        /// <returns>True if a guaranteed spawn point is found; otherwise, false.</returns>
        public static bool GetClosestVehicleSpawnPoint(this Vector3 searchPoint, out SpawnPoint sp)
        {
            var guaranteedSpawnPointFound = true;
            
            if (!NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(searchPoint.X, searchPoint.Y, searchPoint.Z, out Vector3 tempSpawnPoint, out float tempHeading, 1, 0x40400000, 0) || !tempSpawnPoint.IsNodeSafe())
            {
                tempSpawnPoint = World.GetNextPositionOnStreet(searchPoint);

                var closestEntity = World.GetClosestEntity(tempSpawnPoint, 30f, GetEntitiesFlags.ConsiderGroundVehicles | GetEntitiesFlags.ExcludeEmptyVehicles | GetEntitiesFlags.ExcludePlayerVehicle);
                if (closestEntity.Exists())
                {
                    tempSpawnPoint = closestEntity.Position;
                    tempHeading = closestEntity.Heading;
                    closestEntity.Delete();
                }
                else
                {
                    Vector3 directionFromSpawnToPlayer = (Game.LocalPlayer.Character.Position - tempSpawnPoint);
                    directionFromSpawnToPlayer.Normalize();

                    tempHeading = MathHelper.ConvertDirectionToHeading(directionFromSpawnToPlayer) + 180f;
                    guaranteedSpawnPointFound = false;
                }
            }

            sp = new SpawnPoint(tempSpawnPoint, tempHeading);
            return guaranteedSpawnPointFound;
        }


        /// <summary>
        /// Gets the vehicle spawn point towards the start point with given parameters.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="spawnDistance">The distance from the start point to the spawn point.</param>
        /// <param name="useSpecialId">Determines whether to use special ID.</param>
        /// <param name="sp">The vehicle spawn point.</param>
        /// <returns>True if special ID is used; otherwise, false.</returns>
        public static bool GetVehicleSpawnPointTowardsStartPoint(this Vector3 startPoint, float spawnDistance, bool useSpecialId, out SpawnPoint sp)
        {
            var tempSpawn = World.GetNextPositionOnStreet(startPoint.Around2D(spawnDistance + 5f));
            var spawnPoint = Vector3.Zero;
            float heading = 0;
            var specialIdUsed = true;
            if (!useSpecialId || !NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_FAVOUR_DIRECTION<bool>(tempSpawn.X, tempSpawn.Y, tempSpawn.Z, startPoint.X, startPoint.Y, startPoint.Z, 0, out spawnPoint, out heading, 0, 0x40400000, 0) || !spawnPoint.IsNodeSafe())
            {
                spawnPoint = World.GetNextPositionOnStreet(startPoint.Around2D(spawnDistance + 5f));
                var directionFromVehicleToPed1 = (startPoint - spawnPoint);
                directionFromVehicleToPed1.Normalize();

                heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
                specialIdUsed = false;
            }
            
            sp = new SpawnPoint(spawnPoint, heading);
            return specialIdUsed;
        }

        /// <summary>
        /// Gets the vehicle spawn point towards the given position with checks.
        /// </summary>
        /// <param name="startPoint">The start point from which to find the vehicle spawn point.</param>
        /// <param name="spawnDistance">The maximum spawn distance of the vehicle from the start point.</param>
        /// <returns>The vehicle spawn point towards the position.</returns>
        public static SpawnPoint GetVehicleSpawnPointTowardsPositionWithChecks(this Vector3 startPoint, float spawnDistance)
        {
            SpawnPoint spawnPoint;
            var useSpecialId = true;
            var waitCount = 0;
            while (true)
            {
                GetVehicleSpawnPointTowardsStartPoint(startPoint, spawnDistance, useSpecialId, out spawnPoint);
                float travelDistance = NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(spawnPoint.Position.X, spawnPoint.Position.Y, spawnPoint.Position.Z, startPoint.X, startPoint.Y, startPoint.Z);
                waitCount++;
                if (Vector3.Distance(startPoint, spawnPoint) > spawnDistance - 15f)
                {

                    if (travelDistance < (spawnDistance * 4.5f))
                    {

                        var directionFromVehicleToPed1 = (startPoint - spawnPoint.Position);
                        directionFromVehicleToPed1.Normalize();

                        var headingToPlayer = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);

                        if (Math.Abs(MathHelper.NormalizeHeading(spawnPoint.Heading) - MathHelper.NormalizeHeading(headingToPlayer)) < 150f)
                        {
                            break;
                        }
                    }
                }
                if (waitCount >= 400)
                {
                    useSpecialId = false;
                }
                if (waitCount == 600)
                {
                    Game.DisplayNotification("Press ~b~Y ~s~to force a spawn in the ~g~wilderness.");
                }
                if ((waitCount >= 600) && ExtensionMethods.IsKeyDownComputerCheck(Keys.Y))
                {
                    return new SpawnPoint(Game.LocalPlayer.Character.Position.Around2D(20f), 0);
                }

                GameFiber.Yield();
            }
            return spawnPoint;
        }
    }
}
