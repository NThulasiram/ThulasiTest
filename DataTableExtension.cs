using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace EncompassLibrary.Utilities
{
    /// <summary>
    /// Extension For Data Table
    /// </summary>
    public static class DataTableExtension
    {
        /// <summary>
        /// Converts Passed In Collection to a Datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> list)
        {
            var entityType = typeof(T);
            var dataTable = new DataTable(entityType.Name);
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(entityType);
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
            {
                var propertyType = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType) ?? propertyDescriptor.PropertyType;
                dataTable.Columns.Add(propertyDescriptor.Name, propertyType);
            }
            foreach (T item in list)
            {
                var row = dataTable.NewRow();
                foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
                {
                    var value = propertyDescriptor.GetValue(item);
                    row[propertyDescriptor.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}