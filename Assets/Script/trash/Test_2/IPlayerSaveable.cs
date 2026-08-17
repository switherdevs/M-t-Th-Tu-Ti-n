namespace PersistenceSystem
{
    public interface IPlayerSaveable
    {
        void SaveToData(PlayerData data);
        void LoadFromData(PlayerData data);
    }
}