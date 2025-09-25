using System;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace TrimIndicator
{
	[KSPAddon(KSPAddon.Startup.Flight, once: false)]

	public class TrimIndicator : MonoBehaviour
	{
        bool showWheelTrim;
        float startX = 25f;
        float startY = 25f;

        TrimLabel pitchTrimLabel;
        TrimLabel yawTrimLabel;
        TrimLabel rollTrimLabel;
        TrimLabel wheelThrottleTrimLabel;
        TrimLabel wheelSteerTrimLabel;

        bool controlActive = false;
        bool sig = false;
        float fixDT = 0f;
        Quaternion surfaceRotation1;
        Quaternion surfaceRotation2;
        Rect trimControlDialogRect = new Rect();

        float sliderPosition1 = -4f;
        float sliderPosition2 = -5f;
        float Kp = 0.0001f;
        float Ki = 0.00001f;

        int i = 0;

        static readonly string SettingsFilePath = $"{KSPUtil.ApplicationRootPath}GameData/{nameof(TrimIndicator)}/{nameof(TrimIndicator)}.settings";

        void Start()
        {
            LoadSettings();

            trimControlDialogRect.x = startX;
            trimControlDialogRect.y = startY;

            var gaugesObject = FindObjectOfType<KSP.UI.Screens.Flight.LinearControlGauges>().gameObject;

            pitchTrimLabel = new TrimLabel(gaugesObject, new Vector3(34f, -15.5f), isVertical: true);
            yawTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -63.5f), isVertical: false);
            rollTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -25f), isVertical: false);

            if (showWheelTrim)
            {
                wheelThrottleTrimLabel = new TrimLabel(gaugesObject, new Vector3(62f, -15.5f), isVertical: true);
                wheelSteerTrimLabel = new TrimLabel(gaugesObject, new Vector3(-24f, -76f), isVertical: false);
            }
        }

        void Update()
        {
			var ctrlState = FlightInputHandler.state;

			pitchTrimLabel?.SetValue(ctrlState?.pitchTrim ?? 0);
			yawTrimLabel?.SetValue(ctrlState?.yawTrim ?? 0);
			rollTrimLabel?.SetValue(ctrlState?.rollTrim ?? 0);
			wheelThrottleTrimLabel?.SetValue(ctrlState?.wheelThrottleTrim ?? 0);
			wheelSteerTrimLabel?.SetValue(-(ctrlState?.wheelSteerTrim ?? 0));
        }

        void FixedUpdate()
        {
            if (sig)
            {
                GetValues();
            }
        }

		void OnGUI()
		{
			DrawTrimControlDialog();
		}

		void LoadSettings()
		{
			try
			{
				var settings = ConfigNode.Load(SettingsFilePath);
				settings.TryGetValue("ShowWheelTrim", ref showWheelTrim);
                settings.TryGetValue("DefaultWindowX", ref startX);
                settings.TryGetValue("DefaultWindowY", ref startY);
            }
			catch (Exception exception)
			{
                print($"{nameof(TrimIndicator)}: Cannot load settings: {exception}");
			}
		}

		void DrawTrimControlDialog()
		{
            if (PauseMenu.isOpen || FlightDriver.Pause)
            {
                return;
            }
            trimControlDialogRect = GUILayout.Window(23475, trimControlDialogRect, TrimControlDialog, "Control");
		}

		void TrimControlDialog(int id)
		{
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("--P", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.pitchTrim -= 0.0005f;
                    }
                    if (GUILayout.Button("-P", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.pitchTrim -= 0.0001f;
                    }
                    if (GUILayout.Button("+P", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.pitchTrim += 0.0001f;
                    }
                    if (GUILayout.Button("++P", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.pitchTrim += 0.0005f;
                    }
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("--R", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.rollTrim -= 0.0005f;
                    }
                    if (GUILayout.Button("-R", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.rollTrim -= 0.0001f;
                    }
                    if (GUILayout.Button("+R", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.rollTrim += 0.0001f;
                    }
                    if (GUILayout.Button("++R", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.rollTrim += 0.0005f;
                    }
                    GUILayout.FlexibleSpace();
                }
                /*
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("--Y", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.yawTrim -= 0.0005f;
                    }
                    if (GUILayout.Button("-Y", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.yawTrim -= 0.0001f;
                    }
                    if (GUILayout.Button("+Y", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.yawTrim += 0.0001f;
                    }
                    if (GUILayout.Button("++Y", GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        FlightInputHandler.state.yawTrim += 0.0005f;
                    }
                    GUILayout.FlexibleSpace();
                }
                */
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Start Auto"))
                    {
                        if (!controlActive)
                        {
                            controlActive = true;
                            new Thread(new ThreadStart(ControllerBackground)).Start();
                        }
                    }
                    if (GUILayout.Button("Stop Auto"))
                    {
                        controlActive = false;
                    }
                    if (GUILayout.Button("Test"))
                    {
                        Debug.Log(sliderPosition1);
                    }
                    GUILayout.FlexibleSpace();
                }
                if (controlActive)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        sliderPosition1 = GUILayout.HorizontalSlider(sliderPosition1, -5, -1, options: GUILayout.Width(250f));
                        Kp = (float)Math.Pow(10, sliderPosition1);
                        GUILayout.Label(Kp.ToString("0.000E+0", CultureInfo.InvariantCulture));
                        GUILayout.FlexibleSpace();
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        sliderPosition2 = GUILayout.HorizontalSlider(sliderPosition2, -5, -1, options: GUILayout.Width(250f));
                        Ki = (float)Math.Pow(10, sliderPosition2);
                        GUILayout.Label(Ki.ToString("0.000E+0", CultureInfo.InvariantCulture));
                        GUILayout.FlexibleSpace();
                    }
                }
            }
            GUI.DragWindow();
        }

        Quaternion GetSurfaceRotation()
        {
            // This code was derived from MechJeb2's implementation for getting the vessel's surface relative rotation.
            Vessel vessel = FlightGlobals.ActiveVessel;
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 north = Vector3.ProjectOnPlane(vessel.mainBody.position + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius - vessel.CoMD, up).normalized;

            return Quaternion.Inverse(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(vessel.transform.rotation) * Quaternion.LookRotation(north, up));
        }

        void ControllerBackground()
        {
            
            float roll1;
            float roll2;
            float rollRate;
            while (controlActive)
            {
                if (!PauseMenu.isOpen && !FlightDriver.Pause && FlightGlobals.ActiveVessel.situation == Vessel.Situations.FLYING)
                {
                    sig = true;
                    SpinWait.SpinUntil(() => { return i == 2; });
                    i = 0;
                    roll1 = surfaceRotation1.eulerAngles.z > 180.0f ? 360f - surfaceRotation1.eulerAngles.z : -surfaceRotation1.eulerAngles.z;
                    roll2 = surfaceRotation2.eulerAngles.z > 180.0f ? 360f - surfaceRotation2.eulerAngles.z : -surfaceRotation2.eulerAngles.z;
                    rollRate = (roll2 - roll1) / fixDT;
                    // Debug.Log(roll1);
                    // Debug.Log(roll2);
                    // Debug.Log(fixDT);
                    FlightInputHandler.state.rollTrim -= (roll2 * Ki + rollRate * Kp);
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        void GetValues()
        {
            if (i == 0)
            {
                surfaceRotation1 = GetSurfaceRotation();
                i++;
            }
            else if (i == 1)
            {
                surfaceRotation2 = GetSurfaceRotation();
                fixDT = TimeWarp.fixedDeltaTime;
                i++;
                sig = false;
            }
        }
    }
}