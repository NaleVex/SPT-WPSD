using System;
using System.Collections.Generic;
using WeaponConfigStats.Config;
using WeaponConfigStats.Models;

namespace WeaponConfigStats.Services
{
    /// <summary>
    /// Runtime selection state for toggles / sort. Persists through ModConfig (F12).
    /// </summary>
    public sealed class StatSelectionService
    {
        private readonly ModConfig _config;
        private readonly HashSet<WeaponStatId> _enabled;

        public event Action Changed;

        public StatSelectionService(ModConfig config)
        {
            _config = config;
            _enabled = config.GetEnabledStats();
        }

        public bool IsEnabled(WeaponStatId id) => _enabled.Contains(id);

        public IReadOnlyCollection<WeaponStatId> EnabledStats => _enabled;

        public WeaponStatId SortStat => _config.SortStat.Value;

        public SortDirection SortDirection => _config.SortDirection.Value;

        public bool ShowDeltas => _config.ShowDeltasVsInstalled.Value;

        public void ToggleStat(WeaponStatId id)
        {
            if (!_enabled.Add(id))
            {
                _enabled.Remove(id);
            }

            _config.SetEnabledStats(_enabled);
            RaiseChanged();
        }

        public void SetSort(WeaponStatId id, SortDirection direction)
        {
            if (_config.SortStat.Value == id && _config.SortDirection.Value == direction)
            {
                _config.SortDirection.Value = SortDirection.None;
            }
            else
            {
                _config.SortStat.Value = id;
                _config.SortDirection.Value = direction;
            }

            RaiseChanged();
        }

        public void CycleSort(WeaponStatId id)
        {
            if (_config.SortStat.Value != id || _config.SortDirection.Value == SortDirection.None)
            {
                SetSort(id, SortDirection.Descending);
                return;
            }

            if (_config.SortDirection.Value == SortDirection.Descending)
            {
                SetSort(id, SortDirection.Ascending);
                return;
            }

            SetSort(id, SortDirection.None);
        }

        private void RaiseChanged() => Changed?.Invoke();
    }
}
