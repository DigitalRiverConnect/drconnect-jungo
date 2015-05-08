using System;
using N2.Definitions;
using N2.Edit;
using N2.Integrity;
using N2.Persistence;
using N2.Security;
using N2.Web;
using N2.Configuration;

namespace N2.Engine
{
	/// <summary>
	/// Classes implementing this interface can serve as a portal for the 
	/// various services composing the N2 engine. Edit functionality, modules
	/// and implementations access most N2 functionality through this 
	/// interface.
	/// </summary>
	public interface IEngine
	{
		/// <summary>Gets the persistence manager responsible of storing content items to persistence medium (database).</summary>
		IPersister Persister { get; }

		/// <summary>Gets the url parser responsible of mapping managementUrls to items and back again.</summary>
		IUrlParser UrlParser { get; }

		/// <summary>Gets the definition manager responsible of maintaining available item definitions.</summary>
		IDefinitionManager Definitions { get; }

		/// <summary>Gets the integrity manager used to control which items are allowed below which.</summary>
		IIntegrityManager IntegrityManager { get; }

		/// <summary>Gets the security manager responsible of controlling access to items.</summary>
		ISecurityManager SecurityManager { get; }

		/// <summary>Gets the class responsible for plugins in edit mode.</summary>
		IEditManager EditManager { get; }

		IEditUrlManager ManagementPaths { get; }

		/// <summary>Contextual data associated with the current request.</summary>
		IWebContext RequestContext { get; }

		/// <summary>The base of the web site.</summary>
		IHost Host { get; }

		/// <summary>The inversion of control container supporting this application.</summary>
		IServiceContainer Container { get; }

        /// <summary>The configuration used by N2 dring setup.</summary>
        ConfigurationManagerWrapper Config { get; }

		/// <summary>
		/// Initialize components and plugins in the N2 CMS environment.
		/// </summary>
		void Initialize();

		/// <summary>Resolves a service configured for the factory.</summary>
		/// <typeparam name="T">The type of service to resolve.</typeparam>
		/// <returns>An instance of the resolved service.</returns>
        //[Obsolete("use constructor injection or Engine.Container instead") ]
		T Resolve<T>() where T : class;

		/// <summary>Resolves a service configured for the factory.</summary>
		/// <param name="serviceType">The type of service to resolve.</param>
		/// <returns>An instance of the resolved service.</returns>
		//object Resolve(Type serviceType);

        /// <summary>Content helper used to find and manipulate data.</summary>
		ContentHelperBase Content { get; }
	}

	public enum ComponentLifeStyle
	{
		Singleton = 0,
		Transient = 1,
	}
}