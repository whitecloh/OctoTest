using System.Collections.Generic;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitsSaveMigrationPipeline
    {
        private readonly List<IUnitsSaveMigration> _migrations = new ();

        public void MigrateToCurrent(UnitsSaveData saveData, UnitCatalog unitCatalog)
        {
            if (saveData == null)
            {
                return;
            }

            int version = saveData.SaveVersion;
            while (version < UnitsSaveData_v_1_0.CurrentVersion)
            {
                IUnitsSaveMigration migration = FindMigration(version);
                if (migration == null)
                {
                    break;
                }

                migration.Migrate(saveData, unitCatalog);
                version = migration.TargetVersion;
                saveData.SaveVersion = version;
            }
        }

        private IUnitsSaveMigration FindMigration(int sourceVersion)
        {
            for (int i = 0; i < _migrations.Count; i++)
            {
                IUnitsSaveMigration migration = _migrations[i];
                if (migration.SourceVersion == sourceVersion)
                {
                    return migration;
                }
            }

            return null;
        }
    }
}
