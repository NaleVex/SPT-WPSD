using System;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WPSD.Models;

namespace WPSD.UI
{
    /// <summary>
    /// Per-stat eye + sort buttons injected beside a CharacteristicsPanel row (green area).
    /// Visuals are placeholders; layout will be tuned against CompactCharacteristicPanel.
    /// </summary>
    public sealed class StatRowControls : MonoBehaviour
    {
        private WeaponStatId _statId;
        private Button _toggleButton;
        private Button _sortButton;
        private TextMeshProUGUI _toggleLabel;
        private TextMeshProUGUI _sortLabel;
        private Action<WeaponStatId> _onToggle;
        private Action<WeaponStatId> _onSort;
        private TMP_FontAsset _font;
        private Material _fontMaterial;

        public WeaponStatId StatId => _statId;

        public void Init(
            WeaponStatId statId,
            Action<WeaponStatId> onToggle,
            Action<WeaponStatId> onSort,
            CompactCharacteristicPanel sourceRow = null)
        {
            _statId = statId;
            _onToggle = onToggle;
            _onSort = onSort;
            CaptureFont(sourceRow);
            EnsureUi();
            Refresh();
        }

        public void Refresh()
        {
            if (_toggleLabel == null)
            {
                return;
            }

            var selection = Plugin.Selection;
            var enabled = selection.IsEnabled(_statId);
            _toggleLabel.text = enabled ? "[*]" : "[ ]";

            if (selection.SortStat == _statId && selection.SortDirection != SortDirection.None)
            {
                _sortLabel.text = selection.SortDirection == SortDirection.Ascending ? "A" : "D";
            }
            else
            {
                _sortLabel.text = "-";
            }
        }

        private void CaptureFont(CompactCharacteristicPanel sourceRow)
        {
            if (sourceRow == null)
            {
                return;
            }

            var source = sourceRow.NameText != null ? sourceRow.NameText : sourceRow.ValueText;
            if (source == null)
            {
                return;
            }

            _font = source.font;
            _fontMaterial = source.fontSharedMaterial;
        }

        private void EnsureUi()
        {
            if (_toggleButton != null)
            {
                return;
            }

            var root = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            root.sizeDelta = new Vector2(48f, 20f);

            _toggleButton = CreateButton("Toggle", new Vector2(-12f, 0f), out _toggleLabel);
            _sortButton = CreateButton("Sort", new Vector2(12f, 0f), out _sortLabel);

            _toggleButton.onClick.AddListener(() => _onToggle?.Invoke(_statId));
            _sortButton.onClick.AddListener(() => _onSort?.Invoke(_statId));
        }

        private Button CreateButton(string name, Vector2 anchoredPos, out TextMeshProUGUI label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(22f, 18f);
            rt.anchoredPosition = anchoredPos;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
            image.maskable = false;
            image.raycastTarget = true;

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            label = textGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 11f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.text = "?";
            label.maskable = false;
            label.raycastTarget = false;
            if (_font != null)
            {
                label.font = _font;
            }

            if (_fontMaterial != null)
            {
                label.fontSharedMaterial = _fontMaterial;
            }

            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }
    }
}
