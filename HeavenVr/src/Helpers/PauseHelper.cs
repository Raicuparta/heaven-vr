namespace HeavenVr.Helpers;

public static class PauseHelper
{
    public static bool IsPaused()
    {
        return RM.mechController == null ||
               RM.mechController.deck == null ||
               !RM.mechController.Alive ||
               RM.time == null ||
               RM.time.GetIsTimeScaleZero() ||
               RM.time.GetIsPaused() ||
               RM.drifter == null ||
               !RM.drifter.enabled;
    }
}