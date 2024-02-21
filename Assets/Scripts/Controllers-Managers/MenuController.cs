using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public class MenuController : MonoSingleton<MenuController> {
	[SerializeField] private Page _initialPage;
	[SerializeField] private Canvas _rootCanvas;
	private Stack<Page> _pageStack = new();

	private void Start() {
		if (_initialPage != null) PushPage(_initialPage);
	}

	///<summary>
	///Disable the active page
	///</summary>
	private void OnCancel() {
		if (_rootCanvas.enabled && _rootCanvas.gameObject.activeInHierarchy && _pageStack.Count > 0) PopPage();
	}

	///<summary>
	///Return whether the given page is in the page stack
	///</summary>
	///<param name="page">The page</param>
	///<returns>Whether the given page is in the page stack</returns>
	public bool IsPageInStack(Page page) {
		return _pageStack.Contains(page);
	}

	///<summary>
	///Returns whether the given page is on the top of the page stack
	///</summary>
	///<param name="page">The page</param>
	///<returns>Whether the given page is on the top of the page stack</returns>
	public bool IsPageOnTopOfStack(Page page) {
		return _pageStack.Count > 1 && page == _pageStack.Peek();
	}
	
	///<summary>
	///Add the page to the page stack and set it as active
	///</summary>
	///<param name="page">The page</param>
	public void PushPage(Page page) {
		if (_pageStack.Count > 0) _pageStack.Peek().Exit();
		_pageStack.Push(page);
		page.Enter();
	}

	///<summary>
	///Disable the active page and remove it from the page stack
	///</summary>
	public void PopPage() {
		if (_pageStack.Count <= 1) return;
		_pageStack.Pop().Exit();
		if (_pageStack.Count > 0) _pageStack.Peek().Enter();
	}

	///<summary>
	///Remove and disable all the pages in the page stack
	///</summary>
	public void PopAllPages() {
		for (var i = 1; i < _pageStack.Count; i++) PopPage();
	}
	
	///<summary>
	///Quit the application
	///</summary>
	public void OnQuitButton() {
		Server.Instance.ShutDown();
		Client.Instance.ShutDown();
		Application.Quit();
	}
}