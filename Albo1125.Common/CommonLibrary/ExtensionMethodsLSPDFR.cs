using Rage;
using LSPD_First_Response.Mod.API;

namespace Albo1125.Common.CommonLibrary
{
    public static class ExtensionMethodsLspdfr
    {
        /// <summary>
        /// Clones a Ped object.
        /// </summary>
        /// <param name="oldPed">The original Ped object to clone.</param>
        /// <param name="cloneOldPed">Determines whether to clone the old Ped's personality settings as well.</param>
        /// <returns>A new Ped object with the same properties as the original Ped.</returns>
        public static Ped ClonePed(this Ped oldPed, bool cloneOldPed)
        {
            var oldPersona = Functions.GetPersonaForPed(oldPed);
            var newPed = oldPed.ClonePed();
            if (cloneOldPed)
            {
                Functions.SetPersonaForPed(newPed, oldPersona);

            }
            return newPed;
        }


    }
}
