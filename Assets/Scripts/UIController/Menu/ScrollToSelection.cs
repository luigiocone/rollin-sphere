using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollToSelection : MonoBehaviour
{
    readonly List<GameObject> m_Childrens = new();
    RectTransform m_ContentPanel;
    Scrollbar m_Scrollbar;
    GameObject m_LastSelected;

    void Awake()
    {
        m_ContentPanel = GetComponent<ScrollRect>().content; 
        m_Scrollbar = GetComponentInChildren<Scrollbar>();

        foreach (Transform children in m_ContentPanel.transform)
        {
            m_Childrens.Add(children.gameObject);
	    }
    }

    void Update()
    {
        // Get the currently selected UI element from the event system
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (!selected || selected == m_LastSelected) 
	        return;
        m_LastSelected = selected;

        // Check if the element is children of the content panel
        GameObject parent = GetParentUntil(m_ContentPanel.transform, selected.transform);
        if (!parent) return;

        MoveScrollbar(parent);
    }

    GameObject GetParentUntil(Transform until, Transform current)
    {
        var parent = current.transform.parent;
        if (parent == null) return null;
        if (parent == until) return current.gameObject;
        return GetParentUntil(until, parent);
    }

    void MoveScrollbar(GameObject curr)
    {
        // Set the scrollbar position by selected item
        float total = m_Childrens.Count;
        float position = m_Childrens.IndexOf(curr);
        float percentage = (position == total-1) ? 0f : 1f - (position / total);
        m_Scrollbar.value = percentage;
    }
}
