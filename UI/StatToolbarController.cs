using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;
using WeaponConfigStats.Models;
using WeaponConfigStats.Services;

namespace WeaponConfigStats.UI
{
    /// <summary>
    /// Toggle/sort controls on the left side of each numeric characteristic row.
    /// Rebuild is always deferred to end-of-frame so InstantiateItemFields can finish first.
    /// Rows under a collapsed _characteristics parent are still bound (inactive parents are OK).
    /// </summary>
    public sealed class StatToolbarController : MonoBehaviour
    {
        private static readonly FieldInfo ItemAttributeField =
            typeof(CompactCharacteristicPanel).GetField(
                "ItemAttribute",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo CompactTemplateField =
            typeof(CharacteristicsPanel).GetField(
                "_compactCharTemplate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo FullTemplateField =
            typeof(CharacteristicsPanel).GetField(
                "_charPanelTemplate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private CharacteristicsPanel _panel;
        private readonly List<StatRowControls> _rows = new List<StatRowControls>();
        private Coroutine _rebuildRoutine;
        private int _requestId;

        public void RequestRebuild(CharacteristicsPanel panel)
        {
            _panel = panel;
            _requestId++;
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_rebuildRoutine != null)
            {
                StopCoroutine(_rebuildRoutine);
            }

            _rebuildRoutine = StartCoroutine(RebuildWhenReady(_requestId));
        }

        private IEnumerator RebuildWhenReady(int requestId)
        {
            // Let CharacteristicsPanel / ViewList finish creating compact rows.
            yield return null;
            if (requestId != _requestId)
            {
                yield break;
            }

            RebuildRows();

            // Second pass — some attributes bind one frame later.
            yield return null;
            if (requestId != _requestId)
            {
                yield break;
            }

            RebuildRows();
            _rebuildRoutine = null;
        }

        private void OnDestroy()
        {
            if (Plugin.Selection != null)
            {
                Plugin.Selection.Changed -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged()
        {
            foreach (var row in _rows)
            {
                if (row != null)
                {
                    row.Refresh();
                }
            }
        }

        private void ClearRows()
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (row != null)
                {
                    Destroy(row.gameObject);
                }
            }

            _rows.Clear();

            if (_panel == null)
            {
                return;
            }

            var orphans = new List<GameObject>();
            foreach (var t in _panel.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name.StartsWith("WeaponConfigStats_Row_"))
                {
                    orphans.Add(t.gameObject);
                }
            }

            foreach (var go in orphans)
            {
                Destroy(go);
            }
        }

        private void RebuildRows()
        {
            ClearRows();
            if (_panel == null)
            {
                return;
            }

            Plugin.Selection.Changed -= OnSelectionChanged;
            Plugin.Selection.Changed += OnSelectionChanged;

            var compactTemplate = CompactTemplateField?.GetValue(_panel) as CompactCharacteristicPanel;
            var fullTemplate = FullTemplateField?.GetValue(_panel) as CharacteristicPanel;

            var seen = new HashSet<WeaponStatId>();
            var panels = _panel.GetComponentsInChildren<CompactCharacteristicPanel>(true);
            var found = 0;
            var templateSkipped = 0;
            var noAttribute = 0;
            var unmapped = 0;
            var mapped = 0;

            foreach (var charPanel in panels)
            {
                if (charPanel == null)
                {
                    continue;
                }

                found++;

                // Skip inactive templates sitting in the hierarchy (not collapsed rows).
                if (ReferenceEquals(charPanel, compactTemplate) || ReferenceEquals(charPanel, fullTemplate))
                {
                    templateSkipped++;
                    continue;
                }

                var attribute = ItemAttributeField?.GetValue(charPanel) as ItemAttribute;
                if (attribute?.Id == null)
                {
                    noAttribute++;
                    continue;
                }

                if (!WeaponStatMapper.TryMap(attribute.Id, out var statId))
                {
                    unmapped++;
                    continue;
                }

                if (!seen.Add(statId))
                {
                    continue;
                }

                var rowRt = charPanel.transform as RectTransform;
                if (rowRt == null)
                {
                    continue;
                }

                mapped++;
                var rowGo = new GameObject($"WeaponConfigStats_Row_{statId}", typeof(RectTransform));
                rowGo.transform.SetParent(rowRt, false);

                var rt = rowGo.GetComponent<RectTransform>();
                // Hang just left of the row so labels/icons stay readable.
                // Graphics use maskable=false so RectMask2D does not clip them.
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-4f, 0f);
                rt.sizeDelta = new Vector2(48f, 18f);
                rt.SetAsLastSibling();

                var controls = rowGo.AddComponent<StatRowControls>();
                controls.Init(statId, OnToggle, OnSort, charPanel);
                _rows.Add(controls);
            }

            Plugin.Log?.LogInfo(
                $"Stat toolbar: found={found}, templates={templateSkipped}, noAttr={noAttribute}, unmapped={unmapped}, mapped={mapped}, controls={_rows.Count}");
        }

        private void OnToggle(WeaponStatId id) => Plugin.Selection.ToggleStat(id);

        private void OnSort(WeaponStatId id) => Plugin.Selection.CycleSort(id);

        public static StatToolbarController EnsureOn(CharacteristicsPanel panel)
        {
            if (panel == null)
            {
                return null;
            }

            var host = panel.gameObject;
            var existing = host.GetComponent<StatToolbarController>();
            if (existing == null)
            {
                existing = host.AddComponent<StatToolbarController>();
            }

            existing.RequestRebuild(panel);
            return existing;
        }
    }
}
