using System.Collections.Generic;
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
          var keycards = Utils.GetPlayerKeycards(player);
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingDoor(): Keycards are null after trying to get them from the player's inventory (most likely means that either one or multiple were deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.AnyKeycardHasPermissionForDoor(keycards, player, doorBase))
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingDoor(): Keycards in player inventory do not have permission to open door (KeycardNames: [{keycardNames}], DoorName: {doorBase.DoorName}, KeycardHasPermsForDoor: {Utils.AnyKeycardHasPermissionForDoor(keycards, player, doorBase)})");
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
          var keycards = Utils.GetPlayerKeycards(player);
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingLocker(): Keycards are null after trying to get them from the player's inventory (most likely means that either one or multiple were deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.AnyKeycardHasPermissionForLocker(keycards, player, chamber))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingLocker(): Keycards in player inventory do not have permission to open locker (KeycardNames: [{keycardNames}], ChamberName: {chamber.Base.name}, KeycardHasPermsForLocker: {Utils.AnyKeycardHasPermissionForLocker(keycards, player, chamber)})");
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
          var keycards = Utils.GetPlayerKeycards(player);
          if (keycards == null)
          {
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    Logger.Debug("OnPlayerInteractingGenerator(): Keycards are null after trying to get them from the player's inventory (most likely means that either one or multiple were deleted after the PlayerHasKeycard check)");
               }
               return;
          }
          if (!Utils.AnyKeycardHasPermissionForGenerator(keycards, player, generatorBase))
          { 
               if (RemoteKeycard.Instance.Config.Debug)
               {
                    var keycardNames = keycards.Any() ? string.Join(", ", keycards.Select(k => k.ItemTypeId.ToString())) : "None";
                    Logger.Debug($"OnPlayerInteractingGenerator(): Keycards in player inventory do not have permission to open generator (KeycardNames: [{keycardNames}], GeneratorName: {generatorBase.name}, KeycardHasPermsForGen: {Utils.AnyKeycardHasPermissionForGenerator(keycards, player, generatorBase)})");
               }
               return;
          }
          Utils.TryUnlockGenerator(generatorBase);
          args.IsAllowed = false;
     }
}