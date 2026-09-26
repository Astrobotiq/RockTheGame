using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using New_Scripts.NPC;

namespace New_Scripts.Editor
{
    public static class NPCDialogueSetupEditor
    {
        [MenuItem("GameObject/NPC/Setup NPC Dialogue Controller", false, 10)]
        [MenuItem("Tools/NPC/Setup NPC Dialogue Controller")]
        public static void SetupNPCDialogueController()
        {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select an NPC GameObject in the Hierarchy.", "OK");
                return;
            }

            // 1. Add/Get NPCDialogueController
            NPCDialogueController controller = selectedGo.GetComponent<NPCDialogueController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<NPCDialogueController>(selectedGo);
            }

            // 2. Ensure Trigger Collider exists
            Collider2D collider = selectedGo.GetComponent<Collider2D>();
            if (collider == null)
            {
                collider = Undo.AddComponent<BoxCollider2D>(selectedGo);
                collider.isTrigger = true;
            }
            else if (!collider.isTrigger)
            {
                Undo.RecordObject(collider, "Set Collider as Trigger");
                collider.isTrigger = true;
            }

            Debug.Log($"[{selectedGo.name}] NPCDialogueController successfully added! It will automatically bind to the global NPCDialogueBubble at runtime.", selectedGo);
        }

        [MenuItem("GameObject/UI/Create Global Dialogue UI", false, 20)]
        [MenuItem("Tools/NPC/Create Global Dialogue UI")]
        public static void CreateGlobalDialogueUI()
        {
            // Find existing Canvas in the scene
            Canvas targetCanvas = Object.FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                // Create a new Canvas if none exists
                GameObject canvasGo = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create UI Canvas");
                targetCanvas = canvasGo.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Check if NPCDialogueBubble already exists in the scene
            NPCDialogueBubble existingBubble = Object.FindObjectOfType<NPCDialogueBubble>();
            if (existingBubble != null)
            {
                EditorUtility.DisplayDialog("Dialogue UI Exists", $"A global dialogue bubble already exists on GameObject '{existingBubble.gameObject.name}'.", "OK");
                Selection.activeGameObject = existingBubble.gameObject;
                return;
            }

            // Create BubbleContainer under the Canvas
            GameObject bubbleContainerGo = new GameObject("DialogueBubbleUI", typeof(RectTransform), typeof(Image), typeof(NPCDialogueBubble));
            bubbleContainerGo.transform.SetParent(targetCanvas.transform, false);
            Undo.RegisterCreatedObjectUndo(bubbleContainerGo, "Create Global Dialogue UI");

            RectTransform bubbleContainerRect = bubbleContainerGo.GetComponent<RectTransform>();
            // Anchor to Bottom-Center
            bubbleContainerRect.anchorMin = new Vector2(0.5f, 0f);
            bubbleContainerRect.anchorMax = new Vector2(0.5f, 0f);
            bubbleContainerRect.pivot = new Vector2(0.5f, 0f);
            bubbleContainerRect.sizeDelta = new Vector2(800, 150);
            bubbleContainerRect.anchoredPosition = new Vector2(0, 50); // 50 units above bottom edge

            // Setup Image component with a default dark semi-transparent color for mockup
            Image bgImage = bubbleContainerGo.GetComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.85f);

            NPCDialogueBubble bubble = bubbleContainerGo.GetComponent<NPCDialogueBubble>();

            // Create DialogueText under BubbleContainer
            GameObject textGo = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(bubbleContainerGo.transform, false);
            Undo.RegisterCreatedObjectUndo(textGo, "Create Dialogue Text");

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30, 20);
            textRect.offsetMax = new Vector2(-30, -20);

            TextMeshProUGUI tmpText = textGo.GetComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.fontSize = 28;
            tmpText.color = Color.white;
            tmpText.text = "Diyalog yazısı önizlemesi...";

            // Connect references
            SerializedObject bubbleSO = new SerializedObject(bubble);
            SerializedProperty bubbleContainerProp = bubbleSO.FindProperty("bubbleContainer");
            SerializedProperty dialogueTextProp = bubbleSO.FindProperty("dialogueText");

            if (bubbleContainerProp != null)
            {
                bubbleContainerProp.objectReferenceValue = bubbleContainerRect;
            }
            if (dialogueTextProp != null)
            {
                dialogueTextProp.objectReferenceValue = tmpText;
            }
            bubbleSO.ApplyModifiedProperties();

            Selection.activeGameObject = bubbleContainerGo;
            Debug.Log($"[{targetCanvas.name}] Global Dialogue UI successfully created under Canvas!", targetCanvas);
        }

        // Legacy option in case they still want local World Space bubble
        [MenuItem("GameObject/NPC/Setup Dialogue System (Legacy World Space)", false, 30)]
        [MenuItem("Tools/NPC/Setup Dialogue System (Legacy World Space)")]
        public static void SetupLegacyWorldDialogueSystem()
        {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject in the Hierarchy to setup.", "OK");
                return;
            }

            NPCDialogueController controller = selectedGo.GetComponent<NPCDialogueController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<NPCDialogueController>(selectedGo);
            }

            GameObject canvasGo = new GameObject("DialogueCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NPCDialogueBubble));
            canvasGo.transform.SetParent(selectedGo.transform, false);
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Dialogue Canvas");

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            canvasRect.sizeDelta = new Vector2(200, 100);
            canvasRect.anchoredPosition3D = new Vector3(0, 2f, 0);

            NPCDialogueBubble bubble = canvasGo.GetComponent<NPCDialogueBubble>();

            GameObject bubbleContainerGo = new GameObject("BubbleContainer", typeof(RectTransform), typeof(Image));
            bubbleContainerGo.transform.SetParent(canvasGo.transform, false);
            Undo.RegisterCreatedObjectUndo(bubbleContainerGo, "Create Bubble Container");

            RectTransform bubbleContainerRect = bubbleContainerGo.GetComponent<RectTransform>();
            bubbleContainerRect.anchorMin = Vector2.zero;
            bubbleContainerRect.anchorMax = Vector2.one;
            bubbleContainerRect.sizeDelta = Vector2.zero;
            bubbleContainerRect.anchoredPosition = Vector2.zero;

            GameObject textGo = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(bubbleContainerGo.transform, false);
            Undo.RegisterCreatedObjectUndo(textGo, "Create Dialogue Text");

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI tmpText = textGo.GetComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 24;
            tmpText.color = Color.black;
            tmpText.text = "Dialogue text...";

            SerializedObject bubbleSO = new SerializedObject(bubble);
            SerializedProperty bubbleContainerProp = bubbleSO.FindProperty("bubbleContainer");
            SerializedProperty dialogueTextProp = bubbleSO.FindProperty("dialogueText");

            if (bubbleContainerProp != null)
            {
                bubbleContainerProp.objectReferenceValue = bubbleContainerRect;
            }
            if (dialogueTextProp != null)
            {
                dialogueTextProp.objectReferenceValue = tmpText;
            }
            bubbleSO.ApplyModifiedProperties();

            SerializedObject controllerSO = new SerializedObject(controller);
            SerializedProperty dialogueBubbleProp = controllerSO.FindProperty("dialogueBubble");
            if (dialogueBubbleProp != null)
            {
                dialogueBubbleProp.objectReferenceValue = bubble;
            }
            controllerSO.ApplyModifiedProperties();

            Selection.activeGameObject = canvasGo;
        }

        // Validate menu items
        [MenuItem("GameObject/NPC/Setup NPC Dialogue Controller", true)]
        [MenuItem("Tools/NPC/Setup NPC Dialogue Controller", true)]
        [MenuItem("GameObject/UI/Create Global Dialogue UI", true)]
        [MenuItem("Tools/NPC/Create Global Dialogue UI", true)]
        [MenuItem("GameObject/NPC/Setup Dialogue System (Legacy World Space)", true)]
        [MenuItem("Tools/NPC/Setup Dialogue System (Legacy World Space)", true)]
        public static bool ValidateMenuItems()
        {
            return Selection.activeGameObject != null;
        }
    }
}
