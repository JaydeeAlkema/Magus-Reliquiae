#if UNITY_EDITOR
using System.IO;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Menu: Magus ▶ Setup Relic UI
///     Builds (or rebuilds) the complete Relic UI hierarchy in the active scene:
///       • Creates RelicCellUI and RelicBagItemUI prefab assets in Assets/Prefabs/UI/
///       • Populates the existing "Relic Acuirement Screen" canvas object
///       • Creates the "Inventory Overlay" canvas object (board on top, bag on bottom)
///       • Wires RelicUICoordinator to AcquirementScreen, InventoryOverlay, and DragHandler
///     Run this once after opening Game.unity. Safe to re-run (clears & rebuilds children).
/// </summary>
public static class RelicUIBuilder
{
	private const string CellPrefabPath = "Assets/Prefabs/UI/RelicCellPrefab.prefab";
	private const string BagItemPrefabPath = "Assets/Prefabs/UI/RelicBagItemPrefab.prefab";

	// ── Entry point ──────────────────────────────────────────────────────────

	[MenuItem("Magus/Setup Relic UI")]
	public static void Build()
	{
		Canvas canvas = FindCanvas();
		if (canvas == null)
		{
			Debug.LogError("[RelicUIBuilder] No Canvas found in the active scene. Open Game.unity first.");
			return;
		}

		Directory.CreateDirectory("Assets/Prefabs/UI");

		GameObject cellPrefab = GetOrCreateCellPrefab();
		GameObject bagItemPrefab = GetOrCreateBagItemPrefab();

		SetupAcquirementScreen(canvas, cellPrefab, bagItemPrefab);
		InventoryOverlayUI overlay = SetupInventoryOverlay(canvas, cellPrefab, bagItemPrefab);
		WireCoordinator(canvas, overlay);

		EditorUtility.SetDirty(canvas.gameObject.scene.GetRootGameObjects()[0]);
		AssetDatabase.SaveAssets();
		Debug.Log("[RelicUIBuilder] ✓ Relic UI hierarchy built successfully.");
	}

	// ── Canvas lookup ─────────────────────────────────────────────────────────

	private static Canvas FindCanvas()
	{
		foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
		{
			if (c.gameObject.name == "UI") return c;
		}

		return Object.FindAnyObjectByType<Canvas>();
	}

	// ═══════════════════════════════════════════════════════════════════════════
	// Prefab creation
	// ═══════════════════════════════════════════════════════════════════════════

	private static GameObject GetOrCreateCellPrefab()
	{
		if (AssetDatabase.LoadAssetAtPath<GameObject>(CellPrefabPath) is { } existing)
		{
			Debug.Log("[RelicUIBuilder] RelicCellPrefab already exists — using existing asset.");
			return existing;
		}

		// Root: Image (raycast target) + RelicCellUI
		var root = new GameObject("RelicCellPrefab");
		root.layer = 5; // UI layer
		var rootRect = root.AddComponent<RectTransform>();
		rootRect.sizeDelta = new Vector2(80, 80);
		var rootImage = root.AddComponent<Image>();
		rootImage.color = Color.clear;
		rootImage.raycastTarget = true;
		root.AddComponent<CanvasRenderer>();

		var cellUI = root.AddComponent<RelicCellUI>();

		// Background child
		var bg = CreateUIObject("Background", root.transform, new Vector2(80, 80));
		var bgImage = bg.AddComponent<Image>();
		bgImage.raycastTarget = false;
		bg.AddComponent<CanvasRenderer>();

		// Icon child
		var icon = CreateUIObject("Icon", root.transform, new Vector2(56, 56));
		var iconImage = icon.AddComponent<Image>();
		iconImage.raycastTarget = false;
		iconImage.enabled = false;
		icon.AddComponent<CanvasRenderer>();

		// LevelBadge
		var badge = CreateUIObject("LevelBadge", root.transform, new Vector2(32, 20));
		var badgeRT = badge.GetComponent<RectTransform>();
		badgeRT.anchorMin = new Vector2(1, 0);
		badgeRT.anchorMax = new Vector2(1, 0);
		badgeRT.pivot = new Vector2(1, 0);
		badgeRT.anchoredPosition = new Vector2(-2, 2);
		var badgeBg = badge.AddComponent<Image>();
		badgeBg.color = new Color(0, 0, 0, 0.7f);
		badge.AddComponent<CanvasRenderer>();
		badge.SetActive(false);

		var levelTextGO = CreateUIObject("LevelText", badge.transform, new Vector2(30, 18));
		var levelText = levelTextGO.AddComponent<TextMeshProUGUI>();
		levelText.text = "1";
		levelText.fontSize = 12;
		levelText.alignment = TextAlignmentOptions.Center;
		levelText.raycastTarget = false;

		// Wire serialised fields via SerializedObject
		var so = new SerializedObject(cellUI);
		so.FindProperty("BackgroundImage").objectReferenceValue = bgImage;
		so.FindProperty("RelicIconImage").objectReferenceValue = iconImage;
		so.FindProperty("LevelBadge").objectReferenceValue = badge;
		so.FindProperty("LevelText").objectReferenceValue = levelText;
		so.ApplyModifiedProperties();

		var prefab = PrefabUtility.SaveAsPrefabAsset(root, CellPrefabPath);
		Object.DestroyImmediate(root);
		return prefab;
	}

	private static GameObject GetOrCreateBagItemPrefab()
	{
		if (AssetDatabase.LoadAssetAtPath<GameObject>(BagItemPrefabPath) is { } existing)
		{
			Debug.Log("[RelicUIBuilder] RelicBagItemPrefab already exists — using existing asset.");
			return existing;
		}

		var root = new GameObject("RelicBagItemPrefab");
		root.layer = 5;
		var rootRect = root.AddComponent<RectTransform>();
		rootRect.sizeDelta = new Vector2(90, 90);
		var rootImage = root.AddComponent<Image>();
		rootImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
		rootImage.raycastTarget = true;
		root.AddComponent<CanvasRenderer>();

		var bagItem = root.AddComponent<RelicBagItemUI>();

		// RarityBorder child (slightly larger than root, behind icon)
		var border = CreateUIObject("RarityBorder", root.transform, new Vector2(90, 90));
		var borderImage = border.AddComponent<Image>();
		borderImage.raycastTarget = false;
		borderImage.color = Color.grey;
		border.AddComponent<CanvasRenderer>();
		FillParent(border.GetComponent<RectTransform>());

		// Icon child
		var icon = CreateUIObject("Icon", root.transform, new Vector2(64, 64));
		var iconImage = icon.AddComponent<Image>();
		iconImage.raycastTarget = false;
		icon.AddComponent<CanvasRenderer>();

		// Level text (bottom right corner)
		var lvlGO = CreateUIObject("LevelText", root.transform, new Vector2(40, 20));
		var lvlRT = lvlGO.GetComponent<RectTransform>();
		lvlRT.anchorMin = new Vector2(1, 0);
		lvlRT.anchorMax = new Vector2(1, 0);
		lvlRT.pivot = new Vector2(1, 0);
		lvlRT.anchoredPosition = new Vector2(-2, 2);
		var lvlText = lvlGO.AddComponent<TextMeshProUGUI>();
		lvlText.text = "";
		lvlText.fontSize = 11;
		lvlText.alignment = TextAlignmentOptions.Right;
		lvlText.raycastTarget = false;

		var so = new SerializedObject(bagItem);
		so.FindProperty("IconImage").objectReferenceValue = iconImage;
		so.FindProperty("RarityBorderImage").objectReferenceValue = borderImage;
		so.FindProperty("LevelText").objectReferenceValue = lvlText;
		so.ApplyModifiedProperties();

		var prefab = PrefabUtility.SaveAsPrefabAsset(root, BagItemPrefabPath);
		Object.DestroyImmediate(root);
		return prefab;
	}

	// ═══════════════════════════════════════════════════════════════════════════
	// Acquirement Screen
	// ═══════════════════════════════════════════════════════════════════════════

	private static void SetupAcquirementScreen(Canvas canvas, GameObject cellPrefab, GameObject bagItemPrefab)
	{
		// Find existing "Relic Acuirement Screen" under the canvas (typo preserved to match scene)
		Transform existing = canvas.transform.Find("Relic Acuirement Screen");
		GameObject screenGO = existing != null ? existing.gameObject : new GameObject("Relic Acuirement Screen");

		if (existing == null)
		{
			screenGO.layer = 5;
			screenGO.transform.SetParent(canvas.transform, false);
		}

		// Clear any pre-existing children
		for (int i = screenGO.transform.childCount - 1; i >= 0; i--)
			Object.DestroyImmediate(screenGO.transform.GetChild(i).gameObject);

		// Ensure RectTransform fills the canvas
		var rt = screenGO.GetOrAddComponent<RectTransform>();
		StretchFull(rt);

		// Dim background
		var bgImage = screenGO.GetOrAddComponent<Image>();
		bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
		bgImage.raycastTarget = true;
		screenGO.GetOrAddComponent<CanvasRenderer>();

		var screenUI = screenGO.GetOrAddComponent<RelicAcquirementScreenUI>();

		// Cards container (centred, horizontal row)
		var cardsContainer = CreateUIObject("CardsContainer", screenGO.transform, new Vector2(1200, 600));
		cardsContainer.AddComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
		var hlg = cardsContainer.GetComponent<HorizontalLayoutGroup>();
		hlg.spacing = 40;
		hlg.childForceExpandWidth = false;
		hlg.childForceExpandHeight = false;
		var ccRT = cardsContainer.GetComponent<RectTransform>();
		ccRT.anchorMin = new Vector2(0.5f, 0.5f);
		ccRT.anchorMax = new Vector2(0.5f, 0.5f);
		ccRT.pivot = new Vector2(0.5f, 0.5f);
		ccRT.anchoredPosition = new Vector2(0, 80);

		// 3 relic offer cards
		var cards = new RelicCardUI[3];
		for (int i = 0; i < 3; i++)
			cards[i] = CreateRelicCard($"Card_{i}", cardsContainer.transform);

		// "Manage Board" button — shown after a card is picked
		var manageBtnGO = CreateButton("ManageBoardButton", screenGO.transform, new Vector2(440, 90), "Manage Board");
		var manageBtnRT = manageBtnGO.GetComponent<RectTransform>();
		manageBtnRT.anchorMin = new Vector2(0.5f, 0);
		manageBtnRT.anchorMax = new Vector2(0.5f, 0);
		manageBtnRT.pivot = new Vector2(0.5f, 0);
		manageBtnRT.anchoredPosition = new Vector2(0, 120);
		manageBtnGO.SetActive(false);

		// Wire RelicAcquirementScreenUI fields
		var so = new SerializedObject(screenUI);
		var cardsProp = so.FindProperty("Cards");
		cardsProp.arraySize = 3;
		for (int i = 0; i < 3; i++)
			cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
		so.FindProperty("ManageBoardButton").objectReferenceValue = manageBtnGO.GetComponent<Button>();
		so.ApplyModifiedProperties();

		screenGO.SetActive(false);
		EditorUtility.SetDirty(screenGO);
		Debug.Log("[RelicUIBuilder] ✓ Acquirement Screen built.");
	}

	private static RelicCardUI CreateRelicCard(string name, Transform parent)
	{
		var cardGO = new GameObject(name);
		cardGO.layer = 5;
		cardGO.transform.SetParent(parent, false);

		var rt = cardGO.AddComponent<RectTransform>();
		rt.sizeDelta = new Vector2(320, 520);

		// Background image
		var bg = cardGO.AddComponent<Image>();
		bg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
		bg.raycastTarget = true;
		cardGO.AddComponent<CanvasRenderer>();

		// Select Button (whole card is a button)
		var btn = cardGO.AddComponent<Button>();
		btn.targetGraphic = bg;

		var card = cardGO.AddComponent<RelicCardUI>();

		// Rarity border (full-size overlay)
		var borderGO = CreateUIObject("RarityBorder", cardGO.transform, Vector2.zero);
		var borderImage = borderGO.AddComponent<Image>();
		borderImage.color = Color.grey;
		borderImage.raycastTarget = false;
		borderGO.AddComponent<CanvasRenderer>();
		FillParent(borderGO.GetComponent<RectTransform>());

		// Icon (upper portion)
		var iconGO = CreateUIObject("Icon", cardGO.transform, new Vector2(200, 200));
		var iconRT = iconGO.GetComponent<RectTransform>();
		iconRT.anchorMin = new Vector2(0.5f, 1);
		iconRT.anchorMax = new Vector2(0.5f, 1);
		iconRT.pivot = new Vector2(0.5f, 1);
		iconRT.anchoredPosition = new Vector2(0, -30);
		var iconImage = iconGO.AddComponent<Image>();
		iconImage.raycastTarget = false;
		iconGO.AddComponent<CanvasRenderer>();

		// Name text
		var nameLabelGO = CreateUIObject("NameText", cardGO.transform, new Vector2(280, 48));
		var nameRT = nameLabelGO.GetComponent<RectTransform>();
		nameRT.anchorMin = new Vector2(0.5f, 1);
		nameRT.anchorMax = new Vector2(0.5f, 1);
		nameRT.pivot = new Vector2(0.5f, 1);
		nameRT.anchoredPosition = new Vector2(0, -240);
		var nameText = nameLabelGO.AddComponent<TextMeshProUGUI>();
		nameText.text = "Relic Name";
		nameText.fontSize = 22;
		nameText.fontStyle = FontStyles.Bold;
		nameText.alignment = TextAlignmentOptions.Center;
		nameText.raycastTarget = false;

		// Description text
		var descGO = CreateUIObject("DescriptionText", cardGO.transform, new Vector2(280, 180));
		var descRT = descGO.GetComponent<RectTransform>();
		descRT.anchorMin = new Vector2(0.5f, 1);
		descRT.anchorMax = new Vector2(0.5f, 1);
		descRT.pivot = new Vector2(0.5f, 1);
		descRT.anchoredPosition = new Vector2(0, -298);
		var descText = descGO.AddComponent<TextMeshProUGUI>();
		descText.text = "Description";
		descText.fontSize = 16;
		descText.alignment = TextAlignmentOptions.Center;
		descText.raycastTarget = false;

		// Wire card fields
		var so = new SerializedObject(card);
		so.FindProperty("IconImage").objectReferenceValue = iconImage;
		so.FindProperty("NameText").objectReferenceValue = nameText;
		so.FindProperty("DescriptionText").objectReferenceValue = descText;
		so.FindProperty("RarityBorderImage").objectReferenceValue = borderImage;
		so.FindProperty("SelectButton").objectReferenceValue = btn;
		so.ApplyModifiedProperties();

		return card;
	}

	// ═══════════════════════════════════════════════════════════════════════════
	// Inventory Overlay
	// ═══════════════════════════════════════════════════════════════════════════

	private static InventoryOverlayUI SetupInventoryOverlay(Canvas canvas, GameObject cellPrefab, GameObject bagItemPrefab)
	{
		// Remove existing overlay if present
		Transform existing = canvas.transform.Find("InventoryOverlay");
		if (existing != null) Object.DestroyImmediate(existing.gameObject);

		var overlayGO = new GameObject("InventoryOverlay");
		overlayGO.layer = 5;
		overlayGO.transform.SetParent(canvas.transform, false);

		var overlayRT = overlayGO.AddComponent<RectTransform>();
		StretchFull(overlayRT);

		var overlayBg = overlayGO.AddComponent<Image>();
		overlayBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
		overlayBg.raycastTarget = true;
		overlayGO.AddComponent<CanvasRenderer>();

		var overlayUI = overlayGO.AddComponent<InventoryOverlayUI>();

		// ── Relic Board section (top ~55% of screen) ──────────────────────────
		var boardSection = CreateUIObject("RelicBoardSection", overlayGO.transform, Vector2.zero);
		var boardSectionRT = boardSection.GetComponent<RectTransform>();
		boardSectionRT.anchorMin = new Vector2(0, 0.45f);
		boardSectionRT.anchorMax = new Vector2(1, 1f);
		boardSectionRT.offsetMin = new Vector2(20, 20);
		boardSectionRT.offsetMax = new Vector2(-20, -80);

		// Board header
		var boardHeader = CreateUIObject("Header", boardSection.transform, new Vector2(0, 50));
		var boardHeaderRT = boardHeader.GetComponent<RectTransform>();
		boardHeaderRT.anchorMin = new Vector2(0, 1);
		boardHeaderRT.anchorMax = new Vector2(1, 1);
		boardHeaderRT.pivot = new Vector2(0.5f, 1);
		boardHeaderRT.anchoredPosition = Vector2.zero;
		boardHeaderRT.offsetMin = new Vector2(0, -50);
		boardHeaderRT.offsetMax = Vector2.zero;
		var boardHeaderText = boardHeader.AddComponent<TextMeshProUGUI>();
		boardHeaderText.text = "Relic Board";
		boardHeaderText.fontSize = 26;
		boardHeaderText.fontStyle = FontStyles.Bold;
		boardHeaderText.alignment = TextAlignmentOptions.Center;
		boardHeaderText.raycastTarget = false;

		// RelicBoardUI container
		var boardGO = CreateUIObject("RelicBoard", boardSection.transform, Vector2.zero);
		var boardRT = boardGO.GetComponent<RectTransform>();
		boardRT.anchorMin = new Vector2(0, 0);
		boardRT.anchorMax = new Vector2(1, 1);
		boardRT.offsetMin = new Vector2(0, 0);
		boardRT.offsetMax = new Vector2(0, -60);
		var boardBg = boardGO.AddComponent<Image>();
		boardBg.color = new Color(0.08f, 0.08f, 0.14f, 0.9f);
		boardBg.raycastTarget = false;
		boardGO.AddComponent<CanvasRenderer>();

		var boardUI = boardGO.AddComponent<RelicBoardUI>();

		// Grid child for RelicBoardUI
		var gridGO = CreateUIObject("Grid", boardGO.transform, Vector2.zero);
		FillParent(gridGO.GetComponent<RectTransform>(), new Vector2(10, 10));
		var grid = gridGO.AddComponent<GridLayoutGroup>();
		grid.cellSize = new Vector2(72, 72);
		grid.spacing = new Vector2(4, 4);
		grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		grid.constraintCount = 6;
		grid.childAlignment = TextAnchor.UpperCenter;

		// Wire RelicBoardUI fields
		var boardSO = new SerializedObject(boardUI);
		boardSO.FindProperty("CellPrefab").objectReferenceValue = cellPrefab.GetComponent<RelicCellUI>();
		boardSO.FindProperty("Grid").objectReferenceValue = grid;
		boardSO.ApplyModifiedProperties();

		// ── Bag of Holding section (bottom ~45% of screen) ────────────────────
		var bagSection = CreateUIObject("BagSection", overlayGO.transform, Vector2.zero);
		var bagSectionRT = bagSection.GetComponent<RectTransform>();
		bagSectionRT.anchorMin = new Vector2(0, 0);
		bagSectionRT.anchorMax = new Vector2(1, 0.45f);
		bagSectionRT.offsetMin = new Vector2(20, 20);
		bagSectionRT.offsetMax = new Vector2(-20, -10);

		// Bag header
		var bagHeader = CreateUIObject("Header", bagSection.transform, new Vector2(0, 44));
		var bagHeaderRT = bagHeader.GetComponent<RectTransform>();
		bagHeaderRT.anchorMin = new Vector2(0, 1);
		bagHeaderRT.anchorMax = new Vector2(1, 1);
		bagHeaderRT.pivot = new Vector2(0.5f, 1);
		bagHeaderRT.anchoredPosition = Vector2.zero;
		bagHeaderRT.offsetMin = new Vector2(0, -44);
		bagHeaderRT.offsetMax = Vector2.zero;
		var bagHeaderText = bagHeader.AddComponent<TextMeshProUGUI>();
		bagHeaderText.text = "Bag of Holding";
		bagHeaderText.fontSize = 22;
		bagHeaderText.fontStyle = FontStyles.Bold;
		bagHeaderText.alignment = TextAlignmentOptions.Center;
		bagHeaderText.raycastTarget = false;

		// ScrollRect
		var scrollGO = CreateUIObject("ScrollRect", bagSection.transform, Vector2.zero);
		var scrollRT = scrollGO.GetComponent<RectTransform>();
		scrollRT.anchorMin = new Vector2(0, 0);
		scrollRT.anchorMax = new Vector2(1, 1);
		scrollRT.offsetMin = new Vector2(0, 0);
		scrollRT.offsetMax = new Vector2(0, -54);

		var scrollBg = scrollGO.AddComponent<Image>();
		scrollBg.color = new Color(0.08f, 0.08f, 0.14f, 0.9f);
		scrollBg.raycastTarget = true;
		scrollGO.AddComponent<CanvasRenderer>();

		var bagUI = scrollGO.AddComponent<BagOfHoldingUI>();
		var scrollRect = scrollGO.AddComponent<ScrollRect>();
		scrollRect.horizontal = false;
		scrollRect.vertical = true;

		// Viewport
		var viewportGO = CreateUIObject("Viewport", scrollGO.transform, Vector2.zero);
		FillParent(viewportGO.GetComponent<RectTransform>());
		var viewportImage = viewportGO.AddComponent<Image>();
		viewportImage.color = Color.clear;
		viewportImage.raycastTarget = false;
		viewportGO.AddComponent<CanvasRenderer>();
		viewportGO.AddComponent<Mask>().showMaskGraphic = false;

		// Content
		var contentGO = CreateUIObject("Content", viewportGO.transform, new Vector2(0, 0));
		var contentRT = contentGO.GetComponent<RectTransform>();
		contentRT.anchorMin = new Vector2(0, 1);
		contentRT.anchorMax = new Vector2(1, 1);
		contentRT.pivot = new Vector2(0.5f, 1);
		contentRT.anchoredPosition = Vector2.zero;
		contentRT.sizeDelta = new Vector2(0, 200);

		var contentLayout = contentGO.AddComponent<GridLayoutGroup>();
		contentLayout.cellSize = new Vector2(90, 90);
		contentLayout.spacing = new Vector2(10, 10);
		contentLayout.padding = new RectOffset(10, 10, 10, 10);
		contentLayout.childAlignment = TextAnchor.UpperLeft;

		// ContentSizeFitter so scroll view grows with items
		var csf = contentGO.AddComponent<ContentSizeFitter>();
		csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		// Finish wiring ScrollRect
		scrollRect.viewport = viewportGO.GetComponent<RectTransform>();
		scrollRect.content = contentRT;

		// Wire BagOfHoldingUI fields
		var bagSO = new SerializedObject(bagUI);
		bagSO.FindProperty("ItemPrefab").objectReferenceValue = bagItemPrefab.GetComponent<RelicBagItemUI>();
		bagSO.FindProperty("ItemContainer").objectReferenceValue = contentRT;
		bagSO.ApplyModifiedProperties();

		// ── Divider line ───────────────────────────────────────────────────────
		var divider = CreateUIObject("Divider", overlayGO.transform, new Vector2(0, 2));
		var divRT = divider.GetComponent<RectTransform>();
		divRT.anchorMin = new Vector2(0.05f, 0.45f);
		divRT.anchorMax = new Vector2(0.95f, 0.45f);
		divRT.sizeDelta = new Vector2(0, 2);
		var divImage = divider.AddComponent<Image>();
		divImage.color = new Color(0.5f, 0.5f, 0.6f, 0.5f);
		divImage.raycastTarget = false;
		divider.AddComponent<CanvasRenderer>();

		// ── Close button ────────────────────────────────────────────────────────
		var closeBtn = CreateButton("CloseButton", overlayGO.transform, new Vector2(200, 70), "Close");
		var closeBtnRT = closeBtn.GetComponent<RectTransform>();
		closeBtnRT.anchorMin = new Vector2(1, 1);
		closeBtnRT.anchorMax = new Vector2(1, 1);
		closeBtnRT.pivot = new Vector2(1, 1);
		closeBtnRT.anchoredPosition = new Vector2(-20, -20);

		// Wire InventoryOverlayUI fields
		var overlaySO = new SerializedObject(overlayUI);
		overlaySO.FindProperty("_boardUI").objectReferenceValue = boardUI;
		overlaySO.FindProperty("_bagUI").objectReferenceValue = bagUI;
		overlaySO.FindProperty("CloseButton").objectReferenceValue = closeBtn.GetComponent<Button>();
		overlaySO.ApplyModifiedProperties();

		overlayGO.SetActive(false);
		EditorUtility.SetDirty(overlayGO);
		Debug.Log("[RelicUIBuilder] ✓ Inventory Overlay built.");
		return overlayUI;
	}

	// ═══════════════════════════════════════════════════════════════════════════
	// Coordinator wiring
	// ═══════════════════════════════════════════════════════════════════════════

	private static void WireCoordinator(Canvas canvas, InventoryOverlayUI overlay)
	{
		var coordinator = Object.FindAnyObjectByType<RelicUICoordinator>();
		if (coordinator == null)
		{
			Debug.LogWarning("[RelicUIBuilder] RelicUICoordinator not found in scene — skipping coordinator wiring.");
			return;
		}

		var acquirementScreen = Object.FindAnyObjectByType<RelicAcquirementScreenUI>(FindObjectsInactive.Include);
		var dragHandler = Object.FindAnyObjectByType<RelicDragHandler>(FindObjectsInactive.Include);

		var so = new SerializedObject(coordinator);
		so.FindProperty("AcquirementScreen").objectReferenceValue = acquirementScreen;
		so.FindProperty("InventoryOverlay").objectReferenceValue = overlay;
		so.FindProperty("DragHandler").objectReferenceValue = dragHandler;
		so.ApplyModifiedProperties();

		EditorUtility.SetDirty(coordinator);
		Debug.Log("[RelicUIBuilder] ✓ RelicUICoordinator wired.");
	}

	// ═══════════════════════════════════════════════════════════════════════════
	// Helpers
	// ═══════════════════════════════════════════════════════════════════════════

	private static GameObject CreateUIObject(string name, Transform parent, Vector2 size)
	{
		var go = new GameObject(name);
		go.layer = 5;
		go.transform.SetParent(parent, false);
		var rt = go.AddComponent<RectTransform>();
		rt.sizeDelta = size;
		return go;
	}

	private static GameObject CreateButton(string name, Transform parent, Vector2 size, string label)
	{
		var go = CreateUIObject(name, parent, size);
		var img = go.AddComponent<Image>();
		img.color = new Color(0.18f, 0.28f, 0.5f, 1f);
		img.raycastTarget = true;
		go.AddComponent<CanvasRenderer>();
		var btn = go.AddComponent<Button>();
		btn.targetGraphic = img;

		var lblGO = CreateUIObject("Label", go.transform, size);
		FillParent(lblGO.GetComponent<RectTransform>());
		var txt = lblGO.AddComponent<TextMeshProUGUI>();
		txt.text = label;
		txt.fontSize = 20;
		txt.alignment = TextAlignmentOptions.Center;
		txt.raycastTarget = false;

		return go;
	}

	private static void StretchFull(RectTransform rt)
	{
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
	}

	private static void FillParent(RectTransform rt, Vector2? padding = null)
	{
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		Vector2 pad = padding ?? Vector2.zero;
		rt.offsetMin = pad;
		rt.offsetMax = -pad;
	}
}

// Extension method to avoid duplicate AddComponent calls
public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		return go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();
	}
}
#endif
