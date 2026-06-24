namespace OctoGames.TestTask.Gameplay.Units
{
    public readonly struct UnitMovePlan
    {
        public UnitMovePlan(UnitRuntimeState unit, int targetPointIndex)
        {
            Unit = unit;
            TargetPointIndex = targetPointIndex;
        }

        public UnitRuntimeState Unit { get; }
        public int TargetPointIndex { get; }
    }
}
