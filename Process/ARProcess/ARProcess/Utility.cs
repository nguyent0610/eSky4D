using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using HQ.eSkyFramework;

namespace ARProcess
{
    public static class Utility
    {
        public static void ConvertToEntity<T>(DataRow tableRow, ref T entity, string[] key) where T : new()
        {
            // Create a new type of the entity I want
            Type t = typeof(T);
            T returnObject = entity;

            foreach (DataColumn col in tableRow.Table.Columns)
            {
                string colName = col.ColumnName;
                if (key != null && key.Where(p => p.ToUpper() == colName.ToUpper()).Count() > 0) continue;
                // Look for the object's property with the columns name, ignore case
                PropertyInfo pInfo = t.GetProperty(colName.ToLower(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // did we find the property ?
                if (pInfo != null)
                {
                    object val = tableRow[colName];

                    // is this a Nullable<> type
                    bool IsNullable = (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
                    if (IsNullable)
                    {
                        if (val is System.DBNull)
                        {
                            val = null;
                        }
                        else
                        {
                            // Convert the db type into the T we have in our Nullable<T> type
                            val = Convert.ChangeType
                    (val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                        }
                    }
                    else
                    {
                        // Convert the db type into the type of the property in our entity
                        val = Convert.ChangeType(val, pInfo.PropertyType);
                    }
                    // Set the value of the property with the value from the db
                    pInfo.SetValue(returnObject, val, null);
                }
            }

            // return the entity object with values

        }
        public static string ToAlphaExcel(this int num)
        {
            const int A = 65;    //ASCII value for capital A
            string sCol = string.Empty;
            int iRemain = 0;
            // THIS ALGORITHM ONLY WORKS UP TO ZZ. It fails on AAA
            if (num > 701)
            {
                return string.Empty;
            }
            if (num <= 26)
            {
                if (num == 0)
                {
                    sCol = Convert.ToChar((A + 26) - 1).ToString(); ;
                }
                else
                {
                    sCol = Convert.ToChar((A + num) - 1).ToString();
                }
            }
            else
            {
                iRemain = ((num / 26)) - 1;
                if ((num % 26) == 0)
                {
                    sCol = iRemain.ToAlphaExcel() + (num % 26).ToAlphaExcel();
                }
                else
                {
                    sCol = Convert.ToChar(A + iRemain) + (num % 26).ToAlphaExcel();
                }
            }
            return sCol;

        }
        public static DateTime Short(this  DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day);
        }
        public static string PassNull(this string s)
        {
            if (s == null) return string.Empty;
            return s;
        }
        public static void DeleteTempFile(string fileName)
        {
            try
            {
                System.IO.File.Delete(fileName);
            }
            catch (Exception)
            {
            }
        }
        public static string LastLineRef(int num)
        {
            string lineRef = (num + 1).ToString();
            int len = lineRef.Length;
            for (int i = 0; i < 5 - len; i++)
            {
                lineRef = "0" + lineRef;
            }
            return lineRef;
        }
        public static short Short(this DataRow row, string columnName)
        {
            return (short)row[columnName];
        }
        public static double Double(this DataRow row, string columnName)
        {
            return (double)row[columnName];
        }
        public static string String(this DataRow row, string columnName)
        {
            return row[columnName].ToString();
        }
        public static int Int(this DataRow row, string columnName)
        {
            return (int)row[columnName];
        }
        public static DateTime Date(this DataRow row, string columnName)
        {
            return (DateTime)row[columnName];
        }
        public static bool Bool(this DataRow row, string columnName)
        {
            return (bool)row[columnName];
        }
        public static void AppendLog(List<ProcessException> log, ProcessException ex)
        {
            if (log == null) log = new List<ProcessException>();
            log.Add(ex);
        }
    }

    public static class DataTableHelper
    {
        public static DataTable ConvertTo<T>(IList<T> list)
        {
            DataTable table = CreateTable<T>();
            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item);
                table.Rows.Add(row);
            }
            return table;
        }

        public static IList<T> ConvertTo<T>(IList<DataRow> rows)
        {
            IList<T> list = null;
            if (rows != null)
            {
                list = new List<T>();
                foreach (DataRow row in rows)
                {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }
            return list;
        }

        public static IList<T> ConvertTo<T>(DataTable table)
        {
            if (table == null)
                return null;

            List<DataRow> rows = new List<DataRow>();
            foreach (DataRow row in table.Rows)
                rows.Add(row);

            return ConvertTo<T>(rows);
        }

        public static T CreateItem<T>(DataRow row)
        {
            string columnName;
            T obj = default(T);
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();
                foreach (DataColumn column in row.Table.Columns)
                {
                    columnName = column.ColumnName;
                    //Get property with same columnName
                    PropertyInfo prop = obj.GetType().GetProperty(columnName);
                    try
                    {
                        //Get value for the column
                        object value = (row[columnName].GetType() == typeof(DBNull))
                        ? null : row[columnName];
                        //Set property value
                        prop.SetValue(obj, value, null);
                    }
                    catch
                    {
                        //throw;
                        //Catch whatever here
                    }
                }
            }
            return obj;
        }

        public static DataTable CreateTable<T>()
        {
            Type entityType = typeof(T);
            DataTable table = new DataTable(entityType.Name);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, prop.PropertyType);

            return table;
        }
    }
}

