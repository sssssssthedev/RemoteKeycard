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
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role); // check for role
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard; // check for amnesia
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Door", out var doorEnabled); // check if it's activated in config
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          if (doorBase.ActiveLocks > 0 && !player.IsBypassEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Active Locks is bigger than 0 and player has bypass (Locks: {doorBase.ActiveLocks}, IsBypass: {player.IsBypassEnabled})");
               }
               return;
          }
          if (!doorEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Door is disabled in config or player is SCP or player has no items or player's role is blacklisted player's or current item is a keycard (DoorEnabled: {doorEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem})");
               }
               return;
          }
          if (!doorBase.AllowInteracting(player.ReferenceHub, 0))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Door does not allow interacting with this player (AllowInteracting: {doorBase.AllowInteracting(player.ReferenceHub, 0)})");
               }
               return;
          }
          if (!Utils.PlayerHasKeycard(player))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Player does not have a keycard in his inventory (HasKeycard: {Utils.PlayerHasKeycard(player)})");
               }
               return;
          }
          var keycard = Utils.GetPlayerKeycard(player);
          if (keycard == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingDoor(): Keycard is null after trying to get it from the player's inventory (most likely means that it was deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.KeycardHasPermissionForDoor(keycard, player, doorBase))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingDoor(): Keycard in player inventory does not have permsision to open door (KeycardName: {keycard.Name}, DoorName: {doorBase.DoorName}, KeycardHasPermsForDoor: {Utils.KeycardHasPermissionForDoor(keycard, player, doorBase)})");
               }
               return;
          }
          Utils.TryToggleDoor(door);
          args.IsAllowed = false;
     }

     public override void OnPlayerInteractingLocker(PlayerInteractingLockerEventArgs args)
     {
          var player = args.Player;
          var locker = args.Locker;
          var chamber = args.Chamber;
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role); // check for role
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard; // check for amnesia
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Locker", out var lockerEnabled); // check if it's activated in config
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          if (!lockerEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Locker is disabled in config or player is SCP or player has no items or player's role is blacklisted player's or current item is a keycard (LockerEnabled: {lockerEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem})");
               }
               return;
          }
          if (!Utils.PlayerHasKeycard(player))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Player does not have a keycard in his inventory (HasKeycard: {Utils.PlayerHasKeycard(player)})");
               }
               return;
          }
          var keycard = Utils.GetPlayerKeycard(player);
          if (keycard == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingLocker(): Keycard is null after trying to get it from the player's inventory (most likely means that it was deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.KeycardHasPermissionForLocker(keycard, player, chamber))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingLocker(): Keycard in player inventory does not have permsision to open locker chamber (KeycardName: {keycard.Name}, ChamberName: {chamber.Base.name}, KeycardHasPermsForDoor: {Utils.KeycardHasPermissionForLocker(keycard, player, chamber)})");
               }
               return;
          }
          Utils.TryToggleLocker(chamber, locker);
          args.IsAllowed = false;
     }

     public override void OnPlayerInteractingGenerator(PlayerInteractingGeneratorEventArgs args)
     {
          var player = args.Player;
          var generatorBase = args.Generator.Base;
          var blacklistedRoleFound = RemoteKeycard.Instance.Config.BlacklistedRoles.Contains(player.Role); // check for role
          var amnesiaEnabled = RemoteKeycard.Instance.Config.AmnesiaAffectsKeycard; // check for amnesia
          RemoteKeycard.Instance.Config.UseList.TryGetValue("Generator", out var generatorEnabled); // check if it's activated in config
          if (generatorBase.IsUnlocked)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Generator is already unlocked (IsUnlocked: {generatorBase.IsUnlocked})");
               }
               return;
          }
          if (amnesiaEnabled && player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Player is affected by amnesia and config has it enabled (AmnesiaEnabled: {amnesiaEnabled}, HasAmnesia: {player.GetEffect<CustomPlayerEffects.AmnesiaItems>().IsEnabled})");
               }
               return;
          }
          if (!generatorEnabled || player.IsSCP || player.IsWithoutItems || blacklistedRoleFound || player.CurrentItem is KeycardItem || args.ColliderId != Scp079Generator.GeneratorColliderId.Door)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Generator is disabled in config or player is SCP or player has no items or player's role is blacklisted or player's current item is a keycard or collider id does not match door collider (GeneratorEnabled: {generatorEnabled}, IsSCP: {player.IsSCP}, IsWithoutItems: {player.IsWithoutItems}, BlacklistedRole: {blacklistedRoleFound}, CurrentItem: {player.CurrentItem}, ColliderId: {args.ColliderId})");
               }
               return;
          }
          if (!Utils.PlayerHasKeycard(player))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Player does not have a keycard in his inventory (HasKeycard: {Utils.PlayerHasKeycard(player)})");
               }
               return;
          }
          var keycard = Utils.GetPlayerKeycard(player);
          if (keycard == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingGenerator(): Keycard is null after trying to get it from the player's inventory (most likely means that it was deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.KeycardHasPermissionForGenerator(keycard, player, generatorBase))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug($"OnPlayerInteractingGenerator(): Keycard in player inventory does not have permsision to open generator (KeycardName: {keycard.Name}, GeneratorName: {generatorBase.name}, KeycardHasPermsForDoor: {Utils.KeycardHasPermissionForGenerator(keycard, player, generatorBase)})");
               }
               return;
          }
          Utils.TryUnlockGenerator(generatorBase);
          args.IsAllowed = false;
     }
}