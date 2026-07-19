#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Dibuja un dropdown para elegir el tipo concreto de un campo [SerializeReference].
    /// Funciona en campos sueltos y en cada elemento de una lista.
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Dibuja el foldout + hijos, y encima el dropdown de tipo en la zona de valor.
            EditorGUI.PropertyField(position, property, label, true);

            Rect popupRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                EditorGUIUtility.singleLineHeight);

            if (EditorGUI.DropdownButton(popupRect, new GUIContent(NiceName(property.managedReferenceFullTypename)), FocusType.Keyboard))
                ShowMenu(property);
        }

        void ShowMenu(SerializedProperty property)
        {
            Type baseType = FieldType(property.managedReferenceFieldTypename);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(None)"), false, () => Assign(property, null));

            if (baseType != null)
            {
                foreach (var t in TypeCache.GetTypesDerivedFrom(baseType)
                             .Where(x => !x.IsAbstract && !x.IsGenericType && x.GetConstructor(Type.EmptyTypes) != null)
                             .OrderBy(x => x.Name))
                {
                    Type captured = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => Assign(property, captured));
                }
            }
            menu.ShowAsContext();
        }

        static void Assign(SerializedProperty property, Type type)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
        }

        static Type FieldType(string managedReferenceFieldTypename)
        {
            var parts = managedReferenceFieldTypename?.Split(' ');
            if (parts == null || parts.Length != 2) return null;
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }

        static string NiceName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return "(elegir tipo)";
            var parts = managedReferenceFullTypename.Split(' ');
            string full = parts.Length == 2 ? parts[1] : managedReferenceFullTypename;
            int dot = full.LastIndexOf('.');
            return dot >= 0 ? full.Substring(dot + 1) : full;
        }
    }
}
#endif
