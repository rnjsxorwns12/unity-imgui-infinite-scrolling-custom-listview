//https://github.com/rnjsxorwns12/unity-infinite-scrolling-custom-listview
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
[ExecuteInEditMode]
public class InfiniteScrollingCustomListView : MonoBehaviour
{
    [Serializable]
    class Position
    {
        [Range(0, 1)]
        public float left;
        [Range(0, 1)]
        public float right;
        [Range(0, 1)]
        public float top;
        [Range(0, 1)]
        public float bottom;
        public Position(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
        public Position() : this(0, 0, 0, 0) { }
    }
    [Serializable]
    class Element
    {
        public enum ElementType
        {
            Text,
            Image,
            ImageButton
        }
        public enum CompareType {
            String,
            Int,
            Float
        }
        public enum FloatCompare
        {
            Greater,
            Less
        }
        public enum IntCompare
        {
            Greater,
            Less,
            Equal,
            NotEqual
        }
        public enum StringCompare
        {
            IsNull,
            IsNotNull,
            Equal,
            NotEqual
        }
        public bool init;
        public Rect rect;
        public ElementType elementType;
        public bool useCondition;
        public string conditionField;
        public FloatCompare floatCompare;
        public StringCompare stringCompare;
        public IntCompare intCompare;
        public string stringCompareValue;
        public int intCompareValue;
        public float floatCompareValue;
        public TextAnchor textAnchor;
        public FontStyle fontStyle;
        public Font font;
        public Color fontColor;
        public bool directInput;
        public string textField;
        public Texture image;
        public Texture activeImage;
        public GameObject onClick;
        public string function;
    }
    [CustomPropertyDrawer(typeof(Element))]
    class ElementProperty : PropertyDrawer
    {
        private void FieldBindingToList(ref List<FieldInfo> fieldInfoList, ref List<string> fieldNameList, Type type)
        {
            FieldBindingToList(new List<Type>(), string.Empty, ref fieldInfoList, ref fieldNameList, type);
        }
        private void FieldBindingToList(List<Type> avoidTypes, string fieldPath, ref List<FieldInfo> fieldInfoList, ref List<string> fieldNameList, Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<Type> currentLevelClassTypes = new List<Type>();
            for(int i = 0; i < fieldInfos.Length; i++)
            {
                Type t = fieldInfos[i].FieldType;
                if (t.IsClass && !avoidTypes.Contains(t) && !currentLevelClassTypes.Contains(t)) currentLevelClassTypes.Add(t);
            }
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                Type t = fieldInfo.FieldType;
                if (t == typeof(int) || t == typeof(float) || t == typeof(string))
                {
                    fieldInfoList.Add(fieldInfo);
                    fieldNameList.Add(string.Concat(fieldPath, fieldInfo.Name));
                }
                else if (t.IsClass && !avoidTypes.Contains(t))
                {
                    List<Type> newAvoidTypes = avoidTypes;
                    if (currentLevelClassTypes.Count > 0)
                    {
                        newAvoidTypes = new List<Type>(avoidTypes);
                        newAvoidTypes.AddRange(currentLevelClassTypes);
                    }
                    FieldBindingToList(newAvoidTypes, string.Concat(fieldPath, fieldInfo.Name, "."), ref fieldInfoList, ref fieldNameList, t);
                }
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty init = property.FindPropertyRelative("init");
            if (!init.boolValue)
            {
                SerializedProperty rect = property.FindPropertyRelative("rect");
                rect.FindPropertyRelative("width").floatValue = 1;
                rect.FindPropertyRelative("height").floatValue = 1;
                property.FindPropertyRelative("fontColor").colorValue = Color.white;
                property.FindPropertyRelative("textAnchor").enumValueIndex = (int)TextAnchor.MiddleCenter;
                init.boolValue = true;
            }
            int indent = EditorGUI.indentLevel;
            position.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
            {
                SerializedProperty itemClass = property.serializedObject.FindProperty("itemClass");
                MonoScript itemClassScript = (MonoScript)itemClass.objectReferenceValue;
                List<FieldInfo> fieldInfos = new List<FieldInfo>();
                List<string> fieldNames = new List<string>();
                position.y += position.height * 1.2f;
                if (itemClassScript != null)
                {
                    FieldBindingToList(ref fieldInfos, ref fieldNames, itemClassScript.GetClass());
                    SerializedProperty useCondition = property.FindPropertyRelative("useCondition");
                    if (useCondition.boolValue = EditorGUI.ToggleLeft(position, useCondition.displayName, useCondition.boolValue, EditorStyles.boldLabel))
                    {
                        position.y += position.height * 1.2f;
                        EditorGUI.indentLevel++;
                        Rect r = new Rect(position);
                        r.width /= 3;
                        SerializedProperty conditionField = property.FindPropertyRelative("conditionField");
                        int conditionFieldIndex = EditorGUI.Popup(r, fieldNames.IndexOf(conditionField.stringValue), fieldNames.ToArray());
                        EditorGUI.indentLevel--;
                        conditionField.stringValue = conditionFieldIndex < 0 ? string.Empty : fieldNames[conditionFieldIndex];
                        if (conditionFieldIndex >= 0)
                        {
                            Type t = fieldInfos[conditionFieldIndex].FieldType;
                            r.x += r.width;
                            string s = t == typeof(int) ? "intCompare" : (t == typeof(float) ? "floatCompare" : "stringCompare");
                            SerializedProperty enumProperty = property.FindPropertyRelative(s);
                            EditorGUI.PropertyField(r, enumProperty, GUIContent.none);
                            if(t != typeof(string) || enumProperty.enumValueIndex == (int)Element.StringCompare.Equal || enumProperty.enumValueIndex == (int)Element.StringCompare.NotEqual)
                            {
                                r.x += r.width;
                                SerializedProperty enumValueProperty = property.FindPropertyRelative(string.Concat(s, "Value"));
                                EditorGUI.PropertyField(r, enumValueProperty, GUIContent.none);
                            }
                        }
                    }
                    position.y += position.height * 1.2f;
                }
                EditorGUI.LabelField(new Rect(position.x, position.y, 80, position.height), "Rect", EditorStyles.boldLabel);
                position.y += position.height * 1.2f;
                EditorGUI.indentLevel++;
                SerializedProperty rect = property.FindPropertyRelative("rect");
                EditorGUI.PropertyField(position, rect, GUIContent.none);
                SerializedProperty rectX = rect.FindPropertyRelative("x");
                SerializedProperty rectY = rect.FindPropertyRelative("y");
                SerializedProperty rectWidth = rect.FindPropertyRelative("width");
                SerializedProperty rectHeight = rect.FindPropertyRelative("height");
                rectX.floatValue = Mathf.Clamp01(rectX.floatValue);
                rectY.floatValue = Mathf.Clamp01(rectY.floatValue);
                rectWidth.floatValue = Mathf.Clamp01(rectWidth.floatValue);
                rectHeight.floatValue = Mathf.Clamp01(rectHeight.floatValue);
                position.y += position.height * 2.4f;
                EditorGUI.indentLevel--;
                EditorGUI.LabelField(new Rect(position.x, position.y, 80, position.height), "Content", EditorStyles.boldLabel);
                position.y += position.height * 1.2f;
                EditorGUI.indentLevel++;
                SerializedProperty elementTypeProperty = property.FindPropertyRelative("elementType");
                EditorGUI.PropertyField(position, elementTypeProperty, new GUIContent("Type"));
                position.y += position.height * 1.2f;
                if (elementTypeProperty.enumValueIndex == (int)Element.ElementType.Text)
                {
                    Rect r = new Rect(position);
                    r.width /= 2;
                    SerializedProperty textField = property.FindPropertyRelative("textField");
                    SerializedProperty directInput = property.FindPropertyRelative("directInput");
                    directInput.boolValue = EditorGUI.ToggleLeft(r, directInput.displayName, itemClassScript == null || directInput.boolValue);
                    r.x += r.width;
                    if (directInput.boolValue) EditorGUI.PropertyField(r, textField, GUIContent.none);
                    else
                    {
                        int textFieldIndex = EditorGUI.Popup(r, fieldNames.IndexOf(textField.stringValue), fieldNames.ToArray());
                        textField.stringValue = textFieldIndex < 0 ? string.Empty : fieldNames[textFieldIndex];
                    }
                    position.y += position.height * 1.2f;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("textAnchor"));
                    position.y += position.height * 1.2f;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("font"));
                    position.y += position.height * 1.2f;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("fontColor"));
                    position.y += position.height * 1.2f;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("fontStyle"));
                }
                else
                {
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("image"));
                    if (elementTypeProperty.enumValueIndex == (int)Element.ElementType.ImageButton)
                    {
                        position.y += position.height * 1.2f;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative("activeImage"));
                        position.y += position.height * 1.2f;
                        SerializedProperty onClick = property.FindPropertyRelative("onClick");
                        EditorGUI.PropertyField(position, onClick);
                        if (onClick.objectReferenceValue != null)
                        {
                            position.y += position.height * 1.2f;
                            MonoBehaviour[] monoBehaviours = ((GameObject)onClick.objectReferenceValue).GetComponents<MonoBehaviour>();
                            List<string> methodNames = new List<string>();
                            for (int i = 0; i < monoBehaviours.Length; i++)
                            {
                                MethodInfo[] methods = monoBehaviours[i].GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                                for (int j = 0; j < methods.Length; j++)
                                {
                                    ParameterInfo[] parameters = methods[j].GetParameters();
                                    if (methods[j].ReturnType != typeof(void)) continue;
                                    else if (parameters.Length != 1 || parameters[0].ParameterType != typeof(int)) continue;
                                    methodNames.Add(string.Concat(methods[j].DeclaringType.Name, ".", methods[j].Name));
                                }
                            }
                            SerializedProperty function = property.FindPropertyRelative("function");
                            int functionIndex = EditorGUI.Popup(position, function.displayName, methodNames.IndexOf(function.stringValue), methodNames.ToArray());
                            function.stringValue = functionIndex < 0 ? string.Empty : methodNames[functionIndex];
                        }
                    }
                }
            }
            EditorGUI.indentLevel = indent;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int itemCount = 1;
            if (property.isExpanded)
            {
                if (property.serializedObject.FindProperty("itemClass").objectReferenceValue != null)
                {
                    itemCount++;
                    if (property.FindPropertyRelative("useCondition").boolValue) itemCount++;
                }
                itemCount += 5;
                int elementTypeEnumValue = property.FindPropertyRelative("elementType").enumValueIndex;
                if (elementTypeEnumValue == (int)Element.ElementType.Text) itemCount += 5;
                else
                {
                    itemCount++;
                    if (elementTypeEnumValue == (int)Element.ElementType.ImageButton)
                    {
                        itemCount += 2;
                        if (property.FindPropertyRelative("onClick").objectReferenceValue != null) itemCount++;
                    }
                }
            }
            return EditorGUIUtility.singleLineHeight * 1.2f * itemCount;
        }
    }
    public static InfiniteScrollingCustomListView Instance;
    [SerializeField]
    private Texture background;
    [SerializeField]
    private Position margin = new Position();
    [SerializeField]
    private Position padding = new Position();
    [Header("Item"), SerializeField, Range(0.01f, 1)]
    private float itemHeight = 0.1f;
    [SerializeField, Range(0, 1)]
    private float itemInterval = 0.01f;
    [SerializeField]
    private MonoScript itemClass;
    [SerializeField]
    private Element[] itemElements;
    private float drawPosition;
    private bool touchButton;
    private Vector2 touchPos;
    private Vector2 touchPosOrigin;
    private float touchTime;
    private float touchSpeed;
    private float touchSpeedOrigin;
    private delegate void OnClick(int index);
    [HideInInspector]
    public IList List;
    private float DrawPosition
    {
        set
        {
            int drawSize = 1;
            if (List != null) drawSize = Mathf.Max(drawSize, List.Count);
            float limit = Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top) * (itemHeight + itemInterval) * drawSize;
            value %= limit;
            if (value < 0) value += limit;
            drawPosition = value;
        }
    }

    private void Awake()
    {
        Instance = this;
        List = itemClass == null ? null : (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemClass.GetClass()));
    }

    private object GetFieldValue(out Type type, string field, object obj)
    {
        type = null;
        if (obj == null || string.IsNullOrEmpty(field)) return null;
        string[] cls = field.Split('.');
        for (int i = 0; i < cls.Length; i++)
        {
            FieldInfo objField = obj.GetType().GetField(cls[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (objField == null) return null;
            obj = objField.GetValue(obj);
            if (i + 1 < cls.Length && obj == null) return null;
            else if (i == cls.Length - 1) type = objField.FieldType;
        }
        return obj;
    }

    void OnGUI()
    {
        Rect bg = new Rect(
            Screen.width * margin.left,
            Screen.height * margin.top,
            Screen.width * Mathf.Max(0, 1 - margin.right - margin.left),
            Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top)
        );
        if (bg.width == 0 || bg.height == 0) return;
        if (background != null) GUI.DrawTexture(bg, background);
        Rect rect = new Rect(
            bg.width * padding.left,
            bg.height * padding.top,
            bg.width * Mathf.Max(0, 1 - padding.right - padding.left),
            bg.height * Mathf.Max(0, 1 - padding.bottom - padding.top)
        );
        if (rect.width == 0 || rect.height == 0) return;
        GUI.BeginGroup(bg);
        GUI.BeginGroup(rect);
        float iHeight = bg.height * itemHeight;
        float iTotalHeight = iHeight + bg.height * itemInterval;
        float currY = drawPosition + iTotalHeight / 2 - bg.height * 0.5f + rect.y;
        int currIndex = (int)((drawPosition + iTotalHeight / 2) / iTotalHeight);
        GUIStyle[] guiStyles = new GUIStyle[itemElements.Length];
        OnClick[] onClicks = new OnClick[itemElements.Length];
        for (int i = 0; itemElements != null && i < itemElements.Length; i++)
        {
            Element element = itemElements[i];
            guiStyles[i] = null;
            onClicks[i] = null;
            if (element.elementType == Element.ElementType.Text)
            {
                guiStyles[i] = new GUIStyle();
                guiStyles[i].alignment = element.textAnchor;
                guiStyles[i].fontStyle = element.fontStyle;
                guiStyles[i].font = element.font;
                guiStyles[i].normal.textColor = element.fontColor;
                guiStyles[i].fontSize = (int)(iHeight * Mathf.Min(element.rect.height, 1 - element.rect.y));
            }
            else if (element.elementType == Element.ElementType.ImageButton)
            {
                if (element.onClick != null && !string.IsNullOrEmpty(element.function))
                {
                    string[] split = element.function.Split('.');
                    if (split.Length == 2)
                    {
                        Component component = element.onClick.GetComponent(split[0]);
                        if(component != null)
                        {
                            MethodInfo methodInfo = component.GetType().GetMethod(split[1]);
                            if (methodInfo != null) onClicks[i] = (OnClick)methodInfo.CreateDelegate(typeof(OnClick), component);
                        }
                    }
                }
            }
        }
        for (int i = -Mathf.CeilToInt((currIndex * iTotalHeight - currY) / iTotalHeight); i <= Mathf.CeilToInt((currY + rect.height - (currIndex + 1) * iTotalHeight) / iTotalHeight) ; i++)
        {
            Rect itemRect = new Rect(0, (currIndex + i) * iTotalHeight - currY + (iTotalHeight - iHeight) / 2, rect.width, iHeight);
            GUI.BeginGroup(itemRect);
            int objIndex = GetIndexInRange(currIndex + i);
            for (int j = 0; itemElements != null && j < itemElements.Length; j++)
            {
                Element element = itemElements[j];
                Rect elementRect = new Rect(
                    itemRect.width * element.rect.x,
                    itemRect.height * element.rect.y,
                    itemRect.width * Mathf.Min(element.rect.width, 1 - element.rect.x),
                    itemRect.height * Mathf.Min(element.rect.height, 1 - element.rect.y)
                );
                if (element.useCondition && objIndex >= 0)
                {
                    Type conditionFieldType;
                    object conditionField = GetFieldValue(out conditionFieldType, element.conditionField, List[objIndex]);
                    bool condition = false;
                    if (conditionField != null)
                    {
                        if (typeof(int) == conditionFieldType)
                        {
                            if (element.intCompare == Element.IntCompare.Greater) condition = element.intCompareValue < (int)conditionField;
                            else if (element.intCompare == Element.IntCompare.Less) condition = element.intCompareValue > (int)conditionField;
                            else if (element.intCompare == Element.IntCompare.Equal) condition = element.intCompareValue == (int)conditionField;
                            else if (element.intCompare == Element.IntCompare.NotEqual) condition = element.intCompareValue != (int)conditionField;
                        }
                        else if (typeof(float) == conditionFieldType)
                        {
                            if (element.floatCompare == Element.FloatCompare.Greater) condition = element.floatCompareValue < (float)conditionField;
                            else if (element.floatCompare == Element.FloatCompare.Less) condition = element.floatCompareValue > (float)conditionField;
                        }
                    }
                    else if (typeof(string) == conditionFieldType)
                    {
                        if (element.stringCompare == Element.StringCompare.IsNull) condition = conditionField == null;
                        else if (element.stringCompare == Element.StringCompare.IsNotNull) condition = conditionField != null;
                        else if (element.stringCompare == Element.StringCompare.Equal) condition = conditionField != null && ((string)conditionField).Equals(element.stringCompareValue);
                        else if (element.stringCompare == Element.StringCompare.NotEqual) condition = conditionField != null && !((string)conditionField).Equals(element.stringCompareValue);
                    }
                    if (!condition) continue; 
                }
                if (element.elementType == Element.ElementType.Text)
                {
                    string text = element.directInput ? element.textField : (objIndex >= 0 ? ((string)GetFieldValue(out Type type, element.textField, List[objIndex]) ?? string.Empty) : string.Empty);
                    float textWidth = guiStyles[j].CalcSize(new GUIContent(text)).x;
                    if (elementRect.width < textWidth)
                    {
                        GUI.BeginGroup(elementRect);
                        GUI.Label(new Rect(-(Screen.width * 0.1f * Time.time % textWidth), 0, textWidth, elementRect.height), text, guiStyles[j]);
                        GUI.EndGroup();
                    }
                    else GUI.Label(elementRect, text, guiStyles[j]);
                }
                else
                {
                    Texture image = element.image;
                    if (element.elementType == Element.ElementType.ImageButton)
                    {
                        Rect elementScreenRect = new Rect(bg.x + rect.x + itemRect.x + elementRect.x,
                            bg.y + rect.y + itemRect.y + elementRect.y,
                            elementRect.width,
                            elementRect.height
                        );
                        float rectStart = bg.y + rect.y;
                        float rectEnd = rectStart + rect.height;
                        if (elementScreenRect.y < rectStart)
                        {
                            elementScreenRect.height -= rectStart - elementScreenRect.y;
                            elementScreenRect.y = rectStart;
                        }
                        if (elementScreenRect.y + elementScreenRect.height > rectEnd) elementScreenRect.height -= elementScreenRect.y + elementScreenRect.height - rectEnd;
                        if (elementScreenRect.width > 0 && elementScreenRect.height > 0)
                        {
                            if (Touch(elementScreenRect, touchPosOrigin) && Touch(elementScreenRect, touchPos))
                            {
                                if (element.activeImage != null) image = element.activeImage;
                                if (touchId >= 0) touchButton = true;
                            }
                            if (GUI.Button(elementRect, GUIContent.none, GUIStyle.none) && onClicks[j] != null && objIndex >= 0) onClicks[j](objIndex);
                        }
                    }
                    if (image != null) GUI.DrawTexture(elementRect, image);
                }
            }
            GUI.EndGroup();
        }
        GUI.EndGroup();
        GUI.EndGroup();
    }
    private int GetIndexInRange(int indexOutOfRange)
    {
        int index = -1;
        if (List != null && List.Count > 0)
        {
            index = indexOutOfRange % List.Count;
            if (index < 0) index += List.Count;
        }
        return index;
    }
    private int touchId = -1;
    private bool Touch(Rect rect, Vector3 touchPos)
    {
        return rect.x < touchPos.x && touchPos.x < rect.x + rect.width && rect.y <  Screen.height - touchPos.y && Screen.height - touchPos.y < rect.y + rect.height;
    }
    private void Update()
    {
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS)
        if (Input.GetMouseButtonDown(0) && Touch(
            new Rect(
                Screen.width * margin.left,
                Screen.height * margin.top,
                Screen.width * Mathf.Max(0, 1 - margin.right - margin.left),
                Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top)
            ),
            Input.mousePosition))
        {
            touchId = 0;
            touchPosOrigin = touchPos = Input.mousePosition;
            touchTime = Time.deltaTime;
            touchSpeedOrigin = touchSpeed = 0;
        }
        else if (touchId == 0)
        {
            if (Input.GetMouseButton(0))
            {
                float mouseDeltaPos = Input.mousePosition.y - touchPos.y;
                touchPos = Input.mousePosition;
                if (!touchButton)
                {
                    DrawPosition = drawPosition + mouseDeltaPos;
                    if (touchSpeed == 0 || (touchSpeed > 0 && mouseDeltaPos > 0) || (touchSpeed < 0 && mouseDeltaPos < 0))
                    {
                        touchSpeed += mouseDeltaPos;
                        touchTime += Time.deltaTime;
                    }
                    else
                    {
                        touchSpeed = mouseDeltaPos;
                        touchTime = Time.deltaTime;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                touchId = -1;
                touchPosOrigin = touchPos = Vector2.zero;
                if (touchButton)
                {
                    touchSpeedOrigin = touchSpeed = 0;
                    touchButton = false;
                }
                else
                {
                    touchSpeed /= touchTime;
                    touchSpeedOrigin = touchSpeed;
                }
                touchTime = 0;
            }
        }
#else
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began && Touch(
                new Rect(
                    Screen.width * margin.left,
                    Screen.height * margin.top,
                    Screen.width * Mathf.Max(0, 1 - margin.right - margin.left),
                    Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top)
                ),
                t.position))
            {
                touchId = t.fingerId;
                touchPosOrigin = touchPos = t.position;
                touchTime = Time.deltaTime;
                touchSpeedOrigin = touchSpeed = 0;
            }
            else if (touchId == t.fingerId)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    touchId = -1;
                    touchPosOrigin = touchPos = Vector2.zero;
                    if (touchButton)
                    {
                        touchSpeedOrigin = touchSpeed = 0;
                        touchButton = false;
                    }
                    else
                    {
                        touchSpeed /= touchTime;
                        touchSpeedOrigin = touchSpeed;
                    }
                    touchTime = 0;
                }
                else
                {
                    float mouseDeltaPos = t.position.y - touchPos.y;
                    touchPos = t.position;
                    if (!touchButton)
                    {
                        DrawPosition = drawPosition + mouseDeltaPos;
                        if (touchSpeed == 0 || (touchSpeed > 0 && mouseDeltaPos > 0) || (touchSpeed < 0 && mouseDeltaPos < 0))
                        {
                            touchSpeed += mouseDeltaPos;
                            touchTime += Time.deltaTime;
                        }
                        else
                        {
                            touchSpeed = mouseDeltaPos;
                            touchTime = Time.deltaTime;
                        }
                    }
                }
            }
        }
#endif
        if (touchSpeedOrigin != 0)
        {
            DrawPosition = drawPosition + touchSpeed * Time.deltaTime;
            float mouseSpeedTemp = touchSpeed - touchSpeedOrigin * Time.deltaTime;
            if (touchSpeed < 0) touchSpeed = Mathf.Min(0, mouseSpeedTemp);
            else if (touchSpeed > 0) touchSpeed = Mathf.Max(0, mouseSpeedTemp);
            else touchSpeedOrigin = 0;
        }
    }
}
