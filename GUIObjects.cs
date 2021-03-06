﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GUIObjects 
{
	public static GameObject EventSystemInstance;

	private static string[] backgroundImages = {
		"GUI/In_Game_Menu/Clear",
		"GUI/Main_Load_Screen/Loading_Screen_Image",
		"GUI/Main_Load_Screen/Loading_Bar_Active_Background",
		"GUI/In_Game_Menu/bg_shadow"
	};

	private static string[] buttonImages = {
		"GUI/Main_Load_Screen/Button_PlayGame2",
		"GUI/Main_Load_Screen/Button_NewMap",
		"GUI/Main_Load_Screen/Button_Exit",
		"GUI/Main_Load_Screen/Button_Save",
		"GUI/Main_Load_Screen/Button_Load"
	};

	private static string[] loadingBarImages = {
		"GUI/Main_Load_Screen/Loading_Bar"
	};

	private static string[] spriteImages = {
		"GUI/Main_Load_Screen/Loading_Border"
	};

	private static string[] fontTypes = {
		"Arial.ttf"
	};

	//Constructor
	public GUIObjects()
	{

	}

	public static void SetCursorLock(bool value)
	{
		if (!value)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} 
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public static void addEventSystem () 
	{
		if (EventSystemInstance == null) 
		{
			
			GameObject eventSystem = new GameObject ();
			eventSystem.name = "EventSystem";
			eventSystem.AddComponent<EventSystem> ();
			eventSystem.AddComponent<StandaloneInputModule> ();
			EventSystemInstance = eventSystem;
			GameObject.DontDestroyOnLoad (eventSystem);
		} 
	}

	public static GameObject createCanvas(Camera camera, string canvasName)
	{
		GameObject canvasGo = new GameObject ();
		canvasGo.name = canvasName;
		canvasGo.tag = "Canvas";
		canvasGo.AddComponent<RectTransform> ();
		Canvas canvasCV = canvasGo.AddComponent<Canvas> ();
		canvasCV.renderMode = RenderMode.ScreenSpaceOverlay;
		canvasGo.AddComponent<GraphicRaycaster> ();
		canvasCV.worldCamera = camera;

		return canvasGo;
	}

	public static void addBackgroundImage(GameObject canvasGo, string name, Vector2 screenPos, Vector2 size, int selector)
	{
		Image canvasIM = canvasGo.GetComponent<Image> ();
		if(canvasIM == null)
			canvasIM = canvasGo.AddComponent<Image> ();
		canvasIM.name = name;
		canvasIM.rectTransform.sizeDelta = size;
		canvasIM.rectTransform.anchoredPosition = screenPos;
		Sprite canvasSP = Resources.Load<Sprite> (backgroundImages[selector]);
		canvasIM.overrideSprite = canvasSP;
	}

	public static Button addButton(GameObject canvasGo, string name, Vector2 screenPos, Vector2 size, int selector)
	{
		GameObject buttonGO = new GameObject ();
		buttonGO.transform.position = canvasGo.transform.position;
		buttonGO.name = name;
		buttonGO.tag = "Button";
		RectTransform buttonRT = buttonGO.AddComponent<RectTransform> ();
		buttonRT.SetParent (canvasGo.GetComponent<RectTransform>());
		buttonRT.sizeDelta = size;
		buttonRT.anchoredPosition = screenPos;

		Button buttonBU = buttonGO.AddComponent<Button> ();

		buttonBU.onClick.RemoveAllListeners ();
		Image buttonIM = buttonGO.AddComponent<Image>();
		Sprite buttonSP = Resources.Load<Sprite>(buttonImages[selector]);
		buttonIM.overrideSprite = buttonSP;

		return buttonBU;
	}

	public static Text addText(GameObject canvasGo, string name, Vector2 screenPos, Vector2 size, string text, int fontSelect, int fontSize, Color color, TextAnchor anc)
	{
		GameObject textGO = new GameObject ();
		textGO.transform.position = canvasGo.transform.position;
		textGO.name = name;
		textGO.tag = "Text";
		RectTransform textRT = textGO.AddComponent<RectTransform> ();
		textRT.SetParent (canvasGo.GetComponent<RectTransform>());
		textRT.sizeDelta = size;
		textRT.anchoredPosition = screenPos;

		Text textO = textGO.AddComponent<Text> ();
		textO.text = text;
		textO.fontSize = fontSize;
		textO.color = color;
		textO.alignment = anc;

		Font aFont = (Font)Resources.GetBuiltinResource(typeof (Font), fontTypes[fontSelect]);
		textO.font = aFont;

		return textO;
	}

	public static Image addLoadingBar(GameObject canvasGo, string name, Vector2 screenPos, Vector2 size, int selector)
	{
		GameObject loadingBar = new GameObject();
		loadingBar.transform.position = canvasGo.transform.position;
		RectTransform loaderRT = loadingBar.AddComponent<RectTransform> ();
		Image canvasLD = loadingBar.AddComponent<Image> ();
		loadingBar.tag = "Image";

		loaderRT.SetParent (canvasGo.gameObject.GetComponent<RectTransform>());
		canvasLD.name = name;
		canvasLD.rectTransform.sizeDelta = size;
		canvasLD.rectTransform.anchoredPosition = screenPos;
		Sprite canvasLSP = Resources.Load<Sprite> (loadingBarImages[selector]);
		canvasLD.overrideSprite = canvasLSP;
		canvasLD.type = Image.Type.Filled;
		canvasLD.fillMethod = Image.FillMethod.Horizontal;
		canvasLD.fillAmount = 0;
		return canvasLD;
	}

	public static Image addImage(GameObject canvasGo, string name, Vector2 screenPos, Vector2 size, int selector)
	{
		GameObject im = new GameObject();
		im.transform.position = canvasGo.transform.position;
		RectTransform imRT = im.AddComponent<RectTransform> ();
		Image canvasLD = im.AddComponent<Image> ();
		im.tag = "Image";

		imRT.SetParent (canvasGo.gameObject.GetComponent<RectTransform>());
		canvasLD.name = name;
		canvasLD.rectTransform.sizeDelta = size;
		canvasLD.rectTransform.anchoredPosition = screenPos;
		Sprite canvasLSP = Resources.Load<Sprite> (spriteImages[selector]);
		canvasLD.overrideSprite = canvasLSP;
		canvasLD.type = Image.Type.Filled;
		canvasLD.fillMethod = Image.FillMethod.Horizontal;
		canvasLD.fillAmount = 1;
		return canvasLD;
	}
}
