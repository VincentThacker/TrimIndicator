using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TrimIndicator
{
	class TrimLabel
	{
        readonly Vector3 _sliderDirection;
        readonly float _sliderAmplitude;
        readonly GameObject _slider;
        readonly IEnumerable<TextMeshProUGUI> _textMeshes;

        float _lastValue;

        static TMP_FontAsset _font;
        static TMP_FontAsset Font => _font ??= Resources.LoadAll<TMP_FontAsset>("Fonts").FirstOrDefault(f => f.name == "Calibri SDF");

        static Material _fontMaterial;
        static Material FontMaterial => _fontMaterial ??= Resources.LoadAll<Material>("Fonts").FirstOrDefault(f => f.name == "Calibri SDF Material");

        static readonly Color BackgroundColor = new Color(0.235f, 0.274f, 0.310f); // #3C464F
        static readonly Color ForegroundColor = new Color(0.827f, 0.827f, 0.827f); // #D3D3D3
        const float FontSize = 17f;
        const float OutlineWidth = 1.5f;
        const float HorizontalSliderAmplitude = 39f;
        const float VerticalSliderAmplitude = 49f;
        const double SliderPositionSteepness = 20;

        public TrimLabel(GameObject parent, Vector3 location, bool isVertical)
		{
			_sliderDirection = isVertical ? Vector3.up : Vector3.right;
			_sliderAmplitude = isVertical ? VerticalSliderAmplitude : HorizontalSliderAmplitude;
			var textAlignment = isVertical ? TextAlignmentOptions.BaselineLeft : TextAlignmentOptions.Baseline;

			_slider = MakeDummyObject(MakeDummyObject(parent, location));

			_textMeshes = new[]
			{
				MakeTextMesh(_slider, -OutlineWidth * _sliderDirection, BackgroundColor, textAlignment),
				MakeTextMesh(_slider, +OutlineWidth * _sliderDirection, BackgroundColor, textAlignment),
				MakeTextMesh(_slider, Vector3.zero, ForegroundColor, textAlignment),
			};
		}

		public void SetValue(float value)
		{
			if(value != _lastValue)
			{
				float steps = value * 100;
				string text = steps != 0 ? (steps < 0 ? "−" : "+") + Math.Abs(steps).ToString("##0.00", CultureInfo.InvariantCulture) : string.Empty;

				_slider.transform.localPosition = _sliderAmplitude * GetSliderPosition(value) * _sliderDirection;

				foreach (var textMesh in _textMeshes)
				{
					textMesh.text = text;
				}
				_lastValue = value;
			}
		}

		static GameObject MakeObject(GameObject parent, Vector3 relativeLocation)
		{
			var gameObject = new GameObject { layer = LayerMask.NameToLayer("UI") };
			gameObject.transform.SetParent(parent.transform, false);
			gameObject.transform.localPosition = relativeLocation;
			return gameObject;
		}

		static GameObject MakeDummyObject(GameObject parent, Vector3 relativeLocation = default(Vector3))
		{
			var gameObject = MakeObject(parent, relativeLocation);
			// Just an empty text mesh. Consider replacing by something more adequate
			var textMesh = gameObject.AddComponent<TextMeshProUGUI>();
			textMesh.autoSizeTextContainer = true;
			return gameObject;
		}

		static TextMeshProUGUI MakeTextMesh(GameObject parent, Vector3 relativeLocation, Color color, TextAlignmentOptions alignment)
		{
			var gameObject = MakeObject(parent, relativeLocation);

			var textMesh = gameObject.AddComponent<TextMeshProUGUI>();
			textMesh.autoSizeTextContainer = true;
			textMesh.isOverlay = true;
			textMesh.enableWordWrapping = false;
			textMesh.alignment = alignment;
			textMesh.font = Font;
			textMesh.fontMaterial = FontMaterial;
			textMesh.fontSize = FontSize;
			textMesh.color = color;

			return textMesh;
		}

		static float GetSliderPosition(float value)
		{ 
			return (float)(Math.Atan(value * SliderPositionSteepness) / Math.Atan(SliderPositionSteepness));
		}
	}
}