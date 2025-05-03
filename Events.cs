using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;

namespace RemoteKeycardLabApi;

public class Events : CustomEventsHandler
{
     public override void OnPlayerInteractingDoor(PlayerInteractingDoorEventArgs args) 
     {
          var player = args.Player;
          var door = args.Door;
          var doorBase = door.Base;
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role); 
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard;
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Door", out var doorEnabled);
          // Checking if Amnesia is an enabled feature in the config and if the player actually has Amnesia
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          // Checking if the door doesn't have any locks placed on it and if the player doesn't have bypass enabled
          if (doorBase.ActiveLocks > 0 && !player.IsBypassEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Active Locks is bigger than 0 and player doesn't have bypass (Locks: {doorBase.ActiveLocks}, IsBypass: {player.IsBypassEnabled})");
               }
               return;
          }
          // Checking if Door is an enabled feature in the config or if the player is an SCP or if the player has no items or if the player has a blacklisted role or if the player's current item is a keycard
          if (!doorEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Door is disabled in config or player is SCP or player has no items or player's role is blacklisted player's or current item is a keycard (DoorEnabled: {doorEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem})");
               }
               return;
          }
          // Checking if the player can actually interact with the door
          if (!doorBase.AllowInteracting(player.ReferenceHub, 0))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Door does not allow interacting with this player (AllowInteracting: {doorBase.AllowInteracting(player.ReferenceHub, 0)})");
               }
               return;
          }
          // Checking if the player has any keycard in his inventory
          if (!player.HasKeycard())
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Player does not have a keycard in his inventory (HasKeycard: {player.HasKeycard()})");
               }
               return;
          }
          var keycards = player.GetKeycards();
          // Checking if a keycard or multiple keycards actually exist or not, good practice in cases where something wrong happens
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingDoor(): Keycards are null after trying to get them from the player's inventory (most likely means that the player's inventory was cleared or the keycards got removed after the PlayerHasKeycard check)");
               }
               return;
          }
          // Checking if any of the keycards the player owns has permission to open the door
          if (!doorBase.AnyKeycardHasPermission(player, keycards))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingDoor(): Keycards in player inventory do not have permission to open door (KeycardNames: [{keycardNames}], DoorName: {doorBase.DoorName}, KeycardHasPerms: {doorBase.AnyKeycardHasPermission(player, keycards)})");
               }
               return;
          }
          // Finally, we trigger the toggle for the door, and we cancel the event because we don't want to trigger it twice
          door.TryToggle();
          args.IsAllowed = false;
     }

     public override void OnPlayerInteractingLocker(PlayerInteractingLockerEventArgs args)
     {
          var player = args.Player;
          var locker = args.Locker;
          var chamber = args.Chamber;
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role);
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard;
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Locker", out var lockerEnabled);
          // Checking if Amnesia is an enabled feature in the config and if the player actually has Amnesia
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          // Checking if Locker is an enabled feature in the config or if the player is an SCP or if the player has no items or if the player has a blacklisted role or if the player's current item is a keycard
          if (!lockerEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Locker is disabled in config or player is SCP or player has no items or player's role is blacklisted player's or current item is a keycard (LockerEnabled: {lockerEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem})");
               }
               return;
          }
          // Checking if the player has any keycard in his inventory
          if (!player.HasKeycard())
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Player does not have a keycard in his inventory (HasKeycard: {player.HasKeycard()})");
               }
               return;
          }
          var keycards = player.GetKeycards();
          // Checking if a keycard or multiple keycards actually exist or not, good practice in cases where something wrong happens
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingLocker(): Keycards are null after trying to get them from the player's inventory (most likely means that the player's inventory was cleared or the keycards got removed after the PlayerHasKeycard check)");
               }
               return;
          }
          // Checking if any of the keycards the player owns has permission to open the locker chamber
          if (!chamber.AnyKeycardHasPermission(player, keycards))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingLocker(): Keycards in player inventory do not have permission to open locker chamber (KeycardNames: [{keycardNames}], ChamberName: {chamber.Base.name}, KeycardHasPerms: {chamber.AnyKeycardHasPermission(player, keycards)})");
               }
               return;
          }
          // Finally, we trigger the toggle for the locker chamber, and we cancel the event because we don't want to trigger it twice
          chamber.TryToggle(locker);
          args.IsAllowed = false;
     }

     public override void OnPlayerInteractingGenerator(PlayerInteractingGeneratorEventArgs args)
     {
          var player = args.Player;
          var generatorBase = args.Generator.Base;
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role);
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard; 
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Generator", out var generatorEnabled);
          // Checking if the generator is already unlocked, this is crucial in making sure we can open an unlocked generator even with a keycard in inventory
          if (generatorBase.IsUnlocked)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Generator is already unlocked (IsUnlocked: {generatorBase.IsUnlocked})");
               }
               return;
          }
          // Checking if Amnesia is an enabled feature in the config and if the player actually has Amnesia
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          // Checking if Generator is an enabled feature in the config or if the player is an SCP or if the player has no items or if the player has a blacklisted role or if the player's current item is a keycard or if the ColliderId is not equal to the Door ColliderId
          // ColliderId is necessary to make sure that we can only open the generator by interacting with the door collider, and not with any of the other colliders, and also making sure the switch/close is working with a keycard in inventory
          if (!generatorEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem || args.ColliderId != Scp079Generator.GeneratorColliderId.Door)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Generator is disabled in config or player is SCP or player has no items or player's role is blacklisted or player's current item is a keycard or collider id does not match door collider (GeneratorEnabled: {generatorEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem}, ColliderId: {args.ColliderId})");
               }
               return;
          }
          // Checking if the player has any keycard in his inventory
          if (!player.HasKeycard())
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Player does not have a keycard in his inventory (HasKeycard: {player.HasKeycard()})");
               }
               return;
          }
          var keycards = player.GetKeycards();
          // Checking if a keycard or multiple keycards actually exist or not, good practice in cases where something wrong happens
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingGenerator(): Keycards are null after trying to get them from the player's inventory (most likely means that the player's inventory was cleared or the keycards got removed after the PlayerHasKeycard check)");
               }
               return;
          }
          // Checking if any of the keycards the player owns has permission to open the generator
          if (!generatorBase.AnyKeycardHasPermission(player, keycards))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingGenerator(): Keycards in player inventory do not have permission to open generator (KeycardNames: [{keycardNames}], GeneratorName: {generatorBase.name}, KeycardHasPerms: {generatorBase.AnyKeycardHasPermission(player, keycards)})");
               }
               return;
          }
          // Finally, we unlock the generator, and we cancel the event because we don't want to trigger it twice
          generatorBase.TryUnlock();
          args.IsAllowed = false;
     }

     public override void OnPlayerUnlockingWarheadButton(PlayerUnlockingWarheadButtonEventArgs args)
     {
          var player = args.Player;
          var panel = new AlphaWarheadActivationPanel();
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role);
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard; 
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Warhead", out var warheadEnabled);
          // Checking if Amnesia is an enabled feature in the config and if the player actually has Amnesia
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerUnlockingWarheadButton(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          // Checking if Warhead is an enabled feature in the config or if the player is an SCP or if the player has no items or if the player has a blacklisted role or if the player's current item is a keycard
          if (!warheadEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerUnlockingWarheadButton(): Warhead is disabled in config or player is SCP or player has no items or player's role is blacklisted player's or current item is a keycard (DoorEnabled: {warheadEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem})");
               }
               return;
          }
          // Checking if the player has any keycard in his inventory
          if (!player.HasKeycard())
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerUnlockingWarheadButton(): Player does not have a keycard in his inventory (HasKeycard: {player.HasKeycard()})");
               }
               return;
          }
          var keycards = player.GetKeycards();
          // Checking if a keycard or multiple keycards actually exist or not, good practice in cases where something wrong happens
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerUnlockingWarheadButton(): Keycards are null after trying to get them from the player's inventory (most likely means that the player's inventory was cleared or the keycards got removed after the PlayerHasKeycard check)");
               }
               return;
          }
          // Checking if any of the keycards the player owns has permission to open the warhead
          if (!panel.AnyKeycardHasPermission(player, keycards))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerUnlockingWarheadButton(): Keycards in player inventory do not have permission to open warhead (KeycardNames: [{keycardNames}], PanelName: {panel.name}, KeycardHasPerms: {panel.AnyKeycardHasPermission(player, keycards)})");
               }
               return;
          }
          panel.TryUnlock();
          args.IsAllowed = false;
     }
}