namespace SLC.SpaceHorror.Core
{
    public interface IInteractable
    {
        bool IsInteractable { get; }

        void OnInteract();
    }
}
