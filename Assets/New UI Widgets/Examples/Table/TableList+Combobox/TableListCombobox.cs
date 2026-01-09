namespace UIWidgets.Examples.TableListDemo
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// TableListCombobox.
	/// </summary>
	public class TableListCombobox : ListViewCustom<TableListComboboxComponent, TableListComboboxItem>
	{
		[SerializeField]
		protected RectTransform ParentCanvas;

		[SerializeField]
		protected ListViewString ListView;

		RectTransform listViewRectTransform;

		HierarchyPosition listViewHierarchyPosition;

		Vector2 ListViewPosition;

		bool listViewActive;

		UIWidgets.InstanceID? modalKey;

		List<SelectListener> childrenDeselect = new List<SelectListener>();

		int currentDataIndex = -1;

		int currentValueIndex = -1;

		Button currentToggleButton;

		protected override void InitOnce()
		{
			base.InitOnce();

			if (ParentCanvas == null)
			{
				ParentCanvas = UtilitiesUI.FindTopmostCanvas(transform as RectTransform);
			}

			ListView.gameObject.SetActive(false);
			ListView.OnSelectObject.AddListener(UpdateValue);
			listViewRectTransform = ListView.transform as RectTransform;
			AddDeselectCallbacks();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			ListView.OnSelectObject.RemoveListener(UpdateValue);
			RemoveDeselectCallbacks();
		}

		void UpdateValue(int index)
		{
			if ((currentDataIndex == -1) || (currentValueIndex == -1))
			{
				return;
			}

			DataSource[currentDataIndex].Values[currentValueIndex] = ListView.DataSource[index];

			var instance = GetItemInstance(currentDataIndex);
			if (instance != null)
			{
				instance.UpdateView();
			}

			currentDataIndex = -1;
			currentValueIndex = -1;
			currentToggleButton = null;

			HideListList();
		}

		public void ShowListView(int dataIndex, int valueIndex, Button toggleButton)
		{
			currentDataIndex = dataIndex;
			currentValueIndex = valueIndex;
			currentToggleButton = toggleButton;
			var value = DataSource[dataIndex].Values[currentValueIndex];

			modalKey = ModalHelper.Open(this, null, new Color(0, 0, 0, 0f), HideListList, ParentCanvas);

			listViewRectTransform.SetParent(currentToggleButton.transform, worldPositionStays: false);
			listViewRectTransform.anchoredPosition = new Vector2(0, -GetDefaultItemHeight());
			ListView.Select(value, false);
			listViewHierarchyPosition = HierarchyPosition.SetParent(ListView.transform, ParentCanvas);

			ListView.gameObject.SetActive(true);
			ListView.Init();

			// prevent instant close of the ListView on first open, happens because of select/deselect events
			if (!ListView.gameObject.activeSelf)
			{
				ListView.gameObject.SetActive(true);
			}

			if (ListView.Layout != null)
			{
				ListView.Layout.UpdateLayout();
			}

			ListView.ScrollToPosition(ListView.GetScrollPosition());
			if (ListView.SelectComponent())
			{
				SetChildDeselectListener(EventSystem.current.currentSelectedGameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(ListView.gameObject);
			}

			listViewActive = true;
		}

		void SetChildDeselectListener(GameObject child)
		{
			var deselectListener = Utilities.RequireComponent<SelectListener>(child);
			if (!childrenDeselect.Contains(deselectListener))
			{
				deselectListener.onDeselect.AddListener(OnFocusHideList);
				childrenDeselect.Add(deselectListener);
			}
		}

		void AddDeselectCallbacks()
		{
			if (ListView.ScrollRect == null)
			{
				return;
			}

			if (ListView.ScrollRect.verticalScrollbar == null)
			{
				return;
			}

			var scrollbar = ListView.ScrollRect.verticalScrollbar.gameObject;
			var deselectListener = Utilities.RequireComponent<SelectListener>(scrollbar);

			deselectListener.onDeselect.AddListener(OnFocusHideList);
			childrenDeselect.Add(deselectListener);
		}

		void RemoveDeselectCallbacks()
		{
			foreach (var c in childrenDeselect)
			{
				RemoveDeselectCallback(c);
			}

			childrenDeselect.Clear();
		}

		void RemoveDeselectCallback(SelectListener listener)
		{
			if (listener != null)
			{
				listener.onDeselect.RemoveListener(OnFocusHideList);
			}
		}

		bool IsChildrenEvent(GameObject go)
		{
			if (go.transform.IsChildOf(ListView.transform))
			{
				return true;
			}

			return false;
		}

		void OnFocusHideList(BaseEventData eventData)
		{
			if ((eventData.selectedObject == gameObject) || IsChildrenEvent(eventData.selectedObject))
			{
				return;
			}

			if (eventData is ListViewItemEventData ev_item)
			{
				if (ev_item.NewSelectedObject != null)
				{
					SetChildDeselectListener(ev_item.NewSelectedObject);
				}

				return;
			}

			if ((eventData is AxisEventData) && ListView.Navigation)
			{
				return;
			}

			if (!(eventData is PointerEventData ev_pointer))
			{
				HideListList();
				return;
			}

			var go = ev_pointer.pointerPressRaycast.gameObject;
			if (go == null)
			{
				HideListList();
				return;
			}

			if (go.transform.IsChildOf(ListView.transform))
			{
				SetChildDeselectListener(go);
				return;
			}

			HideListList();
		}

		public virtual void HideListList()
		{
			if (!listViewActive)
			{
				return;
			}

			listViewActive = false;

			listViewRectTransform.anchoredPosition = ListViewPosition;
			ModalHelper.Close(ref modalKey);

			listViewHierarchyPosition.Restore();
			listViewRectTransform.SetParent(ParentCanvas.transform, worldPositionStays: false);

			if (!Utilities.IsNull(ListView))
			{
				ListView.gameObject.SetActive(false);
			}
		}
	}
}