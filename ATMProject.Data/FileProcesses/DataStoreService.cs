namespace ATMProject.Data.FileProcesses;
public class DataStoreService<TModelType> : IDataStoreService<TModelType> where TModelType : class
{
    private readonly HashSet<TModelType> _models;
    public DataStoreService(HashSet<TModelType> models)
    {
        _models = models;
    }
    public HashSet<TModelType> GetModels()
    {
        return _models;
    }
    public void AddItem(TModelType model)
    {
        _models.Add(model);
    }
    public void RemoveItem(TModelType model)
    {
        _models.Remove(model);
    }
};
