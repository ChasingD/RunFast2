#if UIWIDGETS_DATABIND_SUPPORT && UNITY_EDITOR
namespace UIWidgets.DataBindSupport
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UIWidgets;
	using UIWidgets.Extensions;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// DataBind scripts generator.
	/// </summary>
	public class DataBindGenerator : IFormattable
	{
		/// <summary>
		/// Script data.
		/// </summary>
		public class ScriptData
		{
			/// <summary>
			/// Path.
			/// </summary>
			public readonly string Path;

			/// <summary>
			/// Content.
			/// </summary>
			public readonly string Content;

			/// <summary>
			/// File exists.
			/// </summary>
			public readonly bool Exists;

			/// <summary>
			/// Allow to overwrite file.
			/// </summary>
			public bool Overwrite;

			/// <summary>
			/// Need save file.
			/// </summary>
			public bool NeedSave => Changed && (!Exists || Overwrite);

			/// <summary>
			/// File data changed or file not exists.
			/// </summary>
			public readonly bool Changed;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="path">Path.</param>
			/// <param name="template">Template.</param>
			/// <param name="gen">Generator.</param>
			public ScriptData(string path, string content)
			{
				Path = path;
				Exists = File.Exists(path);
				Content = content;
				Overwrite = false;
				Changed = !Exists || (File.ReadAllText(Path) != Content);
			}

			/// <summary>
			/// Save.
			/// </summary>
			public void Save()
			{
				if (NeedSave)
				{
					File.WriteAllText(Path, Content);
				}
			}
		}

		/// <summary>
		/// Data.
		/// </summary>
		protected DataBindOption Option;

		/// <summary>
		/// Path to save generated files.
		/// </summary>
		protected string SavePath;

		/// <summary>
		/// Events.
		/// </summary>
		protected List<DataBindEvents> Events = new List<DataBindEvents>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DataBindGenerator"/> class.
		/// </summary>
		/// <param name="option">Data.</param>
		/// <param name="path">Path to save files.</param>
		protected DataBindGenerator(DataBindOption option, string path)
		{
			Option = option;
			SavePath = path;

			foreach (var ev in Option.Events.Keys)
			{
				Events.Add(new DataBindEvents()
				{
					ClassName = Option.ShortClassName,
					EventName = ev,
					Arguments = Option.Events[ev],
				});
			}
		}

		/// <summary>
		/// Create support scripts for the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="path">Path to save scripts.</param>
		public static void Run(Type type, string path)
		{
			var options = DataBindOption.GetOptions(type);

			foreach (var option in options)
			{
				var gen = new DataBindGenerator(option, path);
				gen.Generate();
			}
		}

		/// <summary>
		/// Check is Data Bind support can be added to the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>true if support can be added; otherwise false.</returns>
		public static bool IsValidType(Type type)
		{
			var options = DataBindOption.GetOptions(type);

			return options.Count > 0;
		}

		/// <summary>
		/// Get scripts.
		/// </summary>
		/// <returns>Scripts.</returns>
		protected List<ScriptData> GetScripts()
		{
			var templates = DataBindTemplates.Instance;

			var scripts = new List<ScriptData>();
			if (Option.CanWrite)
			{
				scripts.Add(new ScriptData(GetFilePath("Setter"), string.Format(templates.Setter.text, this)));
			}

			if (Option.Events.Count > 0)
			{
				scripts.Add(new ScriptData(GetFilePath("Provider"), string.Format(templates.Provider.text, this)));
				scripts.Add(new ScriptData(GetFilePath("Observer"), string.Format(templates.Observer.text, this)));
			}

			if (Option.CanWrite && (Option.Events.Count > 0))
			{
				scripts.Add(new ScriptData(GetFilePath("Synchronizer"), string.Format(templates.Synchronizer.text, this)));
			}

			return scripts;
		}

		/// <summary>
		/// Generate scripts files.
		/// </summary>
		public virtual void Generate()
		{
			var scripts = GetScripts();
			foreach (var script in scripts)
			{
				if (!script.Changed)
				{
					continue;
				}

				// TODO replace with single dialog to confirm
				script.Overwrite = EditorUtility.DisplayDialog(
					"Overwrite existing file?",
					"Overwrite existing file " + script.Path + "?",
					"Overwrite",
					"Skip");
			}

			foreach (var script in scripts)
			{
				script.Save();
			}

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Get class name.
		/// </summary>
		/// <param name="type">Class type.</param>
		/// <returns>Class name.</returns>
		protected virtual string GetClassName(string type)
		{
			return Option.ShortClassName + Option.FieldName + type;
		}

		/// <summary>
		/// Get file path.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>File path.</returns>
		protected virtual string GetFilePath(string type)
		{
			return SavePath + Path.DirectorySeparatorChar + GetClassName(type) + ".cs";
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="formatProvider">Format provider.</param>
		/// <returns>String.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			switch (format)
			{
				case "Namespace":
					return string.IsNullOrEmpty(Option.Namespace) ? "DataBind" : Option.Namespace + ".DataBind";
				case "TargetFullName":
					return Option.ClassName;
				case "TargetShortName":
					return Option.ShortClassName;
				case "FieldName":
					return Option.FieldName;
				case "FieldType":
					return Option.FieldType;
				default:
					var pos = format.IndexOf("@");
					if (pos != -1)
					{
						var template = format.Substring(pos + 1).Replace("[", "{").Replace("]", "}");
						return Events.ToString(template, formatProvider);
					}
					else
					{
						throw new ArgumentOutOfRangeException("Unsupported format: " + format);
					}
			}
		}
	}
}
#endif