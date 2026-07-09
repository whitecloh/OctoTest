using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Ecs
{
    public struct UnitComponent
    {
        public int RuntimeId;
        public string DataId;
    }

    public struct UnitValueComponent
    {
        public int Value;
    }

    public struct UnitGridPositionComponent
    {
        public int CurrentPointIndex;
        public int ReservedPointIndex;
    }

    public struct UnitWorldPositionComponent
    {
        public Vector3 Position;
    }

    public struct UnitMovingComponent
    {
        public Vector3 TargetPosition;
    }
}
