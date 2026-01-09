namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Attributes;
	using UnityEngine;

	/// <summary>
	/// Window instances.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	public class WindowInstances<T>
		where T : MonoBehaviour, ITemplatable
	{
		Templates<T> templates;

		/// <summary>
		/// Templates.
		/// </summary>
		public Templates<T> Templates
		{
			get => templates ??= new Templates<T>();

			set => templates = value;
		}

		readonly HashSet<T> openedSet = new HashSet<T>();

		/// <summary>
		/// List of the opened instances.
		/// </summary>
		readonly List<T> temporary = new List<T>();

		/// <summary>
		/// Opened.
		/// </summary>
		public IReadOnlyList<T> Opened
		{
			get
			{
				temporary.Clear();
				temporary.AddRange(openedSet);

				return temporary;
			}
		}

		/// <summary>
		/// Count of opened instances.
		/// </summary>
		public int Count => openedSet.Count;

		/// <summary>
		/// All instances.
		/// </summary>
		public List<T> All
		{
			get
			{
				var all = Templates.GetAll();
				all.AddRange(Opened);

				return all;
			}
		}

		/// <summary>
		/// Event on custom instance opened.
		/// The parameter is opened instances count.
		/// </summary>
		public event Action<int> OnOpen;

		/// <summary>
		/// Event on custom instance closed.
		/// The parameter is opened instances count.
		/// </summary>
		public event Action<int> OnClose;

		/// <summary>
		/// Open instance.
		/// </summary>
		/// <param name="instance">Instance.</param>
		/// <param name="silent">Invoke event.</param>
		public void Add(T instance, bool silent = false)
		{
			openedSet.Add(instance);

			if (!silent)
			{
				OnOpen?.Invoke(openedSet.Count);
			}
		}

		/// <summary>
		/// Close instance.
		/// </summary>
		/// <param name="instance">Instance.</param>
		/// <param name="silent">Invoke event.</param>
		public void Remove(T instance, bool silent = false)
		{
			openedSet.Remove(instance);

			if (!silent)
			{
				OnClose?.Invoke(openedSet.Count);
			}
		}

		/// <summary>
		/// Get opened instances.
		/// </summary>
		/// <param name="output">Output list.</param>
		public void GetOpened(List<T> output) => output.AddRange(openedSet);

		WindowInstances()
		{
		}

		static WindowInstances<T> instance;

		/// <summary>
		/// Instance.
		/// </summary>
		public static WindowInstances<T> Instance => instance ??= new WindowInstances<T>();

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