#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Reflection;
	using UnityEngine;

	/// <summary>
	/// Class field or property info.
	/// </summary>
	[Serializable]
	public class ClassField : IFormattable
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public string FieldName;

		/// <summary>
		/// Field type name.
		/// </summary>
		public string FieldTypeName;

		/// <summary>
		/// Field type.
		/// </summary>
		protected Type fieldType;

		/// <summary>
		/// Field type.
		/// </summary>
		public Type FieldType => fieldType;

		Type actualFieldType;

		/// <summary>
		/// Actual field type. Match FieldType excepts types like R3.SerializableReactiveProperty&lt;T&gt; where it is T type.
		/// </summary>
		public Type ActualFieldType
		{
			get => actualFieldType;

			protected set => actualFieldType = value;
		}

		/// <summary>
		/// Is field readable?
		/// </summary>
		public bool CanRead = true;

		/// <summary>
		/// Is field writable?
		/// </summary>
		public bool CanWrite = true;

		/// <summary>
		/// Widget class.
		/// </summary>
		public Type WidgetClass;

		/// <summary>
		/// Widget name to display the current field.
		/// </summary>
		public string WidgetFieldName;

		/// <summary>
		/// Widget field to set value.
		/// </summary>
		public string WidgetValueField;

		/// <summary>
		/// Field format.
		/// </summary>
		public string FieldFormat = string.Empty;

		/// <summary>
		/// Is text field?
		/// </summary>
		public bool IsText
		{
			get;
			protected set;
		}

		/// <summary>
		/// Is field implements IComparable?
		/// </summary>
		public bool IsComparableNonGeneric
		{
			get;
			protected set;
		}

		/// <summary>
		/// Is field implements IComparable{T}?
		/// </summary>
		public bool IsComparableGeneric
		{
			get;
			protected set;
		}

		/// <summary>
		/// Is Image field?
		/// </summary>
		public bool IsImage
		{
			get;
			protected set;
		}

		/// <summary>
		/// Is field can be null?
		/// </summary>
		public bool IsNullable
		{
			get;
			protected set;
		}

		/// <summary>
		/// Field can be used in autocomplete widgets.
		/// </summary>
		public bool AllowAutocomplete => ActualFieldType == typeof(string);

		/// <summary>
		/// Field to access value in wrapper type (like Value).
		/// </summary>
		public string WrapperField = string.Empty;

		/// <summary>
		/// Constructor for the wrapper type. Must include namespace.
		/// </summary>
		public string WrapperTypeConstructor = string.Empty;

		/// <summary>
		/// Attributes.
		/// </summary>
		[NonSerialized]
		protected List<Attribute> Attributes;

		static Type[] PrintableTypes => EditorStaticFields.Instance.WidgetsGeneratorPrintableTypes;

		static Type[] ImageTypes => EditorStaticFields.Instance.WidgetsGeneratorImageTypes;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassField"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="type">Type.</param>
		/// <param name="attributes">Attributes.</param>
		/// <param name="canRead">Can read field.</param>
		/// <param name="canWrite">Can write field.</param>
		protected ClassField(string name, Type type, IEnumerable<Attribute> attributes, bool canRead = true, bool canWrite = true)
		{
			FieldName = name;
			WidgetFieldName = name;
			FieldTypeName = UtilitiesEditor.GetFriendlyTypeName(type);
			fieldType = type;
			Attributes = new List<Attribute>(attributes);
			CanRead = canRead;
			CanWrite = canWrite;

			SetWidgetByType(fieldType);
		}

		/// <summary>
		/// Set widget field data by type.
		/// </summary>
		/// <param name="type">Type.</param>
		protected void SetWidgetByType(Type type)
		{
			ActualFieldType = type;

			foreach (var wrapper in EditorStaticFields.Instance.WidgetsGeneratorWrapperTypes)
			{
				if (DetectGenericType(type, wrapper.Key, out var type_args))
				{
					var info = wrapper.Value;
					ActualFieldType = type_args[info.TypeIndex];
					WrapperField = info.Field;
					WrapperTypeConstructor = info.Constructor(ActualFieldType);
					CanWrite &= info.CanCreate;
					break;
				}
			}

			if (Array.IndexOf(ImageTypes, ActualFieldType) != -1)
			{
				if (ActualFieldType == typeof(Sprite))
				{
					WidgetClass = typeof(UnityEngine.UI.Image);
					WidgetValueField = "sprite";

					IsText = false;
					IsImage = true;
					IsNullable = true;
				}
				else if ((ActualFieldType == typeof(Texture2D)) || (ActualFieldType == typeof(Texture)))
				{
					WidgetClass = typeof(UnityEngine.UI.RawImage);
					WidgetValueField = "texture";

					IsText = false;
					IsImage = true;
					IsNullable = true;
				}
				else if ((ActualFieldType == typeof(Color)) || (ActualFieldType == typeof(Color32)))
				{
					WidgetClass = typeof(UnityEngine.UI.Image);
					WidgetValueField = "color";

					IsText = false;
					IsImage = true;
					IsNullable = false;
				}
			}
			else
			{
				WidgetClass = typeof(TextAdapter);
				if (FieldName == "TextAdapter")
				{
					WidgetFieldName = FieldName + "Field";
				}
				else if (FieldName == "Text")
				{
					WidgetFieldName = "TextAdapter";
				}

				WidgetValueField = "text";

				if (ActualFieldType != typeof(string))
				{
					FieldFormat = ".ToString()";
				}

				#if UIWIDGETS_UNITY_LOCALIZATION_SUPPORT
				if (ActualFieldType == typeof(UnityEngine.Localization.LocalizedString))
				{
					FieldFormat = "?.GetLocalizedString()";
				}
				#endif

				var comparable_type = typeof(IComparable<>);
				var generic_comparable_type = comparable_type.MakeGenericType(new[] { ActualFieldType });

				IsText = true;
				IsComparableGeneric = generic_comparable_type.IsAssignableFrom(ActualFieldType);
				IsComparableNonGeneric = typeof(IComparable).IsAssignableFrom(ActualFieldType);
				IsImage = false;
				IsNullable = true;
			}

			// rename widget field name if such field already exists
			if (RequireRename(WidgetFieldName, ActualFieldType, IsText) || HasDefinition(WidgetFieldName))
			{
				WidgetFieldName += "Field";
			}
		}

		static bool RequireRename(string widget, Type type, bool isText)
		{
			if (widget == "Icon" && type != typeof(Sprite))
			{
				return true;
			}

			if (widget == "TextAdapter" && !isText)
			{
				return true;
			}

			return false;
		}

		static bool DetectGenericType(Type type, Type genericType, out Type[] typeArguments)
		{
			while (type != null)
			{
				if (type.IsConstructedGenericType && (type.GetGenericTypeDefinition() == genericType))
				{
					typeArguments = type.GenericTypeArguments;
					return true;
				}

				type = type.BaseType;
			}

			typeArguments = null;
			return false;
		}

		/// <summary>
		/// Check is the specified field, property or method already exists.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>true if specified field, property or method exists; otherwise false.</returns>
		protected static bool HasDefinition(string name)
		{
			if (EditorStaticFields.Instance.WidgetsGeneratorProhibitedNames == null)
			{
				EditorStaticFields.Instance.WidgetsGeneratorProhibitedNames = new HashSet<string>();
				var prohibited = EditorStaticFields.Instance.WidgetsGeneratorProhibitedNames;

				var type = typeof(TreeViewComponentBase<>);
				var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
				foreach (var field in type.GetFields(flags))
				{
					prohibited.Add(field.Name);
				}

				foreach (var property in type.GetProperties(flags))
				{
					prohibited.Add(property.Name);
				}

				foreach (var method in type.GetMethods(flags))
				{
					prohibited.Add(method.Name);
				}

				// special cases for the TreeViewComponent
				prohibited.Remove("Icon");
				prohibited.Remove("TextAdapter");
			}

			return EditorStaticFields.Instance.WidgetsGeneratorProhibitedNames.Contains(name);
		}

		/// <summary>
		/// Is type can used to create ClassField?
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>True if type is allowed; otherwise false.</returns>
		protected static bool IsAllowedType(Type type)
		{
			if (type.IsPrimitive || Array.IndexOf(PrintableTypes, type) != -1 || Array.IndexOf(ImageTypes, type) != -1)
			{
				return true;
			}

			if (type.GetMethod("ToString", Type.EmptyTypes)?.DeclaringType == typeof(UnityEngine.Object))
			{
				return false;
			}

			if (typeof(IFormattable).IsAssignableFrom(type))
			{
				return true;
			}

			if (type.GetMethod("ToString")?.DeclaringType != typeof(object))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Create ClassField from specified field.
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>ClassField instance if field type is allowed; otherwise null.</returns>
		public static ClassField Create(FieldInfo field)
		{
			if (field.IsLiteral)
			{
				return null;
			}

			if (!IsAllowedType(field.FieldType))
			{
				return null;
			}

			return new ClassField(field.Name, field.FieldType, field.GetCustomAttributes());
		}

		/// <summary>
		/// Create ClassField from specified property.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <returns>ClassField instance if property type is allowed; otherwise null.</returns>
		public static ClassField Create(PropertyInfo property)
		{
			var can_read = property.CanRead && property.GetGetMethod() != null;
			if (!can_read)
			{
				return null;
			}

			if (!IsAllowedType(property.PropertyType))
			{
				return null;
			}

			return new ClassField(property.Name, property.PropertyType, property.GetCustomAttributes(), can_read, property.CanWrite && property.GetSetMethod() != null);
		}

		/// <summary>
		/// Check if field has attribute of the specified type.
		/// </summary>
		/// <param name="type">Attribute type.</param>
		/// <returns>true if field has attribute of the specified type; otherwise false.</returns>
		public bool HasAttribute(Type type)
		{
			foreach (var attr in Attributes)
			{
				if (attr.GetType() == type)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return format switch
			{
				"FieldName" => FieldName,
				"FieldType" => (FieldType == typeof(Texture2D))
										? UtilitiesEditor.GetFriendlyTypeName(typeof(Texture))
										: UtilitiesEditor.GetFriendlyTypeName(FieldType),
				"WidgetFieldName" => WidgetFieldName,
				"WidgetClass" => UtilitiesEditor.GetFriendlyTypeName(WidgetClass),
				"WidgetValueField" => WidgetValueField,
				"FieldFormat" => FieldFormat,
				"WrapperField" => WrapperField,
				"WrapperTypeConstructor" => WrapperTypeConstructor,
				_ => throw new ArgumentOutOfRangeException("Unsupported format: " + format),
			};
		}
	}
}
#endif