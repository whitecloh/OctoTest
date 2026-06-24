namespace OctoGames.TestTask.UI.Popups
{
    public interface IPopupView<in TRequest>
    {
        bool Setup(TRequest request);
    }
}
