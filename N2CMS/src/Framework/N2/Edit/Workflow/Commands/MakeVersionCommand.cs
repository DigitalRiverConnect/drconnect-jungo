using N2.Edit.Versioning;

namespace N2.Edit.Workflow.Commands
{
    public class MakeVersionCommand : CommandBase<CommandContext>
    {
        IVersionManager versionMaker;

        public MakeVersionCommand(IVersionManager versionMaker)
        {
            this.versionMaker = versionMaker;
        }
        
        public override void Process(CommandContext state)
        {
            if (versionMaker.IsVersionable(state.Content) && (state.Content.State == ContentState.Published || state.Content.State == ContentState.Unpublished))
            {
                versionMaker.AddVersion(state.Content, asPreviousVersion: true);
            }
        }
    }
}
