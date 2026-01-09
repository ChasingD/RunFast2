#if UNITY_EDITOR
namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Attributes;
	using UIWidgets.WidgetGeneration;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Editor static fields.
	/// Class to reduce amount of static fields, because of static fields have negative impact on domain reload time.
	/// </summary>
	public class EditorStaticFields
	{
		/// <summary>
		/// ListView: data types.
		/// </summary>
		public readonly Dictionary<Type, Type> ListView2DataType = new Dictionary<Type, Type>();

		/// <summary>
		/// Printable types.
		/// </summary>
		public readonly Type[] WidgetsGeneratorPrintableTypes =
		{
			typeof(Enum),
			typeof(string),
			typeof(decimal),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
		};

		/// <summary>
		/// Image types.
		/// </summary>
		public readonly Type[] WidgetsGeneratorImageTypes =
		{
			typeof(Sprite),
			typeof(Texture2D),
			typeof(Texture),
			typeof(Color),
			typeof(Color32),
		};

		/// <summary>
		/// AssemblyDefinitions cached GUIDs.
		/// </summary>
		public readonly Dictionary<string, string[]> AssemblyDefinitionsCachedGUIDs = new Dictionary<string, string[]>();

		/// <summary>
		/// Widgets Generator: wrapper types.
		/// </summary>
		public readonly Dictionary<Type, WrapperType> WidgetsGeneratorWrapperTypes = new Dictionary<Type, WrapperType>()
		{
			#if UIWIDGETS_R3_SUPPORT
			{ typeof(R3.SerializableReactiveProperty<>), new WrapperType(".Value", "new R3.SerializableReactiveProperty<{0}>", 0) },
			#endif
			#if UIWIDGETS_UNITY_LOCALIZATION_SUPPORT
			{ typeof(UnityEngine.Localization.LocalizedAsset<>), new WrapperType("?.LoadAsset()", null, 0) },
			#endif
		};

		/// <summary>
		/// Widgets Generator: Prohibited field names.
		/// </summary>
		public HashSet<string> WidgetsGeneratorProhibitedNames;

		/// <summary>
		/// ListView: Always allow to edit field.
		/// </summary>
		public Func<ListViewCustomBaseEditor, bool> ListViewAllowAlways = editor => true;

		List<BuildTargetGroup> buildTargets;

		/// <summary>
		/// BuildTargetGroups.
		/// </summary>
		public IReadOnlyList<BuildTargetGroup> BuildTargets
		{
			get
			{
				if (buildTargets != null)
				{
					return buildTargets;
				}

				buildTargets = new List<BuildTargetGroup>();
				foreach (var v in Enum.GetValues(typeof(BuildTarget)))
				{
					var target = (BuildTarget)v;
					var group = BuildPipeline.GetBuildTargetGroup(target);
					if (IsBuildTargetSupported(group, target) && !buildTargets.Contains(group))
					{
						buildTargets.Add(group);
					}
				}

				return buildTargets;
			}
		}

		static bool IsBuildTargetSupported(BuildTargetGroup group, BuildTarget target)
		{
#if UNITY_2018_1_OR_NEWER
			return BuildPipeline.IsBuildTargetSupported(group, target);
#else
			var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
			var buildTargetSupported = typeof(BuildPipeline).GetMethod("IsBuildTargetSupported", flags);
			return Convert.ToBoolean(buildTargetSupported.Invoke(null, new object[] { group, target }));
#endif
		}

		static EditorStaticFields instance;

		/// <summary>
		/// Instance.
		/// </summary>
		public static EditorStaticFields Instance => instance ??= new EditorStaticFields();

		private EditorStaticFields()
		{
		}

		#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		[DomainReload(nameof(instance))]
		static void StaticInit()
		{
			instance = null;
		}
		#endif
	}
}
#endif