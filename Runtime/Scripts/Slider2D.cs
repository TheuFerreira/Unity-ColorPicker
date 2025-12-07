using UnityEngine;
using UnityEngine.UIElements;

namespace Ferreira.ColorPicker
{
    [UxmlElement(visibility = LibraryVisibility.Hidden)]
    public partial class Slider2D : BaseField<Vector2>
    {
        [UxmlAttribute("min-x")]
        public float MinX { get; set; } = 0f;

        [UxmlAttribute("min-y")]
        public float MinY { get; set; } = 0f;

        [UxmlAttribute("max-x")]
        public float MaxX { get; set; } = 1f;

        [UxmlAttribute("max-y")]
        public float MaxY { get; set; } = 1f;

        public Vector2 MinValue { get; private set; }
        public Vector2 MaxValue { get; private set; }

        private VisualElement dragElement;
        private VisualElement dragBorderElement;
        private DragManipulator dragger;
        private Vector2 dragElementStartPos;

        private const string stylesResource = "ColorPicker/Styles/Slider2DStyleSheet";
        private const string ussFieldName = "slider-2d";
        private const string ussDragger = ussFieldName + "__dragger";
        private const string ussDraggerBorder = ussFieldName + "__dragger-border";

        // ------------------------------------------------------------------------------------------------------------

        public Slider2D() : base(null, null)
        {
            InitVisuals();

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                MinValue = new Vector2(MinX, MinY);
                MaxValue = new Vector2(MaxX, MaxY);
                UpdateDraggerPosition();
            });
        }

        private void InitVisuals()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(stylesResource));
            AddToClassList(ussFieldName);

            dragBorderElement = new VisualElement { name = "dragger-border", pickingMode = PickingMode.Ignore };
            dragBorderElement.AddToClassList(ussDraggerBorder);
            Add(dragBorderElement);

            dragElement = new VisualElement { name = "dragger", pickingMode = PickingMode.Ignore };
            dragElement.AddToClassList(ussDragger);
            dragElement.RegisterCallback<GeometryChangedEvent>(UpdateDraggerPosition);
            Add(dragElement);

            dragger = new DragManipulator(OnDraggerClicked, OnDraggerDragged);
            pickingMode = PickingMode.Position;

            this.AddManipulator(dragger);
        }

        public override void SetValueWithoutNotify(Vector2 newValue)
        {
            var clampedValue = GetClampedValue(newValue);
            base.SetValueWithoutNotify(clampedValue);
            UpdateDraggerPosition();
        }

        private void UpdateDraggerPosition(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size) return;
            UpdateDraggerPosition();
        }

        private void UpdateDraggerPosition()
        {
            if (panel == null) return;

            var normalizedVal = NormalizeValue();
            float newLeft = normalizedVal.x * resolvedStyle.width;
            float newTop = (1f - normalizedVal.y) * resolvedStyle.height;
            if (float.IsNaN(newLeft) || float.IsNaN(newTop)) return;

            var currTop = dragElement.resolvedStyle.translate.y;
            var currLeft = dragElement.resolvedStyle.translate.x;

            if (!Similar(currLeft, newLeft) || !Similar(currTop, newTop))
            {
                var pos = new Vector3(
                    newLeft - dragElement.resolvedStyle.width * 0.5f,
                    newTop - dragElement.resolvedStyle.height * 0.5f,
                    0f
                );

                dragElement.style.translate = pos;
                dragBorderElement.style.translate = pos;
            }
        }

        private Vector2 NormalizeValue()
        {
            return (value - MinValue) / (MaxValue - MinValue);
        }

        private bool Similar(float a, float b)
        {
            return Mathf.Abs(b - a) < 1f;
        }

        private Vector2 GetClampedValue(Vector2 newValue)
        {
            Vector2 low = MinValue;
            Vector2 high = MaxValue;

            if (low.x > high.x) (low.x, high.x) = (high.x, low.x);
            if (low.y > high.y) (low.y, high.y) = (high.y, low.y);

            return new Vector2(
                Mathf.Clamp(newValue.x, low.x, high.x),
                Mathf.Clamp(newValue.y, low.y, high.y)
            );
        }

        private void OnDraggerClicked()
        {
            if (dragger.FreeMoving) return;

            var x = dragger.StartMousePosition.x - dragElement.resolvedStyle.width * 0.5f;
            var y = dragger.StartMousePosition.y - dragElement.resolvedStyle.height * 0.5f;
            dragElementStartPos = new Vector2(x, y);

            ComputerValueFrom(dragElementStartPos);
        }

        private void OnDraggerDragged()
        {
            if (dragger.FreeMoving)
            {
                ComputerValueFrom(dragElementStartPos + dragger.Delta);
            }
        }

        private void ComputerValueFrom(Vector2 pos)
        {
            float totalWidth = resolvedStyle.width - dragElement.resolvedStyle.width;
            float totalHeight = resolvedStyle.height - dragElement.resolvedStyle.height;

            if (Mathf.Abs(totalWidth) < Mathf.Epsilon || Mathf.Abs(totalHeight) < Mathf.Epsilon)
                return;

            float nPosX = Mathf.Clamp(pos.x, 0f, totalWidth) / totalWidth;
            float nPosY = 1f - Mathf.Clamp(pos.y, 0f, totalHeight) / totalHeight;

            value = new Vector2(
                Mathf.LerpUnclamped(MinValue.x, MaxValue.x, nPosX),
                Mathf.LerpUnclamped(MinValue.y, MaxValue.y, nPosY)
            );
        }
    }
}
