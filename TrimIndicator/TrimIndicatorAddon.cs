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

        static readonly string SettingsFilePath = $"{KSPUtil.ApplicationRootPath}GameData/{nameof(TrimIndicator)}/{nameof(TrimIndicator)}.settings";

        void Start()
		{
            LoadSettings();

			var gaugesObject = FindObjectOfType<KSP.UI.Screens.Flight.LinearControlGauges>().gameObject;

			_pitchTrimLabel = new TrimLabel(gaugesObject, new Vector3(34f, -15.5f), isVertical: true);
			_yawTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -63.5f), isVertical: false);
			_rollTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -25f), isVertical: false);

			if(_showWheelTrim)
			{
				_wheelThrottleTrimLabel = new TrimLabel(gaugesObject, new Vector3(62f, -15.5f), isVertical: true);
				_wheelSteerTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -76f), isVertical: false);
			}

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("trimControlDialog", "", "Trim", HighLogic.UISkin, new Rect(0.25f, 0.25f, 250f, 200f), new DialogGUIVerticalLayout(new DialogGUIBase[] {
                    new DialogGUIHorizontalLayout(new DialogGUIButton("--P", delegate { FlightInputHandler.state.pitchTrim -= 0.0005f; }, 50f, 50f, false), new DialogGUIButton("-P", delegate { FlightInputHandler.state.pitchTrim -= 0.0001f; }, 50f, 50f, false), new DialogGUIButton("+P", delegate { FlightInputHandler.state.pitchTrim += 0.0001f; }, 50f, 50f, false), new DialogGUIButton("++P", delegate { FlightInputHandler.state.pitchTrim += 0.0005f; }, 50f, 50f, false)),
                    new DialogGUIHorizontalLayout(new DialogGUIButton("--R", delegate { FlightInputHandler.state.rollTrim -= 0.0005f; }, 50f, 50f, false), new DialogGUIButton("-R", delegate { FlightInputHandler.state.rollTrim -= 0.0001f; }, 50f, 50f, false), new DialogGUIButton("+R", delegate { FlightInputHandler.state.rollTrim += 0.0001f; }, 50f, 50f, false), new DialogGUIButton("++R", delegate { FlightInputHandler.state.rollTrim += 0.0005f; }, 50f, 50f, false))
				})),
                false, HighLogic.UISkin
            );
        }

        void Update()
        {
			var ctrlState = FlightInputHandler.state;

			_pitchTrimLabel?.SetValue(ctrlState?.pitchTrim ?? 0);
			_yawTrimLabel?.SetValue(ctrlState?.yawTrim ?? 0);
			_rollTrimLabel?.SetValue(ctrlState?.rollTrim ?? 0);
			_wheelThrottleTrimLabel?.SetValue(ctrlState?.wheelThrottleTrim ?? 0);
			_wheelSteerTrimLabel?.SetValue(-(ctrlState?.wheelSteerTrim ?? 0));
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
	}
}