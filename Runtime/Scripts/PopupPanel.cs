using UnityEngine;
using UnityEngine.UIElements;

namespace Ferreira.ColorPicker
{
    [UxmlElement(visibility = LibraryVisibility.Hidden)]
    public partial class PopupPanel : VisualElement
    {
        [UxmlAttribute]
        public int FadeTime { get; set; } = 30;

        private readonly bool StartVisible = false;

        private IVisualElementScheduledItem task;

        private const string stylesResource = "ColorPicker/Styles/PopupPanelStyleSheet";
        private const string ussClassName = "popup-panel";

        public PopupPanel()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(stylesResource));
            AddToClassList(ussClassName);

            SetStartVisibility(false);

            RegisterCallback<AttachToPanelEvent>(evt => SetStartVisibility(StartVisible));
        }

        public virtual void Show()
        {
            task?.Pause();
            task = null;

            if (FadeTime > 0.0f)
            {
                style.visibility = Visibility.Visible;
                style.opacity = 0f;
                task = schedule
                    .Execute(() => style.opacity = Mathf.Clamp01(resolvedStyle.opacity + 0.1f))
                    .Every(FadeTime) // ms	
                    .Until(() => resolvedStyle.opacity >= 1.0f);
            }
            else
            {
                style.visibility = Visibility.Visible;
                style.opacity = 1f;
            }

            Focus();
        }

        public virtual void Hide()
        {
            task?.Pause();
            task = null;

            Blur();

            if (FadeTime > 0.0f)
            {
                task = schedule
                    .Execute(() =>
                    {
                        var o = Mathf.Clamp01(resolvedStyle.opacity - 0.1f);
                        style.opacity = o;
                        if (o <= 0.0f) style.visibility = Visibility.Hidden;
                    })
                    .Every(FadeTime) // ms	
                    .Until(() => resolvedStyle.opacity <= 0.0f);
            }
            else
            {
                style.visibility = Visibility.Hidden;
                style.opacity = 0f;
            }
        }

        private void SetStartVisibility(bool isVisible)
        {
            if (isVisible)
            {
                style.visibility = Visibility.Visible;
                style.opacity = 1f;
            }
            else
            {
                style.visibility = Visibility.Hidden;
                style.opacity = 0f;
            }
        }
    }
}
