#if UNITY_EDITOR
namespace UIThemes.Editor
{
	using System.Collections.Generic;
	using UnityEditor;

	/// <summary>
	/// Scripting Define Symbols.
	/// </summary>
	public static class ScriptingDefineSymbols
	{
		/// <summary>
		/// State of ScriptingDefineSymbol.
		/// </summary>
		public readonly struct State
		{
			/// <summary>
			/// Symbol.
			/// </summary>
			public readonly string Symbol;

			/// <summary>
			/// Count of build targets with ScriptingDefineSymbol.
			/// </summary>
			public readonly int Has;

			/// <summary>
			/// Total count of build targets with ScriptingDefineSymbol.
			/// </summary>
			public readonly int Total;

			/// <summary>
			/// Is any build target has ScriptingDefineSymbol.
			/// </summary>
			public readonly bool Any => Has > 0;

			/// <summary>
			/// All build targets has ScriptingDefineSymbol.
			/// </summary>
			public readonly bool All => Has >= Total;

			/// <summary>
			/// Some build targets does not have ScriptingDefineSymbol.
			/// </summary>
			public readonly bool HasMissing => Any && !All;

			/// <summary>
			/// Initializes a new instance of the <see cref="State"/> struct.
			/// </summary>
			/// <param name="symbol">Symbol.</param>
			/// <param name="has">Count of build targets with ScriptingDefineSymbol.</param>
			/// <param name="total">Total count of build targets with ScriptingDefineSymbol.</param>
			public State(string symbol, int has, int total)
			{
				Symbol = symbol;
				Has = has;
				Total = total;
			}

			/// <summary>
			/// Enable.
			/// </summary>
			public void Enable() => Add(Symbol);

			/// <summary>
			/// Disable.
			/// </summary>
			public void Disable() => Remove(Symbol);
		}

		static IReadOnlyList<BuildTargetGroup> Targets => EditorStaticFields.Instance.BuildTargets;

		/// <summary>
		/// Add scripting define symbols.
		/// </summary>
		/// <param name="symbol">Symbol to add.</param>
		public static void Add(string symbol)
		{
			foreach (var target in Targets)
			{
				var symbols = Symbols(target);

				if (symbols.Contains(symbol))
				{
					continue;
				}

				symbols.Add(symbol);

				Save(symbols, target);
			}
		}

		/// <summary>
		/// Remove scripting define symbols.
		/// </summary>
		/// <param name="symbol">Symbol to remove.</param>
		public static void Remove(string symbol)
		{
			foreach (var target in Targets)
			{
				var symbols = Symbols(target);

				if (!symbols.Contains(symbol))
				{
					continue;
				}

				symbols.Remove(symbol);

				Save(symbols, target);
			}
		}

		/// <summary>
		/// Rename.
		/// </summary>
		/// <param name="oldSymbol">Old symbol.</param>
		/// <param name="newSymbol">New symbol.</param>
		/// <returns>true if symbol exists and was renamed; otherwise false.</returns>
		public static bool Rename(string oldSymbol, string newSymbol)
		{
			if (!GetState(oldSymbol).Any)
			{
				return false;
			}

			Remove(oldSymbol);
			Add(newSymbol);
			AssetDatabase.Refresh();

			return true;
		}

		/// <summary>
		/// Get scripting define symbols.
		/// </summary>
		/// <param name="targetGroup">Target group.</param>
		/// <returns>Scripting define symbols.</returns>
		static HashSet<string> Symbols(BuildTargetGroup targetGroup)
		{
			return UtilitiesEditor.GetScriptingDefineSymbols(targetGroup);
		}

		/// <summary>
		/// Get symbol state.
		/// </summary>
		/// <param name="symbol">Scripting Define Symbol.</param>
		/// <returns>Symbol state.</returns>
		public static State GetState(string symbol)
		{
			var has = 0;
			foreach (var target in Targets)
			{
				var a = EditorUserBuildSettings.selectedBuildTargetGroup;
				if (Symbols(target).Contains(symbol))
				{
					has += 1;
				}
			}

			return new State(symbol, has, Targets.Count);
		}

		static void Save(HashSet<string> symbols, BuildTargetGroup target)
		{
			UtilitiesEditor.SetScriptingDefineSymbols(target, symbols);
			AssetDatabase.Refresh();
		}
	}
}
#endif