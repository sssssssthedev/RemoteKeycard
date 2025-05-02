using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Locker = LabApi.Features.Wrappers.Locker;
using LockerChamber = LabApi.Features.Wrappers.LockerChamber;

namespace RemoteKeycardLabApi;

public class Utils
{
    public static IEnumerable<InventorySystem.Items.Keycards.KeycardItem> GetPlayerKeycards(Player player)
    {
        return player?.Inventory?.UserInventory?.Items == null ? Enumerable.Empty<InventorySystem.Items.Keycards.KeycardItem>() : player.Inventory.UserInventory.Items.Values.Where(item => item.Category == ItemCategory.Keycard).Select(item => item as InventorySystem.Items.Keycards.KeycardItem).Where(keycard => keycard != null);
    }
    public static bool PlayerHasKeycard(Player player)
    {
        return player?.Inventory?.UserInventory?.Items != null && player.Inventory.UserInventory.Items.Values.Any(item => item.Category == ItemCategory.Keycard);
    }
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
    public static bool AnyKeycardHasPermissionForLocker(IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards, Player player, LockerChamber locker)
    {
        if (keycards == null || locker == null || player == null)
            return false;
        
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAny(locker.RequiredPermissions));
    }
    public static bool AnyKeycardHasPermissionForGenerator(IEnumerable<InventorySystem.Items.Keycards.KeycardItem> keycards, Player player, Scp079Generator generator)
    {
        if (keycards == null || generator == null || player == null)
            return false;
        
        return keycards.Any(keycard => keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAny(generator.RequiredPermissions));
    }
    public static void TryToggleDoor(Door door)
    {
        if (door == null)
            return;
        door.IsOpened = !door.IsOpened;
    }
    public static void TryToggleLocker(LockerChamber chamber, Locker locker)
    {
        chamber.Base.SetDoor(!chamber.IsOpen, locker.Base._grantedBeep);
        locker.Base.RefreshOpenedSyncvar();
    }
    public static void TryUnlockGenerator(Scp079Generator generator)
    {
        generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
        generator._cooldownStopwatch.Restart();
    }
}