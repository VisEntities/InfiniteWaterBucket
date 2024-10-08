﻿/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Infinite Water Bucket", "VisEntities", "1.0.1")]
    [Description("Provides an infinite water source by automatically refilling water containers.")]
    public class InfiniteWaterBucket : RustPlugin
    {
        #region Fields

        private static InfiniteWaterBucket _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Auto Refillable Item Short Names")]
            public List<string> AutoRefillableItemShortNames { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                AutoRefillableItemShortNames = new List<string>
                {
                    "bucket.water",
                    "waterjug",
                    "smallwaterbottle",
                    "botabag"
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private int OnItemUse(Item item, int consumptionAmount)
        {
            if (item == null || (item.info.shortname != "water" && item.info.shortname != "water.salt"))
                return consumptionAmount;

            if (item.parentItem == null || item.parentItem.info == null)
                return consumptionAmount;

            if (!_config.AutoRefillableItemShortNames.Contains(item.parentItem.info.shortname))
                return consumptionAmount;

            ItemContainer rootContainer = item.GetRootContainer();
            if (rootContainer == null)
                return consumptionAmount;

            BasePlayer player = rootContainer.playerOwner;
            if (player == null)
                return consumptionAmount;

            if (!PermissionUtil.HasPermission(player, PermissionUtil.USE))
                return consumptionAmount;

            return 0;
        }

        #endregion Oxide Hooks

        #region Permissions

        private static class PermissionUtil
        {
            public const string USE = "infinitewaterbucket.use";
            private static readonly List<string> _permissions = new List<string>
            {
                USE,
            };

            public static void RegisterPermissions()
            {
                foreach (var permission in _permissions)
                {
                    _plugin.permission.RegisterPermission(permission, _plugin);
                }
            }

            public static bool HasPermission(BasePlayer player, string permissionName)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Permissions
    }
}