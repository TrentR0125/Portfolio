using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace DeleteVehicle.Client
{
    public class Client : BaseScript
    {
        [Command("deletevehicle")]
        public void OnDeleteVehicleCommand() => TriggerEvent("DeleteVehicle:DeleteClosestVehicle");

        [EventHandler("DeleteVehicle:DeleteClosestVehicle")]
        public void OnDeleteClosestVehicle()
        {
            Ped playerPed = Game.PlayerPed;
            Vehicle closestVeh = ClosestVehicle(6f);

            if (closestVeh != null && closestVeh.Exists())
            {
                if (closestVeh.Position.DistanceToSquared2D(playerPed.Position) < 6f || closestVeh.Driver == Game.PlayerPed)
                {
                    closestVeh.Delete();
                    Screen.ShowNotification("~g~Success~w~: Vehicle has been deleted!");
                }
                else
                {
                    Screen.ShowNotification("~r~Error~w~: Either the vehicle could not be found, or you are not the driver of the vehicle.");
                }
            }
            else
            {
                Screen.ShowNotification("~r~Error~w~: Could not find a vehicle to delete.");
            }
        }

        public Vehicle ClosestVehicle(float inRadius)
        {
            Vector3 playerPos = Game.PlayerPed.Position;

            if (Game.PlayerPed.IsSittingInVehicle())
            {
                return Game.PlayerPed.CurrentVehicle;
            }

            RaycastResult raycast = World.RaycastCapsule(playerPos, playerPos, inRadius, (IntersectOptions)10, Game.PlayerPed);

            if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
            {
                return (Vehicle)raycast.HitEntity;
            }

            return null;
        }
    }
}
