namespace HeavenVr.Helpers;

public static class PauseHelper
{
    public static bool IsPaused()
    {
        return RM.mechController == null ||
               RM.mechController.deck == null ||
               RM.time == null ||
               RM.time.GetIsTimeScaleZero();
    }
}