using System;
using UnityEngine;

namespace TrimIndicator
{
	[KSPAddon(KSPAddon.Startup.Flight, once: false)]
	public class TrimIndicatorAddon : MonoBehaviour
	{
        bool _showWheelTrim;

        TrimLabel _pitchTrimLabel;
        TrimLabel _yawTrimLabel;
        TrimLabel _rollTrimLabel;
        TrimLabel _wheelThrottleTrimLabel;
        TrimLabel _wheelSteerTrimLabel;

		PopupDialog trimControlDialog = null;
		Rect trimControlDialogRect;

        static readonly string SettingsFilePath = $"{KSPUtil.ApplicationRootPath}GameData/{nameof(TrimIndicator)}/{nameof(TrimIndicator)}.settings";

        void Start()
        {
            LoadSettings();

            var gaugesObject = FindObjectOfType<KSP.UI.Screens.Flight.LinearControlGauges>().gameObject;

            _pitchTrimLabel = new TrimLabel(gaugesObject, new Vector3(34f, -15.5f), isVertical: true);
            _yawTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -63.5f), isVertical: false);
            _rollTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -25f), isVertical: false);

            if (_showWheelTrim)
            {
                _wheelThrottleTrimLabel = new TrimLabel(gaugesObject, new Vector3(62f, -15.5f), isVertical: true);
                _wheelSteerTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -76f), isVertical: false);
            }
            trimControlDialog = OpenTrimControlDialog(new Rect(0.1f, 0.25f, 250f, 200f));

			/*
            Texture2D tex = Texture2D.whiteTexture;
            DefaultControls.Resources uiResources = new DefaultControls.Resources();
            uiResources.standard = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            GameObject uiButton = DefaultControls.CreateButton(uiResources);
            uiButton.transform.SetParent(gaugesObject.transform, false);
			uiButton.GetComponent<Button>().onClick.AddListener(() => { FlightInputHandler.state.pitchTrim += 0.2f; });
			*/
        }

        void Update()
        {
			var ctrlState = FlightInputHandler.state;

			_pitchTrimLabel?.SetValue(ctrlState?.pitchTrim ?? 0);
			_yawTrimLabel?.SetValue(ctrlState?.yawTrim ?? 0);
			_rollTrimLabel?.SetValue(ctrlState?.rollTrim ?? 0);
			_wheelThrottleTrimLabel?.SetValue(ctrlState?.wheelThrottleTrim ?? 0);
			_wheelSteerTrimLabel?.SetValue(-(ctrlState?.wheelSteerTrim ?? 0));

            if (Input.GetKeyDown(KeyCode.O))
            {
                trimControlDialog = OpenTrimControlDialog(trimControlDialogRect);
            }
        }

		void LoadSettings()
		{
			try
			{
				var settings = ConfigNode.Load(SettingsFilePath);
				settings.TryGetValue("ShowWheelTrim", ref _showWheelTrim);
			}
			catch(Exception exception)
			{
				print($"{nameof(TrimIndicator)}: Cannot load settings. {exception}");
			}
		}

		PopupDialog OpenTrimControlDialog(Rect position)
		{
			if (!trimControlDialog)
			{
				PopupDialog dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
					new MultiOptionDialog("trimControlDialog", "", "Trim", HighLogic.UISkin, position, new DialogGUIVerticalLayout(new DialogGUIBase[] {
						new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("--P", delegate { FlightInputHandler.state.pitchTrim -= 0.0005f; }, 50f, 50f, false), new DialogGUIButton("-P", delegate { FlightInputHandler.state.pitchTrim -= 0.0001f; }, 50f, 50f, false), new DialogGUIButton("+P", delegate { FlightInputHandler.state.pitchTrim += 0.0001f; }, 50f, 50f, false), new DialogGUIButton("++P", delegate { FlightInputHandler.state.pitchTrim += 0.0005f; }, 50f, 50f, false), new DialogGUIFlexibleSpace()),
						new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("--R", delegate { FlightInputHandler.state.rollTrim -= 0.0005f; }, 50f, 50f, false), new DialogGUIButton("-R", delegate { FlightInputHandler.state.rollTrim -= 0.0001f; }, 50f, 50f, false), new DialogGUIButton("+R", delegate { FlightInputHandler.state.rollTrim += 0.0001f; }, 50f, 50f, false), new DialogGUIButton("++R", delegate { FlightInputHandler.state.rollTrim += 0.0005f; }, 50f, 50f, false), new DialogGUIFlexibleSpace()),
						new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), new DialogGUIButton("--Y", delegate { FlightInputHandler.state.yawTrim -= 0.0005f; }, 50f, 50f, false), new DialogGUIButton("-Y", delegate { FlightInputHandler.state.yawTrim -= 0.0001f; }, 50f, 50f, false), new DialogGUIButton("+Y", delegate { FlightInputHandler.state.yawTrim += 0.0001f; }, 50f, 50f, false), new DialogGUIButton("++Y", delegate { FlightInputHandler.state.yawTrim += 0.0005f; }, 50f, 50f, false), new DialogGUIFlexibleSpace())
					})),
					false, HighLogic.UISkin, false
				);
				dialog.onDestroy.AddListener(() =>
				{
					Vector3 lastPosition = dialog.GetComponent<RectTransform>().position / GameSettings.UI_SCALE;
					trimControlDialogRect = new Rect(lastPosition.x / Screen.width + 0.5f, lastPosition.y / Screen.height + 0.5f, 250f, 200f);
				});
				return dialog;
			}
			return null;
        }
	}
}