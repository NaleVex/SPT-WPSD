using System.Text;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using WPSD.Services;

namespace WPSD.UI
{
    /// <summary>
    /// Overlay on ModdingSelectableItemView tiles.
    /// Caption is forced hidden in LateUpdate because the game re-enables it after Create/UpdateInfo.
    /// </summary>
    public sealed class AttachmentStatOverlay : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        private TextMeshProUGUI _caption;
        private ModdingSelectableItemView _view;
        private bool _hideCaption;
        private float _captionAlpha = 1f;
        private Item _item;
        private Item _installed;
        /// <summary>True for dropdown candidates; false for the equipped slot tile.</summary>
        private bool _allowDeltas;

        public void Bind(
            ModdingSelectableItemView view,
            Item item,
            Item installedInSlot,
            bool allowDeltas)
        {
            _view = view;
            _item = item;
            _installed = installedInSlot;
            _allowDeltas = allowDeltas;
            EnsureCaption();
            EnsureLabel(view);
            Refresh();
            Plugin.Selection.Changed -= OnSelectionChanged;
            Plugin.Selection.Changed += OnSelectionChanged;
        }

        private void OnDestroy()
        {
            if (Plugin.Selection != null)
            {
                Plugin.Selection.Changed -= OnSelectionChanged;
            }

            RestoreCaption();
        }

        private void OnSelectionChanged() => Refresh();

        private void LateUpdate()
        {
            if (!_hideCaption)
            {
                return;
            }

            EnsureCaption();
            ApplyCaptionHidden();
        }

        public void Refresh()
        {
            if (_label == null || _item == null)
            {
                return;
            }

            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value)
            {
                _label.text = string.Empty;
                _hideCaption = false;
                RestoreCaption();
                return;
            }

            var selection = Plugin.Selection;
            // Slot tiles always show absolute contribution; deltas only in the picker list.
            var asDelta = _allowDeltas && selection.ShowDeltas;
            var values = AttachmentStatResolver.Resolve(_item, _installed, asDelta);
            var sb = new StringBuilder();
            foreach (var id in selection.EnabledStats)
            {
                if (!values.TryGetValue(id, out var value))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(AttachmentStatResolver.FormatOverlayLine(id, value, asDelta));
            }

            _label.text = sb.ToString();
            _label.fontSize = Plugin.Settings.OverlayFontSize.Value;
            _label.color = Plugin.Settings.OverlayColor.Value;

            _hideCaption = sb.Length > 0;
            EnsureCaption();
            if (_hideCaption)
            {
                ApplyCaptionHidden();
            }
            else
            {
                RestoreCaption();
            }
        }

        private void EnsureCaption()
        {
            if (_caption != null || _view == null)
            {
                return;
            }

            _caption = _view.Caption;
            if (_caption != null)
            {
                _captionAlpha = _caption.alpha;
                if (_captionAlpha <= 0.01f)
                {
                    _captionAlpha = 1f;
                }
            }
        }

        private void ApplyCaptionHidden()
        {
            if (_caption == null)
            {
                return;
            }

            // Prefer alpha over SetActive — UpdateItemName / UpdateInfoVisibility re-enable the GO.
            _caption.alpha = 0f;
            _caption.maxVisibleCharacters = 0;
        }

        private void RestoreCaption()
        {
            if (_caption == null)
            {
                return;
            }

            _caption.alpha = _captionAlpha;
            _caption.maxVisibleCharacters = int.MaxValue;
        }

        private void EnsureLabel(ModdingSelectableItemView view)
        {
            if (_label != null)
            {
                return;
            }

            var go = new GameObject("WPSD_Overlay", typeof(RectTransform));
            go.transform.SetParent(view.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -2f);
            rt.sizeDelta = new Vector2(-4f, 42f);

            _label = go.AddComponent<TextMeshProUGUI>();
            _label.alignment = TextAlignmentOptions.TopLeft;
            _label.fontSize = Plugin.Settings?.OverlayFontSize.Value ?? 12f;
            _label.color = Plugin.Settings?.OverlayColor.Value ?? Color.white;
            _label.raycastTarget = false;
            _label.text = string.Empty;
        }

        public static void Attach(
            ModdingSelectableItemView view,
            Item item,
            Item installedInSlot,
            bool allowDeltas)
        {
            if (view == null || item == null)
            {
                return;
            }

            var overlay = view.GetComponent<AttachmentStatOverlay>();
            if (overlay == null)
            {
                overlay = view.gameObject.AddComponent<AttachmentStatOverlay>();
            }

            overlay.Bind(view, item, installedInSlot, allowDeltas);
        }
    }
}
