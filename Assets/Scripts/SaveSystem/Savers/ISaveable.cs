namespace Encore.Saves
{
    public interface ISaveable
    {
        public void Save();

        public void Load(GameManager.GameAssets gameAssets);
    }
}