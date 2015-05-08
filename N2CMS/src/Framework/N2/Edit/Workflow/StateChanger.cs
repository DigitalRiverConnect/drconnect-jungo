using System;
using N2.Engine;

namespace N2.Edit.Workflow
{
    /// <summary>
    /// Helps to change state of an item and notify observers about the change.
    /// </summary>
    [Service]
    public class StateChanger
    {
        /// <summary>Is invoked when the state has changed.</summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>Changes a state of an item to the new state.</summary>
        /// <param name="item">The item whose state is to be changed.</param>
        /// <param name="toState">The next state of the item.</param>
        public virtual void ChangeTo(ContentItem item, ContentState toState)
        {
            //if (item.State != toState)
            {
                var args = new StateChangedEventArgs(item, item.State);

                item.State = toState;
                switch (toState)
                {
                    case ContentState.Published:
                        // TODO check pre-condition: Published
                        item.Published = Utility.CurrentTime();
                        // TODO compare EnsurePublishedCommand handling of Publish Date in future

                        item.Expires = null;
                        break;

                    case ContentState.Unpublished:
                        // TODO check pre-condition: Published
                        item.Expires = Utility.CurrentTime().AddSeconds(-1);
                        break;

                    case ContentState.Draft:
                        item.Published = null; // was EnsureNotPublishedCommand
                        item.Expires = null;
                        break;
                }

                if (StateChanged != null)
                {
                    StateChanged(this, args);
                }
            }
        }
    }
}
