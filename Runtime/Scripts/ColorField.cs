using UnityEngine;
using UnityEngine.UIElements;

namespace Ferreira.ColorPicker
{
    [UxmlElement]
    public partial class ColorField : VisualElement
    {
        [UxmlAttribute]
        public Color Color
        {
            get => color; 
            set
            {
                color = value;
                SetColor(value);
            }
        }

        public ColorPopup Popup { get; set; }

        private Color color;

        private readonly VisualElement rgbField;
        private readonly VisualElement alphaField;

        private const string stylesResource = "ColorPicker/Styles/ColorFieldStyleSheet";
        private const string ussFieldInput = "color-field__input";
        private const string ussFieldInputRGB = "color-field__input-rgb";
        private const string ussFieldInputAlpha = "color-field__input-alpha";
        private const string ussFieldInputAlphaContainer = "color-field__input-alpha-container";

        public ColorField()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(stylesResource));

            AddToClassList(ussFieldInput);

            rgbField = new VisualElement();
            rgbField.AddToClassList(ussFieldInputRGB);
            rgbField.name = ussFieldInputRGB;
            Add(rgbField);

            var alphaFieldContainer = new VisualElement();
            alphaFieldContainer.AddToClassList(ussFieldInputAlphaContainer);
            alphaFieldContainer.name = ussFieldInputAlphaContainer;
            Add(alphaFieldContainer);

            alphaField = new VisualElement();
            alphaField.AddToClassList(ussFieldInputAlpha);
            alphaField.name = ussFieldInputAlpha;

            alphaFieldContainer.Add(alphaField);

            Color = Color.white;

            RegisterCallback<ClickEvent>(OnClick);
        }

        public void SetColor(Color color)
        {
            rgbField.style.backgroundColor = new Color(color.r, color.g, color.b, 1f);

            float w = alphaField.parent.resolvedStyle.width;
            alphaField.style.width = w * color.a;
        }

        private void OnClick(ClickEvent evt)
        {
            Popup?.Show(color);
        }
    }
}
