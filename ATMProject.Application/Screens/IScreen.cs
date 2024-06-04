namespace ATMProject.Application.Screens;
public interface IScreen
{
    void ShowScreen();
}

public interface IReceivableScreen : IScreen
{
    void ReceiveData<T>(T data) where T : class;
}
