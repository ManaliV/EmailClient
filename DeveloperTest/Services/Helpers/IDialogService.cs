
namespace DeveloperTest.Services.Helpers
{
    public interface IDialogService
    {
        bool Confirm(string message, string title);

        void ShowMessage(string message, string title);

        void Warning(string message, string title);

    }
}
