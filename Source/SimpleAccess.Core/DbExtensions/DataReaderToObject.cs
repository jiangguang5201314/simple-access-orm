﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using SimpleAccess.Core.Entity;

namespace SimpleAccess.Core
{
    /// <summary>
    /// Extension to load objects from DataReaders
    /// </summary>
    public static class DataReaderToObjectExtensions
    {
        public static Dictionary<string, Dictionary<string, PropertyInfo>> EntityProperties { get; set; }
        public static Dictionary<string, Dictionary<string, PropertyInfo>> EntityDbColumnProperties { get; set; }

        static DataReaderToObjectExtensions()
        {
            EntityProperties = new Dictionary<string, Dictionary<string, PropertyInfo>>();
            EntityDbColumnProperties = new Dictionary<string, Dictionary<string, PropertyInfo>>();
        }

        /// <summary>
        /// Creates a list of a given type from all the rows in a DataReader.
        /// 
        /// Note this method uses Reflection so this isn't a high performance
        /// operation, but it can be useful for generic data reader to entity
        /// conversions on the fly and with anonymous types.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="reader">An open DataReader that's in position to read</param>
        /// <param name="fieldsToSkip">Optional - comma delimited list of fields that you don't want to update</param>
        /// <param name="piList">
        /// Optional - Cached PropertyInfo dictionary that holds property info data for this object.
        /// Can be used for caching hte PropertyInfo structure for multiple operations to speed up
        /// translation. If not passed automatically created.
        /// </param>
        /// <param name="piListBasedOnDbColumn"> List of <see cref="PropertyInfo"/> object having <see cref="DbColumnAttribute"/> in it's custom attributes</param>
        /// <returns></returns>
        public static List<TType> DataReaderToObjectList<TType>(this IDataReader reader, string fieldsToSkip = null
            , Dictionary<string, PropertyInfo> piList = null, Dictionary<string, PropertyInfo> piListBasedOnDbColumn = null)
            where TType : new()
        {
            if (reader == null)
                return null;

            var items = new List<TType>();
            var entityFullName = typeof(TType).FullName;
            // Create lookup list of property info objects            
            if (piList == null)
            {
                piList = GetEntityPerpertiesInfoList<TType>(entityFullName);
            }

            if (piListBasedOnDbColumn == null)
            {
                piListBasedOnDbColumn = GetEntityDbColumnPropertiesInfoList(entityFullName, piList);
            }

            while (reader.Read())
            {
                var inst = new TType();
                DataReaderToObject<TType>(reader, inst, fieldsToSkip, piList, piListBasedOnDbColumn);
                items.Add(inst);
            }

            return items;
        }

        private static Dictionary<string, PropertyInfo> GetEntityDbColumnPropertiesInfoList(string entityFullName, Dictionary<string, PropertyInfo> piList)
        {

            if (EntityDbColumnProperties.ContainsKey(entityFullName))
                return EntityDbColumnProperties[entityFullName];

            var piListBasedOnDbColumn = new Dictionary<string, PropertyInfo>();
            foreach (var prop in piList.Values)
            {
                var dbColumnAttribute =
                            prop.GetCustomAttributes(false).FirstOrDefault(a =>
                                a is DbColumnAttribute) as DbColumnAttribute;
                if (dbColumnAttribute != null)
                    piListBasedOnDbColumn.Add(dbColumnAttribute.DbColumn.ToLower(), prop);
            }

            EntityDbColumnProperties.Add(entityFullName, piListBasedOnDbColumn);

            return piListBasedOnDbColumn;
        }

        private static Dictionary<string, PropertyInfo> GetEntityPerpertiesInfoList<TType>(string entityFullName) where TType : new()
        {
            if (EntityProperties.ContainsKey(entityFullName))
                return EntityProperties[entityFullName];
            
            var piList = new Dictionary<string, PropertyInfo>();

            var props = typeof(TType).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
                piList.Add(prop.Name.ToLower(), prop);

            EntityProperties.Add(entityFullName, piList);

            return piList;
        }


        /// <summary>
        /// Created the object of TType and populates the properties of that object from a single DataReader row using
        /// Reflection by matching the DataReader fields to a public property 
        /// of the object. Unmatched properties are left unchanged.
        /// 
        /// You need to pass in a data reader located on the active row you want
        /// to serialize.
        /// 
        /// </summary>
        /// <param name="reader">Instance of the DataReader to read data from. Should be located on the correct record (Read() should have been called on it before calling this method)</param>
        /// <param name="fieldsToSkip">Optional - A comma delimited list of object properties that should not be updated</param>
        /// <param name="piList">Optional - Cached PropertyInfo dictionary that holds property info data for this object</param>
        /// <param name="piListBasedOnDbColumn"> List of <see cref="PropertyInfo"/> object having <see cref="DbColumnAttribute"/> in it's custom attributes</param>

        public static TType DataReaderToObject<TType>(this IDataReader reader, string fieldsToSkip = null
            , Dictionary<string, PropertyInfo> piList = null, Dictionary<string, PropertyInfo> piListBasedOnDbColumn = null)
            where TType : new()
        {

            TType item = default(TType);
            if (reader == null)
                return item;

            item = new TType();

            var entityFullName = typeof(TType).FullName;
            // Create lookup list of property info objects            
            if (piList == null)
            {
                piList = GetEntityPerpertiesInfoList<TType>(entityFullName);
            }

            if (piListBasedOnDbColumn == null)
            {
                piListBasedOnDbColumn = GetEntityDbColumnPropertiesInfoList(entityFullName, piList);
            }

            if (reader.Read())
            {
                DataReaderToObject<TType>(reader, item, fieldsToSkip, piList, piListBasedOnDbColumn);
            }


            return item;
        }


        /// <summary>
        /// Populates the properties of an object from a single DataReader row using
        /// Reflection by matching the DataReader fields to a public property on
        /// the object passed in. Unmatched properties are left unchanged.
        /// 
        /// You need to pass in a data reader located on the active row you want
        /// to serialize.
        /// 
        /// This routine works best for matching pure data entities and should
        /// be used only in low volume environments where retrieval speed is not
        /// critical due to its use of Reflection.
        /// </summary>
        /// <param name="reader">Instance of the DataReader to read data from. Should be located on the correct record (Read() should have been called on it before calling this method)</param>
        /// <param name="instance">Instance of the object to populate properties on</param>
        /// <param name="fieldsToSkip">Optional - A comma delimited list of object properties that should not be updated</param>
        /// <param name="piList">Optional - Cached PropertyInfo dictionary that holds property info data for this object</param>
        /// <param name="piListBasedOnDbColumn"> List of <see cref="PropertyInfo"/> object having <see cref="DbColumnAttribute"/> in it's custom attributes</param>
        public static void DataReaderToObject<TEntity>(this IDataReader reader, TEntity instance, string fieldsToSkip = null
            , Dictionary<string, PropertyInfo> piList = null, Dictionary<string, PropertyInfo> piListBasedOnDbColumn = null)
        {
            if (reader.IsClosed)
                throw new InvalidOperationException("Connection is closed.");

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;
            else
                fieldsToSkip = "," + fieldsToSkip + ",";

            fieldsToSkip = fieldsToSkip.ToLower();

            // create a dictionary of properties to look up
            // we can pass this in so we can cache the list once 
            // for a list operation 

            //var entityFullName = typeof(TType).FullName;
            //// Create lookup list of property info objects            
            //if (piList == null)
            //{
            //    piList = GetEntityPerpertiesInfoList<TType>(entityFullName);
            //}

            //if (piListBasedOnDbColumn == null)
            //{
            //    piListBasedOnDbColumn = GetEntityDbColumnPropertiesInfoList(entityFullName, piList);
            //}

            for (int index = 0; index < reader.FieldCount; index++)
            {
                string name = reader.GetName(index).ToLower();

                if (piList.ContainsKey(name))
                {

#if DEBUG
                    System.Diagnostics.Debug.WriteLine("loading value for : " + name);

#endif
                    var prop = piList[name];

                    if (fieldsToSkip.Contains("," + name + ","))
                        continue;

                    //System.Diagnostics.Debug.WriteLine(prop.Name);
                    if ((prop != null) && prop.CanWrite)
                    {
                        try
                        {
                            //var dbColumnPropertyAttribute =
                            //    prop.GetCustomAttributes(false).FirstOrDefault(a =>
                            //        a is DbColumnPropertyAttribute) as DbColumnPropertyAttribute;


                            //if (dbColumnPropertyAttribute != null)
                            //    prop = piList[dbColumnPropertyAttribute.DbColumnProperty.ToLower()];

                            var val = reader.GetValue(index);
                            
                            if (prop.PropertyType.IsGenericType)
                            {
                                if (prop.PropertyType.GetGenericArguments()[0].IsEnum)
                                {
                                    prop.SetValue(instance, (val == DBNull.Value)
                                        ? null
                                        : Enum.Parse(prop.PropertyType.GetGenericArguments()[0],
                                            val.ToString())
                                        , null);
                                    continue;
                                }
                            }
                            /*
                            if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(instance, (val == DBNull.Value)
                                                            ? null
                                                            : Enum.Parse(prop.PropertyType.GetGenericArguments()[0],
                                                                         val.ToString())
                                              , null);
                                continue;
                            }*/


                            prop.SetValue(instance, (val == DBNull.Value) ? null : val, null);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Error in assigning value of {0}. Exception: {1}", prop.Name, ex.Message));
                        }
                    }
                }


                // searching for DbColumn attribute
                if (piListBasedOnDbColumn == null)
                    return;

                if (piListBasedOnDbColumn.ContainsKey(name))
                {
                    var prop = piListBasedOnDbColumn[name];

                    if (fieldsToSkip.Contains("," + name + ","))
                        continue;

                    //System.Diagnostics.Debug.WriteLine(prop.Name);
                    if ((prop != null) && prop.CanWrite)
                    {
                        try
                        {
                            var val = reader.GetValue(index);

                            if (prop.PropertyType.IsGenericType)
                            {
                                if (prop.PropertyType.GetGenericArguments()[0].IsEnum)
                                {
                                    prop.SetValue(instance, (val == DBNull.Value)
                                                                ? null
                                                                : Enum.Parse(prop.PropertyType.GetGenericArguments()[0],
                                                                             val.ToString())
                                                  , null);
                                    continue;
                                }
                            }
                            /*
                            if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(instance, (val == DBNull.Value)
                                                            ? null
                                                            : Enum.Parse(prop.PropertyType.GetGenericArguments()[0],
                                                                         val.ToString())
                                              , null);
                                continue;
                            }*/
                            prop.SetValue(instance, (val == DBNull.Value) ? null : val, null);
                        }
                        catch (Exception ex)
                        {

                            throw new Exception(string.Format("Error in assigning the value of {0}. Exception: {1}", prop.Name, ex.Message), ex);
                        }
                    }
                }
            }
        }
 
    }
}
