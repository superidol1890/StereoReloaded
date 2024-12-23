namespace Stereo.Modules.State;

public class CollapsedState<TModule> : BaseModule
    where TModule : BaseModule
{
    public bool Collapsed { get; set; }
}
