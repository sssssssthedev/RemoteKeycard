using System.Linq;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Locker = LabApi.Features.Wrappers.Locker;
using LockerChamber = LabApi.Features.Wrappers.LockerChamber;

namespace RemoteKeycardLabApi;

public class Utils
{
    public static InventorySystem.Items.Keycards.KeycardItem GetPlayerKeycard(Player player)
    {
        return player?.Inventory?.UserInventory?.Items == null ? null : (from item in player.Inventory.UserInventory.Items.Values where item.Category == ItemCategory.Keycard select item as InventorySystem.Items.Keycards.KeycardItem).FirstOrDefault();
    }
    public static bool PlayerHasKeycard(Player player)
    {
        return player?.Inventory?.UserInventory?.Items != null && player.Inventory.UserInventory.Items.Values.Any(item => item.Category == ItemCategory.Keycard);
    }
    public static bool KeycardHasPermissionForDoor(InventorySystem.Items.Keycards.KeycardItem keycard, Player player, DoorVariant door)
    {
        if (keycard == null || door == null || player == null)
            return false;
        var flags = keycard.GetPermissions(player as IDoorPermissionRequester);
        return door.RequiredPermissions.CheckPermissions(flags);
    }
    public static bool KeycardHasPermissionForLocker(InventorySystem.Items.Keycards.KeycardItem keycard, Player player, LockerChamber locker)
    {
        if (keycard == null || locker == null || player == null)
            return false;
        return keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAny(locker.RequiredPermissions);
    }
    public static bool KeycardHasPermissionForGenerator(InventorySystem.Items.Keycards.KeycardItem keycard, Player player, Scp079Generator generator)
    {
        if (keycard == null || generator == null || player == null)
            return false;
        return keycard.GetPermissions(player as IDoorPermissionRequester).HasFlagAny(generator.RequiredPermissions);
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