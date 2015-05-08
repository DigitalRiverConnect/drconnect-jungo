using N2.Engine;
using NHibernate.Event;
using N2.Edit.Versioning;
using N2.Details;

namespace N2.Persistence.NH
{
	[Service(typeof(ConfigurationBuilderParticipator))]
	public class RelationConfigurationBuilderParticipator : ConfigurationBuilderParticipator
	{
		//class DetailRelationEventListener : IPreInsertEventListener, IPreUpdateEventListener
		//{
		//	public bool OnPreInsert(PreInsertEvent @event)
		//	{
		//		return UpdateRelations(@event.Entity as ContentDetail, @event.Session);
		//	}

		//	public bool OnPreUpdate(PreUpdateEvent @event)
		//	{
		//		return UpdateRelations(@event.Entity as ContentDetail, @event.Session);
		//	}

		//	private bool UpdateRelations(ContentDetail contentDetail, IEventSource eventSource)
		//	{
		//		if (contentDetail == null)
		//			return false;

		//		if (!contentDetail.LinkedItem.HasValue || contentDetail.ValueType != typeof(ContentItem))
		//			return false;
				
		//		contentDetail.StringValue = Utility.GetTrail(contentDetail.LinkedItem);
		//		return true;
		//	}
		//}


		class RelationInsertEventListener : IPostLoadEventListener
		{
			public void OnPostLoad(PostLoadEvent @event)
			{
                InitialializeRelations(@event.Entity as ContentItem, @event.Session);
				InitialializeRelations(@event.Entity as ContentVersion, @event.Session);
				InitialializeRelations(@event.Entity as ContentDetail, @event.Session);
			}

            private ContentItem GetByID(object o, IEventSource session)
            {
                int id = o is int ? (int) o : -1;
                return id == 0 ? null : session.Get<ContentItem>(o);
            }

		    private void InitialializeRelations(ContentDetail detail, IEventSource session)
			{
				if (detail == null)
					return;
			    detail.LinkedItem.ValueAccessor = o => GetByID(o, session);
			}

            private void InitialializeRelations(ContentVersion version, IEventSource session)
            {
                if (version == null)
                    return;
                version.Master.ValueAccessor = o => GetByID(o, session);
            }

			private void InitialializeRelations(ContentItem item, IEventSource session)
			{
				if (item == null)
					return;
                item.VersionOf.ValueAccessor = o => GetByID(o, session); 
			}
		}

		public override void AlterConfiguration(NHibernate.Cfg.Configuration cfg)
		{
			cfg.AppendListeners(NHibernate.Event.ListenerType.PostLoad, new[] { new RelationInsertEventListener() });
			//cfg.AppendListeners(NHibernate.Event.ListenerType.PreInsert, new[] { new DetailRelationEventListener() });
			//cfg.AppendListeners(NHibernate.Event.ListenerType.PreUpdate, new[] { new DetailRelationEventListener() });
		}
	}
}
