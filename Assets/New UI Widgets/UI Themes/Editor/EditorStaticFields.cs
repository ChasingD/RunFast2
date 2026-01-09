#if UNITY_EDITOR
namespace UIThemes.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// StaticFields for editor.
	/// </summary>
	public class EditorStaticFields
	{
		#region ScripingDefineSymbols

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

		#endregion

		/// <summary>
		/// ThemeTargetInspector: Properties cache.
		/// </summary>
		public readonly Dictionary<Type, Func<Target, bool>> ThemeTargetInspectorCache = new Dictionary<Type, Func<Target, bool>>();

		/// <summary>
		/// ThemeAttach: types cache.
		/// </summary>
		public readonly ThemeAttach.TypesCache ThemeAttachCache = new ThemeAttach.TypesCache();

		/// <summary>
		/// ThemeTargetInfo: cache.
		/// </summary>
		public readonly Dictionary<Type, ThemeTargetInfo> ThemeTargetInfoCache = new Dictionary<Type, ThemeTargetInfo>();

		/// <summary>
		/// ThemeInfo: cache.
		/// </summary>
		public readonly Dictionary<Type, ThemeInfo> ThemeInfoCache = new Dictionary<Type, ThemeInfo>();

		static EditorStaticFields instance;

		/// <summary>
		/// Instance.
		/// </summary>
		public static EditorStaticFields Instance => instance ??= new EditorStaticFields();

		EditorStaticFields()
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