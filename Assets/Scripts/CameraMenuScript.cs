using UnityEngine;
using System.Collections;

public class CameraMenuScript : MonoBehaviour {
	
	public GUISkin GuiSkin;

	void woo()
	{
		Debug.Log("Woo!");
	}

	void hurrah(int x)
	{
		Debug.Log("Hurrah: " + x.ToString());
	}


	void goMenu2()
	{
		this.menu.Disable();
		menu2.Enable();
	}
	
	void goMenu1()
	{
		this.menu.Enable();
		menu2.Disable();
	}

	private Menu menu;
	private Menu menu2;

	// Use this for initialization
	void Start () {
		menu = this.gameObject.AddComponent<Menu>();
		menu.AddButtonItem("Button test one", woo);
		menu.AddButtonItem("Go to menu 2", goMenu2); 
		
		string[] items = {"0: Hello", "1: Goodbye", "2: Yellow", "3: And sigh"};
		menu.AddListItem("List test", items, 0, hurrah);

		menu.GuiSkin = GuiSkin;
		
		menu.Enable();

		menu2 = this.gameObject.AddComponent<Menu>();
		menu2.AddButtonItem("Button test two", woo);
		menu2.AddButtonItem("Go back to menu 1", goMenu1);
		
		menu2.GuiSkin = GuiSkin;
		menu2.MenuTop = 25;
		menu2.LineWidth = 100;


	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			this.menu.Disable();
			menu2.Enable();
		}
	}
}
