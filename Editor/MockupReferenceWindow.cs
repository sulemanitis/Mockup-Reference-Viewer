using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace MockupReference
{
    public sealed class MockupReferenceWindow : EditorWindow
    {
        private const string UxmlPath = "Packages/com.mockupreference.editor/Editor/MockupReferenceWindow.uxml";
        private const string PreferencePrefix = "MockupReference.Images.";
        private const float MinimumZoom = 0.05f;
        private const float MaximumZoom = 4f;

        private readonly List<UnityObject> images = new();

        private ObjectField assetField;
        private ObjectField folderField;
        private Image previewImage;
        private Label emptyLabel;
        private Label positionLabel;
        private Label zoomLabel;
        private Label statusLabel;
        private ScrollView previewScroll;
        private Slider zoomSlider;
        private Button previousButton;
        private Button nextButton;
        private Button pingButton;
        private Button clearButton;

        private int currentIndex;
        private float zoom = 1f;
        private bool fitToWindow = true;

        private static string PreferenceKey => PreferencePrefix + Application.dataPath;

        [MenuItem("Tools/UI Mockup Reference")]
        public static void Open()
        {
            var window = GetWindow<MockupReferenceWindow>("UI Mockups");
            window.minSize = new Vector2(340f, 240f);
        }

        public void CreateGUI()
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (layout == null)
            {
                rootVisualElement.Add(new Label($"Missing package layout: {UxmlPath}"));
                return;
            }

            layout.CloneTree(rootVisualElement);

            assetField = rootVisualElement.Q<ObjectField>("assetField");
            folderField = rootVisualElement.Q<ObjectField>("folderField");
            previewImage = rootVisualElement.Q<Image>("mockupImage");
            emptyLabel = rootVisualElement.Q<Label>("emptyLabel");
            positionLabel = rootVisualElement.Q<Label>("positionLabel");
            zoomLabel = rootVisualElement.Q<Label>("zoomLabel");
            statusLabel = rootVisualElement.Q<Label>("statusLabel");
            previewScroll = rootVisualElement.Q<ScrollView>("previewScroll");
            zoomSlider = rootVisualElement.Q<Slider>("zoomSlider");
            previousButton = rootVisualElement.Q<Button>("previousButton");
            nextButton = rootVisualElement.Q<Button>("nextButton");
            pingButton = rootVisualElement.Q<Button>("pingButton");
            clearButton = rootVisualElement.Q<Button>("clearButton");

            assetField.objectType = typeof(UnityObject);
            assetField.allowSceneObjects = false;
            assetField.RegisterValueChangedCallback(OnAssetChanged);

            folderField.objectType = typeof(DefaultAsset);
            folderField.allowSceneObjects = false;

            rootVisualElement.Q<Button>("addSelectionButton").clicked += AddSelectedImages;
            rootVisualElement.Q<Button>("loadFolderButton").clicked += LoadFolder;
            rootVisualElement.Q<Button>("fitButton").clicked += FitImage;
            rootVisualElement.Q<Button>("actualSizeButton").clicked += () => SetZoom(1f, false);
            previousButton.clicked += () => Move(-1);
            nextButton.clicked += () => Move(1);
            pingButton.clicked += () => EditorGUIUtility.PingObject(CurrentImage);
            clearButton.clicked += ClearImages;

            zoomSlider.lowValue = MinimumZoom;
            zoomSlider.highValue = MaximumZoom;
            zoomSlider.showInputField = true;
            zoomSlider.RegisterValueChangedCallback(evt => SetZoom(evt.newValue, false));

            previewScroll.mode = ScrollViewMode.VerticalAndHorizontal;
            previewScroll.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            previewScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            previewScroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (fitToWindow && CurrentImage != null)
                    FitImage();
            });

            previewImage.scaleMode = ScaleMode.ScaleToFit;
            RestoreImages();
            RefreshPreview();
        }

        private UnityObject CurrentImage => currentIndex >= 0 && currentIndex < images.Count
            ? images[currentIndex]
            : null;

        private void OnAssetChanged(ChangeEvent<UnityObject> evt)
        {
            if (!IsProjectImage(evt.newValue))
            {
                assetField.SetValueWithoutNotify(CurrentImage);
                if (evt.newValue != null)
                    ShowNotification(new GUIContent("Choose a project Texture2D or Sprite."));
                return;
            }

            AddImages(new[] { evt.newValue }, false);
        }

        private void AddSelectedImages()
        {
            int before = images.Count;
            AddImages(Selection.objects, false);
            if (images.Count == before)
                ShowNotification(new GUIContent("Select Texture2D or Sprite assets in the Project window."));
        }

        private void LoadFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(folderField.value);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                ShowNotification(new GUIContent("Choose a folder from the Project window."));
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath })
                .Concat(AssetDatabase.FindAssets("t:Sprite", new[] { folderPath }))
                .Distinct()
                .OrderBy(AssetDatabase.GUIDToAssetPath);

            var folderImages = new List<UnityObject>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityObject image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (image == null)
                    image = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (image != null)
                    folderImages.Add(image);
            }

            if (folderImages.Count == 0)
            {
                ShowNotification(new GUIContent("The folder contains no images."));
                return;
            }

            AddImages(folderImages, true);
        }

        private void AddImages(IEnumerable<UnityObject> candidates, bool replace)
        {
            if (replace)
                images.Clear();

            int firstAddedIndex = -1;
            foreach (UnityObject candidate in candidates)
            {
                if (!IsProjectImage(candidate) || images.Contains(candidate))
                    continue;

                images.Add(candidate);
                if (firstAddedIndex < 0)
                    firstAddedIndex = images.Count - 1;
            }

            if (firstAddedIndex >= 0)
                currentIndex = firstAddedIndex;
            else
                currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, images.Count - 1));

            SaveImages();
            RefreshPreview();
        }

        private static bool IsProjectImage(UnityObject asset)
        {
            return (asset is Texture2D || asset is Sprite) && AssetDatabase.Contains(asset);
        }

        private void Move(int direction)
        {
            if (images.Count == 0)
                return;

            currentIndex = (currentIndex + direction + images.Count) % images.Count;
            RefreshPreview();
        }

        private void ClearImages()
        {
            images.Clear();
            currentIndex = 0;
            SaveImages();
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            UnityObject current = CurrentImage;
            assetField.SetValueWithoutNotify(current);

            bool hasImage = current != null;
            previousButton.SetEnabled(images.Count > 1);
            nextButton.SetEnabled(images.Count > 1);
            pingButton.SetEnabled(hasImage);
            clearButton.SetEnabled(images.Count > 0);
            zoomSlider.SetEnabled(hasImage);
            positionLabel.text = hasImage ? $"{currentIndex + 1} / {images.Count}" : "0 / 0";
            emptyLabel.style.display = hasImage ? DisplayStyle.None : DisplayStyle.Flex;
            previewImage.style.display = hasImage ? DisplayStyle.Flex : DisplayStyle.None;

            previewImage.image = null;
            previewImage.sprite = null;
            if (!hasImage)
            {
                statusLabel.text = "No images loaded";
                return;
            }

            if (current is Sprite sprite)
                previewImage.sprite = sprite;
            else
                previewImage.image = (Texture2D)current;

            Vector2 size = GetImageSize(current);
            statusLabel.text = $"{current.name}  —  {size.x:0} × {size.y:0} px";
            if (fitToWindow)
                rootVisualElement.schedule.Execute(FitImage);
            else
                UpdatePreviewSize();
        }

        private static Vector2 GetImageSize(UnityObject image)
        {
            if (image is Sprite sprite)
                return sprite.rect.size;
            if (image is Texture2D texture)
                return new Vector2(texture.width, texture.height);
            return Vector2.zero;
        }

        private void FitImage()
        {
            UnityObject current = CurrentImage;
            if (current == null)
                return;

            Vector2 size = GetImageSize(current);
            float width = previewScroll.contentViewport.resolvedStyle.width - 24f;
            float height = previewScroll.contentViewport.resolvedStyle.height - 24f;
            if (width <= 0f || height <= 0f || size.x <= 0f || size.y <= 0f)
                return;

            SetZoom(Mathf.Min(width / size.x, height / size.y), true);
        }

        private void SetZoom(float value, bool fit)
        {
            fitToWindow = fit;
            zoom = Mathf.Clamp(value, MinimumZoom, MaximumZoom);
            zoomSlider.SetValueWithoutNotify(zoom);
            zoomLabel.text = $"{zoom * 100f:0}%";
            UpdatePreviewSize();
        }

        private void UpdatePreviewSize()
        {
            if (CurrentImage == null)
                return;

            Vector2 size = GetImageSize(CurrentImage);
            previewImage.style.width = Mathf.Round(size.x * zoom);
            previewImage.style.height = Mathf.Round(size.y * zoom);
        }

        private void SaveImages()
        {
            string ids = string.Join("\n", images.Select(image => GlobalObjectId.GetGlobalObjectIdSlow(image).ToString()));
            EditorPrefs.SetString(PreferenceKey, ids);
        }

        private void RestoreImages()
        {
            images.Clear();
            foreach (string idText in EditorPrefs.GetString(PreferenceKey, string.Empty)
                         .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!GlobalObjectId.TryParse(idText, out GlobalObjectId id))
                    continue;

                UnityObject image = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if (IsProjectImage(image))
                    images.Add(image);
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, images.Count - 1));
        }
    }
}
