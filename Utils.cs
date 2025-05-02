using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Locker = LabApi.Features.Wrappers.Locker;
using LockerChamber = LabApi.Features.Wrappers.LockerChamber;

namespace RemoteKeycardLabApi;

/**
 * Helper class used for utilty
 * TODO: Move all of this to wrappers instead
 */
public class Utils
{
    /***
     * Returns all the keycards a player has in a IEnumerable format
     */
    public static IEnumerable<InventorySystem.Items.Keycards.KeycardItem> GetPlayerKeycards(Player player)
    {
        return player?.Inventory?.UserInventory?.Items == null ? Enumerable.Empty<InventorySystem.Items.Keycards.KeycardItem>() : player.Inventory.UserInventory.Items.Values.Where(item => item.Category == ItemCategory.Keycard).Select(item => item as InventorySystem.Items.Keycards.KeycardItem).Where(keycard => keycard != null);
    }
    /***
     * Checks if a player has any keycard in their inventory
     */
    public static bool PlayerHasKeycard(Player player)
    {
        return player?.Inventory?.UserInventory?.Items != null && player.Inventory.UserInventory.Items.Values.Any(item => item.Category == ItemCategory.Keycard);
    }
    /***
     * Check if any keycard in a player inventory has permission to open the door
     */
    public static bool AnyKeycardHasPermissionForDoor(IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards, Player player, DoorVariant door)
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
    public static bool AnyKeycardHasPermissionForLocker(IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards, Player player, LockerChamber locker)
    {
        if (keycards == null || locker == null || player == null)
            return false;
        
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAll(locker.RequiredPermissions));
    }
    /***
     * Check if any keycard in a player inventory has permission to open the generator
     */
    public static bool AnyKeycardHasPermissionForGenerator(IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards, Player player, Scp079Generator generator)
    {
        if (keycards == null || generator == null || player == null)
            return false;
        
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAll(generator.RequiredPermissions));
    }
    /***
     * Attempts to toggle the door by setting the IsOpened flag
     * NOTE: There is another way to do this by triggering an action through DoorEvents using the door's NetworkState, but we use this for simplicity
     */
    public static void TryToggleDoor(Door door)
    {
        if (door == null)
            return;
        door.IsOpened = !door.IsOpened;
    }
    /***
     * Attempts to toggle the locker using the base game method
     */
    public static void TryToggleLocker(LockerChamber chamber, Locker locker)
    {
        chamber.Base.SetDoor(!chamber.IsOpen, locker.Base._grantedBeep);
        locker.Base.RefreshOpenedSyncvar();
    }
    /***
     * Attempts to unlock the generator using the base game method
     * NOTE: This does not grant tickets, don't know if it's necessary or not
     */
    public static void TryUnlockGenerator(Scp079Generator generator)
    {
        generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
        generator._cooldownStopwatch.Restart();
    }
}