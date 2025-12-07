using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ferreira.ColorPicker
{
    [UxmlElement]
    public partial class ColorPopup : PopupPanel
    {
        [UxmlAttribute("heading")]
        public string Heading { get; set; } = "Pick Colour";

        [UxmlAttribute("button-label")]
        public string ButtonLabel { get; set; } = "Close";

        public Action<Color> OnSubmit;

        private float H, S, V, A;

        private readonly Label headingLabel;
        private readonly Button submitButton;

        private readonly Slider rSlider;
        private readonly Slider gSlider;
        private readonly Slider bSlider;
        private readonly Slider aSlider;
        private readonly Slider2D gradientSlider;
        private readonly Slider hueSlider;
        private readonly VisualElement gradientSliderDragger;
        private readonly VisualElement hueSliderDragger;

        private Texture2D gradientTexture;
        private Texture2D hueSliderTexture;

        private const string popupStylesResource = "ColorPicker/Styles/ColorPopupStyleSheet";
        private const string ussPopupClassName = "color-popup";
        private const string ussHeadingBack = ussPopupClassName + "__heading-area";
        private const string ussContentBack = ussPopupClassName + "__content-area";
        private const string ussHeading = ussPopupClassName + "__heading";
        private const string ussButtonsBar = ussPopupClassName + "__buttons-bar";
        private const string ussSubmitButton = ussPopupClassName + "__submit-button";
        private const string ussRSlider = ussPopupClassName + "__red-slider";
        private const string ussGSlider = ussPopupClassName + "__green-slider";
        private const string ussBSlider = ussPopupClassName + "__blue-slider";
        private const string ussASlider = ussPopupClassName + "__alpha-slider";
        private const string ussGradientArea = ussPopupClassName + "__gradient-area";
        private const string ussGradientSlider = ussPopupClassName + "__gradient-slider";
        private const string ussHueSlider = ussPopupClassName + "__hue-slider";

        public ColorPopup()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(popupStylesResource));
            AddToClassList(ussPopupClassName);

            //
            // HEADER
            //
            var head = new VisualElement() { name = ussHeadingBack };
            head.AddToClassList(ussHeadingBack);
            hierarchy.Add(head);

            headingLabel = new Label() { text = Heading, name = ussHeading };
            headingLabel.AddToClassList(ussHeading);
            head.Add(headingLabel);

            //
            // CONTENT
            //
            var content = new VisualElement() { name = ussContentBack };
            content.AddToClassList(ussContentBack);
            hierarchy.Add(content);

            //
            // GRADIENT AREA
            //
            var gradientArea = new VisualElement() { name = ussGradientArea };
            gradientArea.AddToClassList(ussGradientArea);
            content.Add(gradientArea);

            gradientSlider = new Slider2D() { name = ussGradientSlider };
            gradientSliderDragger = gradientSlider.Q("dragger");
            gradientSlider.AddToClassList(ussGradientSlider);
            gradientArea.Add(gradientSlider);

            hueSlider = new Slider(null, 0f, 360f, SliderDirection.Vertical, 0f) { name = ussHueSlider };
            hueSliderDragger = hueSlider.Q("unity-dragger");
            hueSlider.AddToClassList(ussHueSlider);
            gradientArea.Add(hueSlider);

            //
            // RGBA SLIDERS
            //
            rSlider = new Slider("Red", 0f, 1f) { name = ussRSlider };
            gSlider = new Slider("Green", 0f, 1f) { name = ussGSlider };
            bSlider = new Slider("Blue", 0f, 1f) { name = ussBSlider };
            aSlider = new Slider("Alpha", 0f, 1f) { name = ussASlider };

            rSlider.showInputField = true;
            gSlider.showInputField = true;
            bSlider.showInputField = true;
            aSlider.showInputField = true;

            rSlider.AddToClassList(ussRSlider);
            gSlider.AddToClassList(ussGSlider);
            bSlider.AddToClassList(ussBSlider);
            aSlider.AddToClassList(ussASlider);

            content.Add(rSlider);
            content.Add(gSlider);
            content.Add(bSlider);
            content.Add(aSlider);

            //
            // BUTTON BAR
            //
            var buttons = new VisualElement() { name = ussButtonsBar };
            buttons.AddToClassList(ussButtonsBar);
            hierarchy.Add(buttons);

            submitButton = new Button() { text = ButtonLabel, name = ussSubmitButton };
            submitButton.AddToClassList(ussSubmitButton);
            buttons.Add(submitButton);

            submitButton.clicked += OnSubmitButton;

            //
            // REGISTER CALLBACKS
            //
            rSlider.RegisterValueChangedCallback(ev => SetColorFromRSliders(ev.newValue));
            gSlider.RegisterValueChangedCallback(ev => SetColorFromGSliders(ev.newValue));
            bSlider.RegisterValueChangedCallback(ev => SetColorFromBSliders(ev.newValue));
            aSlider.RegisterValueChangedCallback(ev => A = ev.newValue);
            hueSlider.RegisterValueChangedCallback(SetColorFromHueSlider);
            gradientSlider.RegisterValueChangedCallback(SetColorFromGradientSlider);

            //
            // Atributos UXML → Aplicar após deserialização
            //
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                headingLabel.text = Heading;
                submitButton.text = ButtonLabel;
            });
        }

        public void Show(Color color)
        {
            Color.RGBToHSV(color, out H, out S, out V);
            A = color.a;

            headingLabel.text = Heading;
            submitButton.text = ButtonLabel;

            CreateTextures();
            OnColorChanged(true, true);
            aSlider.SetValueWithoutNotify(A);

            base.Show();
        }

        public override void Hide()
        {
            gradientSlider.style.backgroundImage = null;
            hueSlider.style.backgroundImage = null;

            UnityEngine.Object.Destroy(gradientTexture);
            UnityEngine.Object.Destroy(hueSliderTexture);

            OnSubmit = null;

            base.Hide();
        }

        private void OnSubmitButton()
        {
            var c = Color.HSVToRGB(H, S, V);
            c.a = A;

            OnSubmit?.Invoke(c);
            Hide();
        }

        private void SetColorFromGradientSlider(ChangeEvent<Vector2> ev)
        {
            S = ev.newValue.x;
            V = ev.newValue.y;
            OnColorChanged(false, false);
        }

        private void SetColorFromHueSlider(ChangeEvent<float> ev)
        {
            H = ev.newValue / 360f;
            OnColorChanged(false, true);
        }

        private void SetColorFromRSliders(float value)
        {
            var c = Color.HSVToRGB(H, S, V);
            c.r = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        private void SetColorFromGSliders(float value)
        {
            var c = Color.HSVToRGB(H, S, V);
            c.g = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        private void SetColorFromBSliders(float value)
        {
            var c = Color.HSVToRGB(H, S, V);
            c.b = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        private void OnColorChanged(bool updateHue, bool updateGradient)
        {
            var c = Color.HSVToRGB(H, S, V);

            hueSliderDragger.style.backgroundColor = Color.HSVToRGB(H, 1f, 1f);
            gradientSliderDragger.style.backgroundColor = c;

            rSlider.SetValueWithoutNotify(Round(c.r, 3));
            gSlider.SetValueWithoutNotify(Round(c.g, 3));
            bSlider.SetValueWithoutNotify(Round(c.b, 3));

            if (updateHue)
                hueSlider.SetValueWithoutNotify(H * 360f);

            if (updateGradient)
            {
                UpdateGradientTexture();
                gradientSlider.SetValueWithoutNotify(new Vector2(S, V));
            }
        }

        private void CreateTextures()
        {
            gradientTexture = new Texture2D(64, 64, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            gradientSlider.style.backgroundImage = gradientTexture;

            hueSliderTexture = new Texture2D(1, 64, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            hueSlider.style.backgroundImage = hueSliderTexture;

            UpdateHueSliderTexture();
        }

        private void UpdateHueSliderTexture()
        {
            if (hueSliderTexture == null) return;

            for (int i = 0; i < hueSliderTexture.height; i++)
                hueSliderTexture.SetPixel(0, i, Color.HSVToRGB((float)i / (hueSliderTexture.height - 1), 1f, 1f));

            hueSliderTexture.Apply();
            hueSlider.MarkDirtyRepaint();
        }

        private void UpdateGradientTexture()
        {
            if (gradientTexture == null) return;

            var pixels = new Color[gradientTexture.width * gradientTexture.height];

            for (int x = 0; x < gradientTexture.width; x++)
            {
                for (int y = 0; y < gradientTexture.height; y++)
                {
                    pixels[x * gradientTexture.width + y] =
                        Color.HSVToRGB(H, (float)y / gradientTexture.height, (float)x / gradientTexture.width);
                }
            }

            gradientTexture.SetPixels(pixels);
            gradientTexture.Apply();
            gradientSlider.MarkDirtyRepaint();
        }

        private static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10f, digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}
