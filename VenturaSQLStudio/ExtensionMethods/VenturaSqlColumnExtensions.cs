using System;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio
{
    public static class VenturaSqlColumnExtensions
    {

        public static string ShortTypeNameString(this VenturaSqlColumn column)
        {
            var fullTypename = column.FullTypename;

            int index = fullTypename.LastIndexOf('.');

            if (index != -1)
                return fullTypename.Substring(index + 1);

            return fullTypename;
        }

        /// <summary>
        /// The FullTypename is converted into the short name, and if a C# type alias if available.
        /// System.Int32 -> Int32 -> int
        /// </summary>
        public static string ShortTypeNameAsCSharpString(this VenturaSqlColumn column)
        {
            string retvar = ShortTypeNameString(column);

            if (TypeTools.TryConvertToCSharpTypeName(retvar, out string converted) == true)
                retvar = converted;

            return retvar;
        }

        /// <summary>
        /// If IsNullable is set to true (AllowDBNull), and then the ColumnType will be converted
        /// to a Nullable type. Otherwise the original type will be returned.
        /// A value type becomes Nullable by using a generic Nullable<>.
        /// A reference type is Nullable anyways.
        /// </summary>
        private static Type ConvertToColumnPropertyType(this VenturaSqlColumn column)
        {
            Type type = column.ColumnType;

            if (column.IsNullable == false) // The column does not need to be Nullable.
                return type;

            // We need a Nullable type: 1) A reference type or 2) a generic Nullable<>

            // Is it already a reference type? (for example object or string)
            if (TypeTools.IsReferenceTypeNullable(type) == true)
                return type;

            // Is it already a generic Nullable<> type? (for example DateTime?)
            if (TypeTools.IsGenericTypeNullable(type) == true)
                return type;

            type = TypeTools.ConvertValueTypeToGenericNullable(type);

            return type;
        }

        /// <summary>
        /// For C# code generator.
        /// Returns the typename as C# source code.
        /// Will be made a nullable type if IsNullable is set to true.
        /// </summary>
        public static string ShortTypeNameForColumnProperty(this VenturaSqlColumn column)
        {
            bool add_question_mark;

            // Convert to Nullable if IsNullable is true.
            Type type = ConvertToColumnPropertyType(column);

            // Take the short type name. Int32 and not System.Int32.
            string name = type.Name;

            if (TypeTools.IsGenericTypeNullable(type))
            {
                Type under_lying = Nullable.GetUnderlyingType(type);
                name = under_lying.Name; // the short type name
                add_question_mark = true;
            }
            else
            {
                name = type.Name;
                add_question_mark = false;
            }

            if (TypeTools.TryConvertToCSharpTypeName(name, out string csharp_name) == true)
            {
                name = csharp_name;
            }

            if (add_question_mark == true)
                name = name + "?";

            return name;
        }

        /// <summary>
        /// For C# code generator.
        /// Converts ColumnName to valid C# PropertyName.
        /// </summary>
        public static string PropertyName(this VenturaSqlColumn column)
        {
            return TemplateHelper.ConvertToValidIdentifier(column.ColumnName);
        }

        /// <summary>
        /// For C# code generator.
        /// For example "_cur__customer_id"
        /// </summary>
        public static string PrivateVariableName_Current(this VenturaSqlColumn column)
        {
            return "_cur__" + TemplateHelper.ConvertToValidIdentifier(column.ColumnName);
        }

        /// <summary>
        /// For C# code generator.
        /// For example "_ori__customer_id"
        /// </summary>
        public static string PrivateVariableName_Original(this VenturaSqlColumn column)
        {
            return "_ori__" + TemplateHelper.ConvertToValidIdentifier(column.ColumnName);
        }

        /// <summary>
        /// For C# code generator.
        /// For example "_mod__customer_id"
        /// </summary>
        public static string PrivateVariableName_Modified(this VenturaSqlColumn column)
        {
            return "_mod__" + TemplateHelper.ConvertToValidIdentifier(column.ColumnName);
        }

        public static bool ForbidNullValue(this VenturaSqlColumn column)
        {
            if (column.IsNullable == true) // the property allows null.
                return false;

            Type type = ConvertToColumnPropertyType(column);

            // Note that Nullable<> is a value type too.
            // That is why a simple Type.IsValueType check does not work.

            if (TypeTools.IsReferenceTypeNullable(type) == true)
                return true;

            if (TypeTools.IsGenericTypeNullable(type) == true)
                return true;

            // It is a value type
            return false;
        }

        /// <summary>
        /// Returns the column name with the first character removed. 
        /// </summary>
        public static string ColumnNameWithoutPrefix(this VenturaSqlColumn column)
        {
            return column.ColumnName.Substring(1);
        }

        public static TableName AsTableName(this VenturaSqlColumn column)
        {
            return new TableName(column.BaseServerName, column.BaseCatalogName, column.BaseSchemaName, column.BaseTableName);
        }

        /// <summary>
        /// Returns for example: "_cur__birthdate = new DateTime();"
        /// Output depends on IsNullable.
        /// </summary>
        public static string InitToDefaultValueAsSourceCode(this VenturaSqlColumn column)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(PrivateVariableName_Current(column));
            sb.Append(" = ");

            if (column.IsNullable == true)
            {
                sb.Append("null;");

                return sb.ToString();
            }

            var column_type = column.ColumnType;

            if (column_type == typeof(Boolean))
                sb.Append("false;");
            else if (column_type == typeof(Byte))
                sb.Append("0;");
            else if (column_type == typeof(DateTime))
                sb.Append("new DateTime(1900, 1, 1);"); // Don't go lower than 1900
            else if (column_type == typeof(Decimal))
                sb.Append("0.0m;");
            else if (column_type == typeof(Single))
                sb.Append("0.0f;");
            else if (column_type == typeof(Double))
                sb.Append("0.0d;");
            else if (column_type == typeof(Int16))
                sb.Append("0;");
            else if (column_type == typeof(Int32))
                sb.Append("0;");
            else if (column_type == typeof(Int64))
                sb.Append("0;");
            else if (column_type == typeof(String))
                sb.Append("\"\";");
            else if (column_type == typeof(Guid))
                sb.Append("Guid.Empty;");
            else if (column_type == typeof(byte[]))
                sb.Append("new byte[0];");
            else if (column_type == typeof(Object))
                sb.Append("new object();"); // Weird, but at least not null.
            else if (column_type == typeof(TimeSpan))
                sb.Append("new TimeSpan(0);");
            else if (column_type == typeof(DateTimeOffset))
                sb.Append("new DateTimeOffset();");
            else
                throw new InvalidOperationException($"InitToDefaultValueAsSourceCode doesn't know the default non-null value for {column_type.FullName} yet. Please contact support.");

            return sb.ToString();


        }

    } // end of class
}
