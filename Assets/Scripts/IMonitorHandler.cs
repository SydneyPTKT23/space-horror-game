namespace SLC
{
    public interface IMonitorHandler
    {
        void EnterInteraction();
        void ExitInteraction();

        bool IsInButtonMode { get; }
    }

}