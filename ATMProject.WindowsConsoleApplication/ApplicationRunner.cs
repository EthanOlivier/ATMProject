using ATMProject.Application;
using ATMProject.Application.Screens;
using ATMProject.Data.ModifyData;

namespace ATMProject.WindowsConsoleApplication
{
    internal class ApplicationRunner
    {
        private readonly IReadFile _readFile;
        private readonly IScreenManager _screenManager;

        public ApplicationRunner(IReadFile readFile, IScreenManager screenManager)
        {
            _readFile = readFile;
            _screenManager = screenManager;
        }

        public void RunApplication()
        {
            _readFile.ReadAllFilesContents();
            _screenManager.ShowScreen(ScreenNames.Login);
        }
    }
}
