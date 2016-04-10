ï»¿using System;

namespace Oxide.Plugins
{
    [Info("NoSleepers", "Wulf/lukespragg", "0.3.1", ResourceId = 1452)]
    [Description("Disables sleepers by killing players on quit.")]

    class NoSleepers : RustPlugin
    {
        // Do NOT edit this file, instead edit NoSleepers.json in server/<identity>/oxide/config

        #region Configuration

        // Settings
        bool AdminCanSleep => GetConfig("AdminCanSleep", true);
        bool KillExisting => GetConfig("KillExisting", true);
        bool RemoveCorpses => GetConfig("RemoveCorpses", false);

        protected override void LoadDefaultConfig()
        {
            // Settings
            Config["AdminCanSleep"] = AdminCanSleep;
            Config["KillExisting"] = KillExisting;
            Config["RemoveCorpses"] = RemoveCorpses;

            SaveConfig();
        }

        #endregion

        #region General Setup

        void Loaded() => LoadDefaultConfig();

        void OnServerInitialized()
        {
            if (!KillExisting) return;

            var killCount = 0;
            var sleepers = BasePlayer.sleepingPlayerList;
            foreach (var sleeper in sleepers.ToArray())
            {
                if (AdminCanSleep && sleeper.IsAdmin()) continue;
                if (sleeper.IsDead()) sleepers.Remove(sleeper);
                sleeper.KillMessage();
                killCount++;
            }
            if (killCount > 0) Puts($"Killed {killCount} sleepers");
        }

        #endregion

        #region Sleeper/Corpse Removal

        void OnPlayerInit(BasePlayer player)
        {
            if (player.IsDead()) player.Respawn();
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (IsAdmin(player) && AdminCanSleep) return;
            if (player.IsSleeping()) player.EndSleeping();
        }

        void OnPlayerDisconnected(BasePlayer player)
        {
            if (IsAdmin(player) && AdminCanSleep) return;
            if (!player.IsDead()) player.KillMessage();
        }

        object OnPlayerSleep(BasePlayer player)
        {
            return IsAdmin(player) && AdminCanSleep ? (object)null : true;
        }

        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (RemoveCorpses && entity.name.EndsWith("player_corpse.prefab")) entity.KillMessage();
        }

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        bool IsAdmin(BasePlayer player) => (permission.UserHasGroup(player.UserIDString, "admin") || player.net.connection.authLevel > 0);

        #endregion
    }
}