namespace ATMProject.Data.FileProcesses;
public interface IDataStoreService<TModelType>
{
    HashSet<TModelType> GetModels();
    void AddItem(TModelType model);
    void RemoveItem(TModelType model);
};
