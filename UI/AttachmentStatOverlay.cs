using System.Text;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using WeaponConfigStats.Services;

namespace WeaponConfigStats.UI
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

        public void Bind(ModdingSelectableItemView view, Item item, Item installedInSlot)
        {
            _view = view;
            _item = item;
            _installed = installedInSlot;
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
            var values = AttachmentStatResolver.Resolve(_item, _installed, selection.ShowDeltas);
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

                sb.Append(AttachmentStatResolver.FormatOverlayLine(id, value, selection.ShowDeltas));
            }

            _label.text = sb.ToString();
            _label.fontSize = Plugin.Settings.OverlayFontSize.Value;

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

            var go = new GameObject("WeaponConfigStats_Overlay", typeof(RectTransform));
            go.transform.SetParent(view.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -2f);
            rt.sizeDelta = new Vector2(-4f, 42f);

            _label = go.AddComponent<TextMeshProUGUI>();
            _label.alignment = TextAlignmentOptions.TopLeft;
            _label.fontSize = 12f;
            _label.color = new Color(1f, 0.85f, 0.35f, 1f);
            _label.raycastTarget = false;
            _label.text = string.Empty;
        }

        public static void Attach(ModdingSelectableItemView view, Item item, Item installedInSlot)
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

            overlay.Bind(view, item, installedInSlot);
        }
    }
}
