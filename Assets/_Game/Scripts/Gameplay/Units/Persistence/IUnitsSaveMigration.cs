using OctoGames.TestTask.Gameplay.Units.Data;

namespace OctoGames.TestTask.Gameplay.Units.Persistence
{
    public interface IUnitsSaveMigration
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        void Migrate(UnitsSaveData saveData, UnitCatalog unitCatalog);
    }
}
