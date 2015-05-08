using System;
using System.Collections.Generic;
using System.Linq;
using N2.Definitions;

namespace N2.Integrity
{
	/// <summary>
	/// A class decoration used to restrict which items may be placed under 
	/// which page type. When this attribute intersects with 
	/// <see cref="AllowedChildrenAttribute"/>, the union of these two are 
	/// considered to be allowed.</summary>
	/// 
	/// by sweber 1-Mar-2013
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class RestrictPageTypesAttribute : TypeIntegrityAttribute, IInheritableDefinitionRefiner
	{
		/// <summary>
		/// Restrict children by template name, allow only children with these template name.
		/// </summary>
		public string[] TemplateKeys { get; set; }

		/// <summary>Initializes a new instance of the RestrictPageTypesAttribute which is used to restrict which types of items may be added below which.</summary>
		public RestrictPageTypesAttribute()
		{
			RefinementOrder = RefineOrder.Before;
		}

		/// <summary>Initializes a new instance of the RestrictPageTypesAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedTypes">Defines wether all types of items are allowed as parent items.</param>
		public RestrictPageTypesAttribute(AllowedTypes allowedTypes)
			: this()
		{
		    Types = allowedTypes == AllowedTypes.All ? null : new Type[0];
		}

	    /// <summary>Initializes a new instance of the RestrictPageTypesAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedParentTypes">A list of allowed types. Null is interpreted as all types are allowed.</param>
		public RestrictPageTypesAttribute(params Type[] allowedParentTypes)
			: this()
		{
			Types = allowedParentTypes; // TODO only pages
		}

		/// <summary>Initializes a new instance of the RestrictPageTypesAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedParentType">A list of allowed types. Null is interpreted as all types are allowed.</param>
		public RestrictPageTypesAttribute(Type allowedParentType)
			: this()
		{
			Types = new [] { allowedParentType };
		}

		/// <summary>Changes allowed parents on the item definition.</summary>
		/// <param name="currentDefinition">The definition to alter.</param>
		/// <param name="allDefinitions">All definitions.</param>
		public override void Refine(ItemDefinition currentDefinition, IList<ItemDefinition> allDefinitions)
		{
			currentDefinition.AllowedParentFilters.Add(new Helper { ChildType = currentDefinition.ItemType, Attribute = this });
		}

		class Helper : IAllowedDefinitionFilter
		{
			public Type ChildType { get; set; }
			public RestrictPageTypesAttribute Attribute { get; set; }

			#region IAllowedDefinitionFilter Members

			public AllowedDefinitionResult IsAllowed(AllowedDefinitionQuery context)
			{
				// get page of context
				var parent = context.Parent;
				while (!parent.IsPage)
				{
					if (parent.Parent == null)
						return AllowedDefinitionResult.Deny;

					parent = parent.Parent;
				}
				var parentType = parent.GetType();

				if (ChildType.IsAssignableFrom(context.ChildDefinition.ItemType))
				{
					if (Attribute.Types != null && !Attribute.Types.Any(t => t.IsAssignableFrom(parentType)))
					{
						//Trace.TraceInformation(context.ChildDefinition.ItemType.Name + " denied on " + parentType.Name);
						return AllowedDefinitionResult.Deny;
					}
					//if (this.Attribute.TemplateKeys != null && !this.Attribute.TemplateKeys.Contains(context.ParentDefinition.TemplateKey))
					//	return AllowedDefinitionResult.Deny;
				}
				return AllowedDefinitionResult.DontCare;
			}

			#endregion
		}
	}
}