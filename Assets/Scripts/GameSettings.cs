using UnityEngine;

public class GameSettings
{
    public static bool SphereTrail = true;

    public static float CameraSensitivity = 0.2f;

    public static float GlobalVolume = 0.5f;

    public static float MusicVolume = 0.3f;

    public static (int, int) GetScreenSize() =>
        (Screen.width, Screen.height);

    public static void SetScreenSize(int width, int height) =>
        Screen.SetResolution(width, height, Screen.fullScreen);
}

