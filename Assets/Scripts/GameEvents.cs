using System;

public static class GameEvents
{
    public static event Action<float> OnNoiseChanged;
    public static void RaiseNoiseChanged(float normalized) => OnNoiseChanged?.Invoke(normalized);

    public static event Action OnLevelRestart;
    public static void RaiseLevelRestart() => OnLevelRestart?.Invoke();

    public static event Action OnGoalReached;
    public static void RaiseGoalReached() => OnGoalReached?.Invoke();
}
