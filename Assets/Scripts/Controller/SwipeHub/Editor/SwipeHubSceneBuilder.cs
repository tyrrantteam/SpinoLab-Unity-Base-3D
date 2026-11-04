using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinGroup.UI.SwipeHub.Editor
{
    public static class SwipeHubSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SwipeHubScene.unity";
        private const float ReferenceWidth = 2160f;
        private const float ReferenceHeight = 1080f;
        private const float BottomBarHeight = 180f;

        private static readonly Color ShopColor = new(0.88f, 0.45f, 0.22f, 1f);
        private static readonly Color HomeColor = new(0.16f, 0.52f, 0.58f, 1f);
        private static readonly Color TeamColor = new(0.40f, 0.28f, 0.58f, 1f);
        private static readonly Color BarColor = new(0.10f, 0.10f, 0.14f, 0.96f);

        [MenuItem("Tools/Swipe Hub/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            BuildAndSave(openAfter: true);
        }

        public static void BuildAndSave(bool openAfter)
        {
            var previous = EditorSceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            try
            {
                CreateCamera();
                CreateEventSystem();
                var canvas = CreateCanvas();
                var viewport = CreateViewport(canvas.transform);
                var content = CreateRect("PageContent", viewport.transform);
                var hub = viewport.gameObject.AddComponent<SwipePageHub>();

                var shopPage = CreateShopPage(content);
                var homePage = CreateHomePage(content);
                var teamPage = CreateTeamPage(content);
                var tabs = CreateBottomBar(canvas.transform);

                var pages = new[]
                {
                    shopPage.GetComponent<SwipePage>(),
                    homePage.GetComponent<SwipePage>(),
                    teamPage.GetComponent<SwipePage>(),
                };

                WireHub(hub, viewport, content, pages, tabs);
                hub.Relayout();
                foreach (var tab in tabs)
                    tab.Initialize(hub);
                hub.SelectPage(1, false);

                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
                Debug.Log($"[SwipeHub] Demo scene saved to {ScenePath}. Open it and press Play to swipe Shop / Home / Team.");

                if (openAfter)
                {
                    EditorSceneManager.OpenScene(ScenePath);
                    return;
                }
            }
            finally
            {
                if (!openAfter)
                {
                    if (scene.IsValid() && scene.isLoaded)
                        EditorSceneManager.CloseScene(scene, true);

                    if (previous.IsValid() && previous.isLoaded)
                        EditorSceneManager.SetActiveScene(previous);
                }
            }
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            go.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1
                                              | AdditionalCanvasShaderChannels.Normal
                                              | AdditionalCanvasShaderChannels.Tangent;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static RectTransform CreateViewport(Transform parent)
        {
            var viewport = CreateRect("PageViewport", parent);
            Stretch(viewport);
            var image = viewport.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            image.sprite = UiSprite();
            viewport.gameObject.AddComponent<RectMask2D>();
            return viewport;
        }

        private static GameObject CreateShopPage(RectTransform parent)
        {
            var page = CreatePageRoot(parent, "ShopPage", SwipePageKind.Shop, ShopColor);

            CreateHeader(page.transform, "SHOP", "Coins  1,200");
            CreateTitle(page.transform, "SHOP", new Vector2(0f, 180f), 96f);

            var scrollRoot = CreateRect("Scroll View", page.transform);
            Stretch(scrollRoot, new Vector2(80f, BottomBarHeight + 40f), new Vector2(-80f, -220f));

            var scrollImage = scrollRoot.gameObject.AddComponent<Image>();
            scrollImage.color = new Color(1f, 1f, 1f, 0.06f);
            scrollImage.sprite = UiSprite();
            scrollImage.type = Image.Type.Sliced;
            scrollImage.raycastTarget = true;

            var viewport = CreateRect("Viewport", scrollRoot);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;

            var content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 1280f);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            for (var i = 1; i <= 8; i++)
                CreateShopRow(content, i);

            var scroll = scrollRoot.gameObject.AddComponent<SwipeNestedScrollRect>();
            var scrollSo = new SerializedObject(scroll);
            scrollSo.FindProperty("m_Content").objectReferenceValue = content;
            scrollSo.FindProperty("m_Viewport").objectReferenceValue = viewport;
            scrollSo.FindProperty("m_Horizontal").boolValue = false;
            scrollSo.FindProperty("m_Vertical").boolValue = true;
            scrollSo.FindProperty("m_MovementType").enumValueIndex = (int)ScrollRect.MovementType.Elastic;
            scrollSo.FindProperty("m_Inertia").boolValue = true;
            scrollSo.ApplyModifiedPropertiesWithoutUndo();

            return page.gameObject;
        }

        private static void CreateShopRow(RectTransform parent, int index)
        {
            var row = CreateRect($"Pack {index}", parent);
            row.sizeDelta = new Vector2(0f, 130f);
            var layout = row.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 130f;
            layout.preferredHeight = 130f;

            var image = row.gameObject.AddComponent<Image>();
            image.sprite = UiSprite();
            image.type = Image.Type.Sliced;
            image.color = index % 2 == 0
                ? new Color(1f, 1f, 1f, 0.16f)
                : new Color(1f, 0.92f, 0.75f, 0.22f);
            image.raycastTarget = false;

            CreateLabel(row, $"Gold Pack {index}    x{index * 100}", 42f, TextAlignmentOptions.MidlineLeft)
                .GetComponent<RectTransform>().offsetMin = new Vector2(32f, 0f);
        }

        private static GameObject CreateHomePage(RectTransform parent)
        {
            var page = CreatePageRoot(parent, "HomePage", SwipePageKind.Home, HomeColor);
            CreateHeader(page.transform, "HOME", "Heart  5");
            CreateTitle(page.transform, "HOME", new Vector2(0f, 220f), 110f);
            var hint = CreateLabel(page.transform, "Vuot trai / phai de chuyen man", 36f, TextAlignmentOptions.Center);
            var hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.pivot = new Vector2(0.5f, 0.5f);
            hintRect.sizeDelta = new Vector2(1200f, 80f);
            hintRect.anchoredPosition = new Vector2(0f, 80f);

            var play = CreateRect("PlayButton", page.transform);
            play.sizeDelta = new Vector2(420f, 140f);
            play.anchoredPosition = new Vector2(0f, -40f);
            var playImage = play.gameObject.AddComponent<Image>();
            playImage.sprite = UiSprite();
            playImage.type = Image.Type.Sliced;
            playImage.color = new Color(1f, 0.84f, 0.25f, 1f);
            var playButton = play.gameObject.AddComponent<Button>();
            playButton.targetGraphic = playImage;
            CreateLabel(play, "PLAY", 64f, TextAlignmentOptions.Center).color = new Color(0.2f, 0.12f, 0.05f);

            return page.gameObject;
        }

        private static GameObject CreateTeamPage(RectTransform parent)
        {
            var page = CreatePageRoot(parent, "TeamPage", SwipePageKind.Team, TeamColor);
            CreateHeader(page.transform, "TEAM", "Power  12");
            CreateTitle(page.transform, "TEAM", new Vector2(0f, 220f), 110f);

            var card = CreateRect("TeamCard", page.transform);
            card.sizeDelta = new Vector2(720f, 360f);
            card.anchoredPosition = new Vector2(0f, -20f);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.sprite = UiSprite();
            cardImage.type = Image.Type.Sliced;
            cardImage.color = new Color(1f, 1f, 1f, 0.16f);
            cardImage.raycastTarget = false;
            CreateLabel(card, "Team placeholder", 48f, TextAlignmentOptions.Center);

            return page.gameObject;
        }

        private static SwipeTabItem[] CreateBottomBar(Transform canvas)
        {
            var bar = CreateRect("BottomBar", canvas);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, BottomBarHeight);
            bar.anchoredPosition = Vector2.zero;

            var barImage = bar.gameObject.AddComponent<Image>();
            barImage.sprite = UiSprite();
            barImage.type = Image.Type.Sliced;
            barImage.color = BarColor;
            barImage.raycastTarget = true;

            var row = CreateRect("Tabs", bar);
            Stretch(row, new Vector2(40f, 16f), new Vector2(-40f, -16f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            return new[]
            {
                CreateTab(row, SwipePageKind.Shop, "SHOP"),
                CreateTab(row, SwipePageKind.Home, "HOME"),
                CreateTab(row, SwipePageKind.Team, "TEAM"),
            };
        }

        private static SwipeTabItem CreateTab(RectTransform parent, SwipePageKind kind, string label)
        {
            var tab = CreateRect($"Tab{kind}", parent);
            var image = tab.gameObject.AddComponent<Image>();
            image.sprite = UiSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 0.08f);
            var button = tab.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var icon = CreateRect("Icon", tab);
            icon.anchorMin = new Vector2(0.5f, 0.58f);
            icon.anchorMax = new Vector2(0.5f, 0.58f);
            icon.sizeDelta = new Vector2(48f, 48f);
            var iconImage = icon.gameObject.AddComponent<Image>();
            iconImage.sprite = UiSprite();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            var text = CreateLabel(tab, label, 28f, TextAlignmentOptions.Center);
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 0.42f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var item = tab.gameObject.AddComponent<SwipeTabItem>();
            var so = new SerializedObject(item);
            so.FindProperty("pageKind").enumValueIndex = (int)kind;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("icon").objectReferenceValue = iconImage;
            so.FindProperty("label").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static RectTransform CreatePageRoot(RectTransform parent, string name, SwipePageKind kind, Color color)
        {
            var page = CreateRect(name, parent);
            Stretch(page);
            var background = page.gameObject.AddComponent<Image>();
            background.sprite = UiSprite();
            background.type = Image.Type.Sliced;
            background.color = color;
            background.raycastTarget = false;

            var swipePage = page.gameObject.AddComponent<SwipePage>();
            var so = new SerializedObject(swipePage);
            so.FindProperty("pageKind").enumValueIndex = (int)kind;
            so.ApplyModifiedPropertiesWithoutUndo();
            return page;
        }

        private static void CreateHeader(Transform parent, string left, string right)
        {
            var header = CreateRect("Header", parent);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, 120f);
            header.anchoredPosition = Vector2.zero;

            var image = header.gameObject.AddComponent<Image>();
            image.sprite = UiSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(0f, 0f, 0f, 0.22f);
            image.raycastTarget = false;

            var leftLabel = CreateLabel(header, left, 36f, TextAlignmentOptions.MidlineLeft);
            leftLabel.rectTransform.offsetMin = new Vector2(48f, 0f);
            leftLabel.rectTransform.offsetMax = new Vector2(-48f, 0f);

            var rightLabel = CreateLabel(header, right, 36f, TextAlignmentOptions.MidlineRight);
            rightLabel.rectTransform.offsetMin = new Vector2(48f, 0f);
            rightLabel.rectTransform.offsetMax = new Vector2(-48f, 0f);
        }

        private static TextMeshProUGUI CreateTitle(Transform parent, string text, Vector2 position, float size)
        {
            var label = CreateLabel(parent, text, size, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1000f, 160f);
            rect.anchoredPosition = position;
            return label;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            var font = LoadTmpFont();
            if (font != null)
                label.font = font;
            return label;
        }

        private static void WireHub(
            SwipePageHub hub,
            RectTransform viewport,
            RectTransform content,
            SwipePage[] pages,
            SwipeTabItem[] tabs)
        {
            var so = new SerializedObject(hub);
            so.FindProperty("viewport").objectReferenceValue = viewport;
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("initialPageIndex").intValue = 1;
            so.FindProperty("pageWidth").floatValue = 1080f;
            so.FindProperty("pageHeight").floatValue = 2160f;

            var pagesProp = so.FindProperty("pages");
            pagesProp.arraySize = pages.Length;
            for (var i = 0; i < pages.Length; i++)
                pagesProp.GetArrayElementAtIndex(i).objectReferenceValue = pages[i];

            var tabsProp = so.FindProperty("tabs");
            tabsProp.arraySize = tabs.Length;
            for (var i = 0; i < tabs.Length; i++)
                tabsProp.GetArrayElementAtIndex(i).objectReferenceValue = tabs[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            var nested = viewport.GetComponentsInChildren<SwipeNestedScrollRect>(true);
            foreach (var scroll in nested)
            {
                scroll.SetHub(hub);
                var scrollSo = new SerializedObject(scroll);
                scrollSo.FindProperty("hub").objectReferenceValue = hub;
                scrollSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static Sprite UiSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static TMP_FontAsset LoadTmpFont()
        {
            var projectFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Resources/Font/VAG Rounded Std Black SDF.asset");
            if (projectFont != null)
                return projectFont;

            var guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
