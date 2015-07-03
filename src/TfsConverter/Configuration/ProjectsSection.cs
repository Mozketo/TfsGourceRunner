using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TfsGource.Configuration
{
	/// <summary>
	/// Usage: var section = WebConfigurationManager.GetSection(ActivityTypeSection.SectionName) as ActivityTypeSection;
	/// </summary>
	/// <see cref="http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx#elementcolls"/>
	public class ProjectsSection : ConfigurationSection
	{
		private static readonly ConfigurationProperty _propElement;

		#region Constructors
		/// <summary>
		/// Predefines the valid properties and prepares
		/// the property collection.
		/// </summary>
		static ProjectsSection()
		{
			_propElement = new ConfigurationProperty("projects", typeof(ProjectElementCollection), null, ConfigurationPropertyOptions.IsRequired);
		}
		#endregion

		#region Properties
		/// <summary>
		/// When asking WebConfigurationManager.GetSection(string) you can use this static property for the Section.
		/// </summary>
		public static string SectionName { get { return "projectsCollection"; } }

		/// <summary>
		/// Gets the NestedElement element.
		/// </summary>
		[ConfigurationProperty("projects")]
		public ProjectElementCollection Projects
		{
			get { return (ProjectElementCollection)base[_propElement]; }
		}

		// ...
		#endregion
	}

	[ConfigurationCollection(typeof(ProjectElement), AddItemName = "project", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class ProjectElementCollection : ConfigurationElementCollection
	{
		#region Constructors
		static ProjectElementCollection()
		{
			_properties = new ConfigurationPropertyCollection();
		}

		public ProjectElementCollection()
		{
		}
		#endregion

		#region Fields
		private static readonly ConfigurationPropertyCollection _properties;
		#endregion

		#region Properties
		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName
		{
			get { return "project"; }
		}
		#endregion

		#region Indexers
		public ProjectElement this[int index]
		{
			get { return (ProjectElement)base.BaseGet(index); }
			set
			{
				if (base.BaseGet(index) != null)
				{
					base.BaseRemoveAt(index);
				}
				base.BaseAdd(index, value);
			}
		}

		public ProjectElement this[string name]
		{
			get { return (ProjectElement)base.BaseGet(name); }
		}
		#endregion

		#region Overrides
		protected override ConfigurationElement CreateNewElement()
		{
			return new ProjectElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as ProjectElement).ProjectPaths;
		}
		#endregion
	}

	public class ProjectElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _propTitle;
		private static readonly ConfigurationProperty _propProjectPath;
		private static readonly ConfigurationProperty _propDays;
		private static readonly ConfigurationProperty _propEnabled;
		private static readonly ConfigurationPropertyCollection _properties;

		/// <summary>
		/// Predefines the valid properties and prepares
		/// the property collection.
		/// </summary>
		static ProjectElement()
		{
			// Predefine properties here
			_propTitle = new ConfigurationProperty("title", typeof(string), null, ConfigurationPropertyOptions.None);
			_propProjectPath = new ConfigurationProperty("projectPaths", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
			_propDays = new ConfigurationProperty("days", typeof(int), 0, ConfigurationPropertyOptions.IsRequired);
			_propEnabled = new ConfigurationProperty("enabled", typeof(bool), null, ConfigurationPropertyOptions.IsRequired);
			_properties = new ConfigurationPropertyCollection
			            {
			               	_propDays, _propProjectPath, _propEnabled, _propTitle
			            };
		}

		#region Properties
		/// <summary>
		/// Gets the Id setting.
		/// </summary>
		[ConfigurationProperty("days", IsRequired = true)]
		public int Days
		{
			get { return (int)base[_propDays]; }
			set { base[_propDays] = value; }
		}

		/// <summary>
		/// Gets the Identifier setting.
		/// </summary>
		[ConfigurationProperty("projectPaths", IsRequired = true)]
		[StringValidator(MinLength = 1, MaxLength = 256)]
		public string ProjectPaths
		{
			get { return (string)base[_propProjectPath]; }
			set { base[_propProjectPath] = value; }
		}

		[ConfigurationProperty("title")]
		[StringValidator(MinLength = 1, MaxLength = 256)]
		public string Title
		{
			get { return (string)base[_propTitle]; }
			set { base[_propTitle] = value; }
		}

		[ConfigurationProperty("enabled", IsRequired = true)]
		public bool Enabled
		{
			get { return (bool)base[_propEnabled]; }
			set { base[_propEnabled] = value; }
		}

		/// <summary>
		/// Override the Properties collection and return our custom one.
		/// </summary>
		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}
		#endregion
	}
}
