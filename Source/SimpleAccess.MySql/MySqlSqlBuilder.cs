﻿using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleAccess.Core;

namespace SimpleAccess.MySql
{

    public class MySqlSqlBuilder : ISqlBuilder<MySqlParameter>
    {
        public IDataParameter[] CreateSqlParametersFromProperties(ParametersType parametersType)
        {
            throw new NotImplementedException();
        }

        public List<PropertyInfo> OutParameterPropertyInfoCollection { get; set; }

        private EntityParameters<MySqlParameter> EntityInsertParameters { get; set; }
        private EntityParameters<MySqlParameter> EntityUpdateParameters { get; set; }

        //public List<IDataParameter> DataParameters { get; set; }

        /// <summary>
        /// Create parameters from object properties
        /// </summary>
        /// <param name="parametersType"></param>
        /// <returns></returns>
        public EntityParameters<MySqlParameter> CreateEntityParameters(object entity, bool checkForIdentityColumn)
        {

            var entityParameters = EntityParameters<MySqlParameter>.Create(entity, (o, dataParameters, outParamsDictionary, checkForIdentity) =>
            {
                var entityType = entity.GetType();
                var propertiesForDataParams = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);

                foreach (var propertyInfo in propertiesForDataParams)
                {
                    CreateDataParameter(propertyInfo, dataParameters, outParamsDictionary, checkForIdentity);
                }

            }, checkForIdentityColumn);

            return entityParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public void CreateDataParameter(PropertyInfo propertyInfo, IDictionary<PropertyInfo, MySqlParameter> dataParameters, IDictionary<PropertyInfo, MySqlParameter> outParamsDictionary, bool checkForIdentity)
        {
            var getMethodInfo = propertyInfo.GetGetMethod();
            if (getMethodInfo.IsVirtual && !getMethodInfo.IsFinal)
                return;

            var sqlParam = new MySqlParameter();

            var attrbutes = propertyInfo.GetCustomAttributes(true);


            var dbColumnAttribute =
                attrbutes.FirstOrDefault(a => a is DbColumnAttribute) as DbColumnAttribute;

            if (dbColumnAttribute == null)
            {
                sqlParam.ParameterName = string.Format("@{0}", propertyInfo.Name);
            }
            else
            {
                sqlParam.ParameterName = string.Format("@{0}", dbColumnAttribute.DbColumn);
            }

            var outParaAttr = attrbutes.FirstOrDefault(a => a is ParameterDirectionAttribute) as ParameterDirectionAttribute;
            if (outParaAttr != null)
            {
                sqlParam.Direction = outParaAttr.SpParameterDirection;
                outParamsDictionary.Add(propertyInfo, sqlParam);
            }


            if ((checkForIdentity)  && attrbutes.FirstOrDefault(a => a is IdentityAttribute) != null)
            {
                sqlParam.Direction = ParameterDirection.InputOutput;
                outParamsDictionary.Add(propertyInfo, sqlParam);
            }

            Debug.WriteLine(sqlParam.ParameterName);
            dataParameters.Add(propertyInfo, sqlParam);
        }


        ///// <summary>
        ///// Create paramters from object properties
        ///// </summary>
        ///// <param name="parametersType"></param>
        ///// <returns></returns>
        //public IDataParameter[] CreateSqlParametersFromProperties(object entity)
        //{
        //    var entityParameters = new EntityParameters();

        //    var procedureType = entity.GetType();
        //    var propertiesForDataParams = procedureType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);

        //    DataParameters =
        //        propertiesForDataParams.Select(propertyInfo => CreateDataParameter(propertyInfo, parametersType, propertiesForDataParams, OutParameterPropertyInfoCollection, OutParameters))
        //            .Where(p => p != null).ToList();

        //    return DataParameters.ToArray();
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="parametesType"></param>
        /// <param name="propertyInfos"></param>
        /// <returns></returns>
        public IDataParameter CreateDataParameter(PropertyInfo propertyInfo, ParametersType parametesType,
            IEnumerable<PropertyInfo> propertyInfos, IList<PropertyInfo> outParameterPropertyInfoCollection, List<IDataParameter> outDataParameters)
        {
            object value = propertyInfo.GetValue(this, new object[] { });

            var sqlParam = new MySqlParameter(string.Format("@{0}", propertyInfo.Name), value);

            if (propertyInfo.PropertyType.Name == "String" && value != null)
            {
                value = SafeSqlLiteral(value.ToString());
            }

            if ((propertyInfo.PropertyType.IsGenericType
                || propertyInfo.PropertyType.Name == "String") & value == null)
            {
                sqlParam.IsNullable = true;
                sqlParam.Value = DBNull.Value;
            }

            var attrbutes = propertyInfo.GetCustomAttributes(true);

            if (propertyInfo.GetGetMethod().IsVirtual)
                return null;

            if (attrbutes.FirstOrDefault(a => a is NotASpParameterAttribute) != null)
                return null;

            
            var dbColumnAttribute =
                attrbutes.FirstOrDefault(a => a is DbColumnAttribute) as DbColumnAttribute;

            if (dbColumnAttribute != null)
            {
                sqlParam.ParameterName = string.Format("@{0}", dbColumnAttribute.DbColumn);
            }



            if (parametesType == ParametersType.Insert)
            {
                //var propertyDataType = propertyInfo.DeclaringType;

                var outParaAttr = attrbutes.FirstOrDefault(a => a is ParameterDirectionAttribute) as ParameterDirectionAttribute;
                if (outParaAttr != null)
                {
                    sqlParam.Direction = outParaAttr.SpParameterDirection;
                    outParameterPropertyInfoCollection.Add(propertyInfo);
                    outDataParameters.Add(sqlParam);
                }

                //if (propertyInfo.PropertyType.GetType() is DateTime
                //    || propertyInfo.PropertyType.GetType() is DateTime?)
                //{
                //    value = value == null || (DateTime)value == DateTime.MinValue ? new DateTime(2000, 1, 1) : value;                       
                //}
            }

            Debug.WriteLine(sqlParam.ParameterName);
            return sqlParam;
        }

        public string GetSelectAllStatement()
        {
            throw new NotImplementedException();
        }

        public string GetInsertStatement()
        {
            throw new NotImplementedException();
        }

        public string GetUpdateSatetment()
        {
            throw new NotImplementedException();
        }

        public string GetDeleteStatment()
        {
            throw new NotImplementedException();
        }

        public EntityParameters<MySqlParameter> GetInsertParameters(object entity)
        {
            EntityInsertParameters = EntityInsertParameters ?? CreateEntityParameters(entity, true);

            EntityInsertParameters.FillParameters(entity, FillInsertParameters);

            return EntityInsertParameters;

        }

        public void FillInsertParameters(object entity, IDictionary<PropertyInfo, MySqlParameter> parameters)
        {

            foreach (var dataParameter in parameters)
            {
                var propertyInfo = dataParameter.Key;
                var sqlParam = dataParameter.Value;

                object value = propertyInfo.GetValue(entity, new object[] {});


                if (propertyInfo.PropertyType.Name == "String" && value != null)
                {
                    value = SafeSqlLiteral(value.ToString());
                }

                if ((propertyInfo.PropertyType.IsGenericType
                    || propertyInfo.PropertyType.Name == "String") & value == null)
                {
                    ((MySqlParameter) sqlParam).IsNullable = true;
                    sqlParam.Value = DBNull.Value;
                    continue;
                }

                sqlParam.Value = value;
                Debug.WriteLine("Filling MySqlParameter: {0}", sqlParam.ParameterName);
            }


        }

        public EntityParameters<MySqlParameter> GetUpdateParameters(object entity)
        {
            EntityUpdateParameters = EntityUpdateParameters ?? CreateEntityParameters(entity, false);

            EntityUpdateParameters.FillParameters(entity, FillInsertParameters);

            return EntityUpdateParameters;
        }


        public string BuildWhereExpression(string propertyName, Type valueType, string @operator, object value)
        {
            var result = "";
            if (value == null && @operator == "=")
            {
                return string.Format(" {0} is null", propertyName);
            }
            if (@operator == "EndsWith")
            {
                return string.Format(" {0} LIKE '%{1}'", propertyName, SafeSqlLiteral(value.ToString()));
            }
            if (@operator == "StartsWith")
            {
                return string.Format(" {0} LIKE '{1}%'", propertyName, SafeSqlLiteral(value.ToString()));
            }
            if (@operator == "Contains")
            {
                return string.Format(" {0} LIKE '%{1}%'", propertyName, SafeSqlLiteral(value.ToString()));
            }
            if (valueType == typeof(bool))
            {
                result = string.Format(" {0} {1} {2} ", propertyName, @operator, (bool)value ? "1" : "0");
            }
            else if (In(valueType, typeof(string), typeof(TimeSpan), typeof(TimeSpan?), typeof(DateTime),
                typeof(DateTime?)))
            {
                result = string.Format("{0} {1} '{2}' ", propertyName, @operator, SafeSqlLiteral(value.ToString()));
            }
            else if (In(valueType, typeof(Int16), typeof(Int16?), typeof(int), typeof(int?), typeof(Int32), typeof(Int32?), typeof(Int64), typeof(Int64?), typeof(Single), typeof(Single?),
                                typeof(float), typeof(float?), typeof(decimal), typeof(decimal?), typeof(double), typeof(double?)))
            {
                result = string.Format("{0} {1} {2} ", propertyName, @operator, value.ToString());
            }
            else
            {
                throw new ArgumentException($"Invalid augument type {valueType.Name}");
            }

            return result;
        }

        /// <summary>
        /// Compare the value with multiple value.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="source">Any type</param>
        /// <param name="list">Multiple values or array of the same type</param>
        /// <returns>Returns a bool value indicates criteria matched or not</returns>
        /// <example>
        /// Example shows how to use In function
        /// <code>
        /// <![CDATA[
        ///     var names = new [] { "Salman", "Jameel", "Kareem", "Kashif" };
        ///     if ("Kareem".In(names))
        ///     {
        ///         Console.Write("Kareem is in names");
        ///     }
        /// ]]>
        /// </code>
        /// </example>
        public static bool In<T>(T source, params T[] list)
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return list.Contains(source);
        }

        /// <summary>
        /// Clear all DbParamters
        /// </summary>
        public void ClearDbParameters()
        {

            EntityInsertParameters = null;
            EntityUpdateParameters = null;

        }

        public void LoadOutParametersProperties(object instance)
        {
            throw new NotImplementedException();
        }

        private string SafeSqlLiteral(string inputSql)
        {
            return inputSql.Replace("'", "''");
        }

    }


}