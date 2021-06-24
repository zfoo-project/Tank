namespace MiniKing.Script.Module.Login.Service
{
    public interface ILoginService
    {
        void ConnectToGateway();

        void LoginByToken();

        void LoginByAccount();

        void Logout();

    }
}