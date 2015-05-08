using System;
using N2.Edit.Workflow.Commands;
using N2.Engine;
using N2.Persistence;
using N2.Security;
using System.Linq;
using N2.Edit.Versioning;

namespace N2.Edit.Workflow
{
    /// <summary>
    /// Provides commands used to change the state of content items.
    /// </summary>
    [Service(typeof(ICommandFactory))]
    public class CommandFactory : ICommandFactory
    {
		ISecurityManager security;
        ReplaceMasterCommand replaceMaster;
        MakeVersionCommand makeVersion;
        UseDraftCommand useDraftCmd;
        UpdateObjectCommand updateObject;
        DeleteCommand delete;
        UseMasterCommand useMaster;
        ValidateCommand validate;
        SaveCommand save;
        UpdateContentStateCommand draftState;
        UpdateContentStateCommand publishedState;
        ActiveContentSaveCommand saveActiveContent;
		MoveToPositionCommand moveToPosition;
		UpdateReferencesCommand updateReferences;
		SaveOnPageVersionCommand saveOnPageVersion;

        public CommandFactory(IPersister persister, ISecurityManager security, IVersionManager versionMaker, IEditUrlManager editUrlManager, IContentAdapterProvider adapters, StateChanger changer)
        {
            //this.persister = persister;
            //makeVersionOfMaster = On.Master(new MakeVersionCommand(versionMaker));
            //showEdit = new RedirectToEditCommand(editUrlManager);
            //clone = new CloneCommand();
            //unpublishedDate = new EnsureNotPublishedCommand(); // moved to StateChanger
            //ensurePublishedDate = new EnsurePublishedCommand();  // moved to StateChanger

            this.security = security;
            
            save = new SaveCommand(persister);
            delete = new DeleteCommand(persister.Repository);

            replaceMaster = new ReplaceMasterCommand(versionMaker);
            makeVersion = new MakeVersionCommand(versionMaker);
            useDraftCmd = new UseDraftCommand(versionMaker);
            saveOnPageVersion = new SaveOnPageVersionCommand(versionMaker);

            draftState = new UpdateContentStateCommand(changer, ContentState.Draft);
            publishedState = new UpdateContentStateCommand(changer, ContentState.Published);

            updateObject = new UpdateObjectCommand();
            useMaster = new UseMasterCommand();
            validate = new ValidateCommand();
            saveActiveContent = new ActiveContentSaveCommand();
			moveToPosition = new MoveToPositionCommand();
			updateReferences = new UpdateReferencesCommand();
        }

        /// <summary>Gets the command used to publish an item.</summary>
        /// <param name="context">The command context used to determine which command to return.</param>
        /// <returns>A command that when executed will publish an item.</returns>
		public virtual CompositeCommand GetPublishCommand(CommandContext context)
        {
			var item = context.Content;

            if (!item.IsPage)
                throw new ArgumentException("Publish requires item to be a page");

            if (item is IActiveContent)
				return Compose("Publish active content", Authorize(Permission.Publish), validate, updateObject, moveToPosition, saveActiveContent, updateReferences);
                
            // Editing
			if (!item.VersionOf.HasValue)
			{
                return Compose("Publish 1", Authorize(Permission.Publish), validate, 
                    (item.ID == 0) ? (CommandBase<CommandContext>) null : makeVersion, 
                    updateObject, // UPDATE
                    publishedState, moveToPosition, save, updateReferences);
			}

			// has been published before
			if (item.State == ContentState.Unpublished)
				 return Compose("Re-Publish", Authorize(Permission.Publish), validate, 
                     replaceMaster, useMaster, // REPLACE & USE
                     publishedState, moveToPosition, save, updateReferences);

			// has never been published before (remove old version)
			return Compose("Publish 2", Authorize(Permission.Publish), validate, 
                    updateObject, replaceMaster, delete, useMaster, // TODO check
                    publishedState, moveToPosition, save, updateReferences);
		}

        /// <summary>Gets the command to save changes to an item without leaving the editing interface.</summary>
		/// <param name="context">The command context used to determine which command to return.</param>
		/// <returns>A command that when executed will save an item.</returns>
		public virtual CompositeCommand GetSaveCommand(CommandContext context)
		{
			if (context.Interface != Interfaces.Editing)
				throw new NotSupportedException("Save is not supported while " + context.Interface);

			if (context.Content is IActiveContent) // handles it's own persistence
				return Compose("Save active content", Authorize(Permission.Write), validate, saveActiveContent);

            // TODO leave this decision to the command - base on page owning modified item
            // should not duplicate logic, tricky due to choice of save command below
            var useDraft = (!context.Content.IsPage  // parts are saved as a version to their page
                        || context.Content.State == ContentState.Published
                        || context.Content.State == ContentState.Unpublished);

            return Compose((useDraft) ? "Save draft" : "Save changes",
                Authorize(Permission.Write),
                validate,
                (useDraft) ? useDraftCmd : null,
                updateObject,
                draftState, /*unpublishedDate,*/
                (useDraft) ? (CommandBase<CommandContext>)saveOnPageVersion : save
            );       
		}

        /*private CommandBase<CommandContext> ReturnTo(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            return new RedirectCommand(url);
        }*/

        private CommandBase<CommandContext> Authorize(Permission permission)
        {
            return new AuthorizeCommand(security, permission);
        }

		protected virtual CompositeCommand Compose(string title, params CommandBase<CommandContext>[] commands)
        {
			var args = new CommandCreatedEventArgs { Command = new CompositeCommand(title, commands.Where(c => c != null).ToArray()) };
			if (CreatedCommand != null)
				CreatedCommand.Invoke(this, args);
			return args.Command;
        }

		/// <summary>Invoked before returning a command to be executed.</summary>
		public event EventHandler<CommandCreatedEventArgs> CreatedCommand;
	}
   
}
