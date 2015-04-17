using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public KeyCode UpKey = KeyCode.UpArrow;
	public KeyCode DownKey = KeyCode.DownArrow;
	public KeyCode LeftKey = KeyCode.LeftArrow;
	public KeyCode RightKey = KeyCode.RightArrow;
	public KeyCode ActionKey = KeyCode.Space;

	public float MenuTop = 200;
	public float LineWidth = 50;

	public GUISkin GuiSkin;

	public enum MenuItemType
	{
		BUTTON,
		ITEMLIST
	};
	public delegate void MenuActivated();
	public delegate void MenuListItemSelected(int index);

	private struct MenuItem
	{
		public string Text;
		public MenuItemType Type;
		public string[] ListItems;
		public MenuActivated onActivate;
		public MenuListItemSelected onItemSelect;
		public int SelectedIndex;

	}

	private System.Collections.Generic.List<MenuItem> m_menuItems = new System.Collections.Generic.List<MenuItem>();
	private bool m_enabled = false;
	private bool m_beEnabledNextFrame = false;
	private int m_selectedItem = 0;

	public void Enable() {m_beEnabledNextFrame = true;}
	public void Disable() {m_beEnabledNextFrame = false;}

	public void AddButtonItem(string itemText, MenuActivated callback)
	{
		MenuItem m = new MenuItem();
		m.Text = itemText;
		m.onActivate = callback;
		m.Type = MenuItemType.BUTTON;
		m_menuItems.Add(m);
	}

	public void AddListItem(string itemText, string[] items, int defaultItem, MenuListItemSelected callback)
	{
		MenuItem m = new MenuItem();
		m.Text = itemText;
		m.onItemSelect = callback;
		m.Type = MenuItemType.ITEMLIST;
		m.ListItems = items; 
		m.SelectedIndex = defaultItem;
		m_menuItems.Add(m);
	}

	void InterpretKeys()
	{
		//Debug.Log("Testing keys");
		if(Input.GetKeyDown(UpKey))
		{
			m_selectedItem -=1;
		}
		else if(Input.GetKeyDown(DownKey))
		{
			m_selectedItem +=1;
		}
		m_selectedItem += m_menuItems.Count;
		m_selectedItem %= m_menuItems.Count;

		MenuItem item = m_menuItems[m_selectedItem];

		if(item.Type == MenuItemType.ITEMLIST)
		{
			bool changed = false;
			if(Input.GetKeyDown(LeftKey))
			{
				item.SelectedIndex -=1;
				changed = true;
			}
			else if(Input.GetKeyDown(RightKey))
			{
				item.SelectedIndex +=1;
				changed = true;
			}

			if(changed)
			{
				item.SelectedIndex += item.ListItems.GetUpperBound(0)+1;
				item.SelectedIndex %= item.ListItems.GetUpperBound(0)+1;
				item.onItemSelect(item.SelectedIndex);
			}
		}
		else
		{
			if(Input.GetKeyDown(ActionKey))
			{
				item.onActivate();
			}

		}
		m_menuItems[m_selectedItem]= item;

	}

	void DrawMenuItem(int index, float x_centre, float top)
	{
		MenuItem item = m_menuItems[index];
		string itemTxt = item.Text;

		if(item.Type == MenuItemType.ITEMLIST)
		{
			itemTxt += ": " + item.ListItems[item.SelectedIndex];
		}

		if(index == m_selectedItem)
		{
			if(item.Type == MenuItemType.BUTTON)
			{
				itemTxt = "> " + itemTxt + " <";
			}
			else
			{
				itemTxt = "< " + itemTxt + " >";
			}
		}
		//Debug.Log("Drawing item: " + itemTxt);
		GUIContent txt = new GUIContent(itemTxt);
		Vector2 size = GUI.skin.label.CalcSize(txt);
		GUI.Label(new Rect(x_centre - size.x/2, top, size.x, size.y), txt);
	}

	void DrawMenu()
	{
		//Debug.Log("Drawing menu:");
		for(int i = 0; i < m_menuItems.Count; i++)
		{
			DrawMenuItem(i, Screen.width/2, MenuTop+i*LineWidth);
		}
	}

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(m_enabled && m_menuItems.Count > 0)
		{
			InterpretKeys();
		}
		m_enabled = m_beEnabledNextFrame; //Otherwise switching between menus can have undesired
		//effects because keys can be pressed across multiple simultaneously.
	}

	void OnGUI()
	{
		if(m_enabled && m_menuItems.Count > 0)
		{
			GUI.skin = GuiSkin;
			DrawMenu();			
		}	
	}
}
