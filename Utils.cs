using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Locker = LabApi.Features.Wrappers.Locker;
using LockerChamber = LabApi.Features.Wrappers.LockerChamber;

namespace RemoteKeycardLabApi;

/**
 * Helper class used for wrapping around classes
 */
public static class Utils
{
    /***
     * Returns all the keycards a player has in a IEnumerable format
     */
    public static IEnumerable<InventorySystem.Items.Keycards.KeycardItem> GetKeycards(this Player player)
    {
        return player?.Inventory?.UserInventory?.Items == null ? Enumerable.Empty<InventorySystem.Items.Keycards.KeycardItem>() : player.Inventory.UserInventory.Items.Values.Where(item => item.Category == ItemCategory.Keycard).Select(item => item as InventorySystem.Items.Keycards.KeycardItem).Where(keycard => keycard != null);
    }
    /***
     * Checks if a player has any keycard in their inventory
     */
    public static bool HasKeycard(this Player player)
    {
        return player?.Inventory?.UserInventory?.Items != null && player.Inventory.UserInventory.Items.Values.Any(item => item.Category == ItemCategory.Keycard);
    }
    /***
     * Check if any keycard in a player inventory has permission to open the door
     */
    public static bool AnyKeycardHasPermission(this DoorVariant door, Player player, IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards)
    {
        if (keycards == null || door == null || player == null)
            return false;
        return keycards.Any(keycard => 
        {
            var flags = keycard.GetPermissions(player as IDoorPermissionRequester);
            return door.RequiredPermissions.CheckPermissions(flags);
        });
    }
    /***
     * Check if any keycard in a player inventory has permission to open the locker chamber
     */
    public static bool AnyKeycardHasPermission(this LockerChamber locker, Player player, IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards)
    {
        if (keycards == null || locker == null || player == null)
            return false;
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAll(locker.RequiredPermissions));
    }
    /***
     * Check if any keycard in a player inventory has permission to open the generator
     */
    public static bool AnyKeycardHasPermission(this Scp079Generator generator, Player player, IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards)
    {
        if (keycards == null || generator == null || player == null)
            return false;
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAll(generator.RequiredPermissions));
    }

    public static bool AnyKeycardHasPermission(this AlphaWarheadActivationPanel panel, Player player, IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards)
    {
        if (keycards == null || panel == null || player == null)
            return false;
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAll(panel.PermissionsPolicy.RequiredPermissions));
    }
    /***
     * Attempts to toggle the door by setting the IsOpened bool
     * NOTE: There is another way to do this by triggering an action through DoorEvents using the door's NetworkState, but we use this for simplicity
     */
    public static void TryToggle(this Door door)
    {
        if (door == null)
            return;
        door.IsOpened = !door.IsOpened;
    }
    /***
     * Attempts to toggle the locker using the base game method
     */
    public static void TryToggle(this LockerChamber chamber, Locker locker)
    {
        if (chamber == null || locker == null)
            return;
        chamber.Base.SetDoor(!chamber.IsOpen, locker.Base._grantedBeep);
        locker.Base.RefreshOpenedSyncvar();
    }
    /***
     * Attempts to unlock the generator using the base game method
     */
    public static void TryUnlock(this Scp079Generator generator)
    {
        if (generator == null)
            return;
        generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
        generator._cooldownStopwatch.Restart();
    }
}