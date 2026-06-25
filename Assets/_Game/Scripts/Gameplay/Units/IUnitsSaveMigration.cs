namespace OctoGames.TestTask.Gameplay.Units
{
    public interface IUnitsSaveMigration
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        void Migrate(UnitsSaveData saveData, UnitCatalog unitCatalog);
    }
}
