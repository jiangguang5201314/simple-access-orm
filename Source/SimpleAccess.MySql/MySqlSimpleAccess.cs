﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Dynamic;
using System.Reflection;
using SimpleAccess.Core;
using SimpleAccess.Core.Logger;

namespace SimpleAccess.MySql
{
    /// <summary>
    /// SimpleAccess implementation for MySql.
    /// </summary>
    public class MySqlSimpleAccess : IMySqlSimpleAccess
    {
        private const string DefaultConnectionStringKey = "simpleAccess:mysqlConnectionStringName";

        /// <summary>
        /// Default connection string.
        /// </summary>
        public static string DefaultConnectionString { get; set; }

        /// <summary>
        /// SimpleLogger to log exception
        /// </summary>
        public ISimpleLogger SimpleLogger { get { return DefaultSimpleAccessSettings.DefaultLogger; } }

        /// <summary>
        /// Default settings for simple access
        /// </summary>
        public SimpleAccessSettings DefaultSimpleAccessSettings { get; set; }

        /// <summary> The SQL connection. </summary>
        private readonly MySqlConnection _sqlConnection;

		/// <summary> The SQL transaction. </summary>

        private MySqlTransaction _transaction;


        #region Constructors

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="sqlConnection"> The SQL connection. </param>

        public MySqlSimpleAccess(MySqlConnection sqlConnection)
        {
            DefaultSimpleAccessSettings = new SimpleAccessSettings
            {
                DefaultCommandType = CommandType.Text, DefaultLogger = new SimpleLogger()
            };
            _sqlConnection = sqlConnection;
        }

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="sqlConnection"> The SQL connection. </param>
        /// <param name="defaultCommandType"> The default command type for all queries </param>

        public MySqlSimpleAccess(MySqlConnection sqlConnection, CommandType defaultCommandType)
        {
            DefaultSimpleAccessSettings = new SimpleAccessSettings (defaultCommandType );
            _sqlConnection = sqlConnection;
        }

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="sqlConnection"> The SQL connection. </param>
        /// <param name="defaultSimpleAccessSettings"> The default settings for simple access </param>

        public MySqlSimpleAccess(MySqlConnection sqlConnection, SimpleAccessSettings defaultSimpleAccessSettings)
        {
            DefaultSimpleAccessSettings = defaultSimpleAccessSettings;
            _sqlConnection = sqlConnection;
        }

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="connection"> The ConnectionString Name from the config file or a complete ConnectionString . </param>
        public MySqlSimpleAccess(string connection)
            : this(new MySqlConnection(SimpleAccessSettings.GetProperConnectionString(connection)))
        {
        }

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="connection"> The ConnectionString Name from the config file or a complete ConnectionString . </param>
        /// <param name="defaultCommandType"> The default command type for all queries </param>
        public MySqlSimpleAccess(string connection, CommandType defaultCommandType)
            : this(new MySqlConnection(SimpleAccessSettings.GetProperConnectionString(connection)), defaultCommandType)
        {
        }

        /// <summary> Constructor. </summary>
        /// 
        /// <param name="connection"> The ConnectionString Name from the config file or a complete ConnectionString . </param>
        /// <param name="defaultSimpleAccessSettings"> The default settings for simple access </param>
        public MySqlSimpleAccess(string connection, SimpleAccessSettings defaultSimpleAccessSettings)
            : this(new MySqlConnection(SimpleAccessSettings.GetProperConnectionString(connection)), defaultSimpleAccessSettings)
        {
        }

        /// <summary> Default constructor. </summary>
        public MySqlSimpleAccess()
            : this(new MySqlConnection(DefaultConnectionString))
        {
        }

        /// <summary> Default constructor. </summary>
        /// <param name="defaultCommandType"> The default command type for all queries </param>
        public MySqlSimpleAccess(CommandType defaultCommandType)
            : this(new MySqlConnection(DefaultConnectionString), defaultCommandType)
        {
        }


        /// <summary> Default constructor. </summary>
        /// <param name="defaultSimpleAccessSettings"> The default settings for simple access </param>
        public MySqlSimpleAccess(SimpleAccessSettings defaultSimpleAccessSettings)
            : this(new MySqlConnection(DefaultConnectionString), defaultSimpleAccessSettings)
        {
        }
        /// <summary>
        /// Static constructor to load default connection string from default configuration file
        /// </summary>
        static MySqlSimpleAccess()
        {
            var connectionStringName = ConfigurationManager.AppSettings[DefaultConnectionStringKey];
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings != null)
            {
                DefaultConnectionString = connectionStringSettings.ConnectionString;
            }
        }
        #endregion


        /// <summary> Executes the non query operation. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters"> Options for controlling the SQL. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteNonQuery(commandText, DefaultSimpleAccessSettings.DefaultCommandType, mySqlParameters);

        }

        /// <summary> Executes the non query operation. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters"> Options for controlling the SQL. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(string commandText, CommandType commandType,
            params MySqlParameter[] mySqlParameters)
        {
            int result;
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                result = dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
            return result;
        }

        /// <summary> Executes the non query operation. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(string commandText, object paramObject = null)
        {
            return ExecuteNonQuery(commandText, DefaultSimpleAccessSettings.DefaultCommandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the non query operation. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(string commandText, CommandType commandType,
            object paramObject = null)
        {
            return ExecuteNonQuery(commandText, commandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes a command text against the connection and returns the number of rows affected. 
        /// </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(MySqlTransaction transaction, string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteNonQuery(transaction, commandText,  DefaultSimpleAccessSettings.DefaultCommandType, mySqlParameters);
        }

        /// <summary> Executes a command text against the connection and returns the number of rows affected. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="parameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(MySqlTransaction transaction, string commandText,
            CommandType commandType, params MySqlParameter[] parameters)
        {
            int result;

            try
            {
                var dbCommand = CreateCommand(transaction, commandText, commandType, parameters);
                dbCommand.Connection.OpenSafely();
                result = dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            return result;

        }

        /// <summary> Executes a command text against the connection and returns the number of rows affected. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>        
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(MySqlTransaction transaction, string commandText, object paramObject = null)
        {
            return ExecuteNonQuery(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes a command text against the connection and returns the number of rows affected. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>        
        /// <returns> Number of rows affected (integer) </returns>
        public int ExecuteNonQuery(MySqlTransaction transaction, string commandText,
            CommandType commandType, object paramObject = null)
        {
            return ExecuteNonQuery(transaction, commandText, commandType, BuildSqlParameters(paramObject));

        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public T ExecuteScalar<T>(string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteScalar<T>(commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , mySqlParameters);
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(string commandText, CommandType commandType, params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.Open();
                var result = dbCommand.ExecuteScalar();

                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(string commandText, object paramObject = null)
        {
            return ExecuteScalar<T>(commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(string commandText, CommandType commandType, object paramObject = null)
        {
            return ExecuteScalar<T>(commandText, commandType
                , BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(MySqlTransaction transaction, string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteScalar<T>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , mySqlParameters);
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public T ExecuteScalar<T>(MySqlTransaction transaction, string commandText,
            CommandType commandType, params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(transaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                var result = dbCommand.ExecuteScalar();

                return (T) Convert.ChangeType(result, typeof (T));
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        ///  <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(MySqlTransaction transaction, string commandText, object paramObject = null)
        {
            return ExecuteScalar<T>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        ///  <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The {T} value </returns>
        public T ExecuteScalar<T>(MySqlTransaction transaction, string commandText,
            CommandType commandType, object paramObject = null)
        {
            return ExecuteScalar<T>(transaction, commandText, commandType
                , BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteReader(commandText, DefaultSimpleAccessSettings.DefaultCommandType, mySqlParameters);
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandType commandType,
            params MySqlParameter[] mySqlParameters)
        {
            return ExecuteReader(commandText, commandType, CommandBehavior.Default, mySqlParameters);
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandBehavior"> The CommandBehavior of executing DbCommand</param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandBehavior commandBehavior,
            params MySqlParameter[] mySqlParameters)
        {
            return ExecuteReader(commandText, DefaultSimpleAccessSettings.DefaultCommandType, commandBehavior, mySqlParameters);
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="commandBehavior"> The CommandBehavior of executing DbCommand</param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandType commandType, CommandBehavior commandBehavior,
            params MySqlParameter[] mySqlParameters)
        {
            try
            {
                var dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                var result = dbCommand.ExecuteReader(commandBehavior);
                dbCommand.Parameters.Clear();
                return result;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, object paramObject = null)
        {
            return ExecuteReader(commandText, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandType commandType, object paramObject = null)
        {
            return ExecuteReader(commandText, commandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandBehavior"> The CommandBehavior of executing DbCommand</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandBehavior commandBehavior, object paramObject = null)
        {
            return ExecuteReader(commandText, commandBehavior, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the commandText and return TDbDataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="commandBehavior"> The CommandBehavior of executing DbCommand</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The TDbDataReader </returns>
        public MySqlDataReader ExecuteReader(string commandText, CommandType commandType, CommandBehavior commandBehavior, object paramObject = null)
        {
            return ExecuteReader(commandText, commandType, commandBehavior, BuildSqlParameters(paramObject));
        }




        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(string commandText, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteValues<T>(commandText, DefaultSimpleAccessSettings.DefaultCommandType, BuildSqlParameters(mySqlParameters));
        }


        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(string commandText, object paramObject = null)
        {
            return ExecuteValues<T>(commandText, DefaultSimpleAccessSettings.DefaultCommandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(string commandText, CommandType commandType, object paramObject = null)
        {
            return ExecuteValues<T>(commandText, commandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///     
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(string commandText, CommandType commandType, params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader())
                {
                    return GetValues<T>(reader);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> value </returns>
        public IEnumerable<T> ExecuteValues<T>(MySqlTransaction transaction, string commandText, object paramObject = null)
        {
            return ExecuteValues<T>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{T}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> value </returns>
        public IEnumerable<T> ExecuteValues<T>(MySqlTransaction transaction, string commandText, CommandType commandType, object paramObject = null)
        {
            return ExecuteValues<T>(transaction, commandText, commandType, BuildSqlParameters(paramObject));
        }
        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="DbException"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(MySqlTransaction transaction, string commandText,
                                             params MySqlParameter[] mySqlParameters)
        {
            return ExecuteValues<T>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType, mySqlParameters);
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="MySqlTransaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{T}" /> </returns>
        public IEnumerable<T> ExecuteValues<T>(MySqlTransaction MySqlTransaction, string commandText, CommandType commandType,
            params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(MySqlTransaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader())
                {
                    return GetValues<T>(reader);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                dbCommand.ClearDbCommand();

            }
        }



        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The TEntity value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(string commandText, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null, params MySqlParameter[] mySqlParameters) 
            where TEntity : new()
        {
            return ExecuteEntities<TEntity>(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip
                , propertyInfoDictionary, mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///     
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(string commandText, CommandType commandType,
            string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null
            , params MySqlParameter[] mySqlParameters) where TEntity : new()
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader())
                {
                    return reader.DataReaderToObjectList<TEntity>(fieldsToSkip, propertyInfoDictionary);
                }
                //return dbCommand.ExecuteReader().DataReaderToObjectList<TEntity>(fieldsToSkip, piList);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        ///  <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The {TEntity} value </returns>

        public IEnumerable<TEntity> ExecuteEntities<TEntity>(string commandText, object paramObject = null, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null)
            where TEntity : new()
        {
            return ExecuteEntities<TEntity>(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip
                , propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(string commandText, CommandType commandType, object paramObject = null,
            string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null) 
            where TEntity : new()
        {
            return ExecuteEntities<TEntity>(commandText, commandType, fieldsToSkip
                , propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="DbException"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(MySqlTransaction transaction, string commandText, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null, params MySqlParameter[] mySqlParameters)
            where TEntity : new()
        {
            return ExecuteEntities<TEntity>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , fieldsToSkip, propertyInfoDictionary, mySqlParameters);
        }

        /// <summary> Executes the command text, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Generic type parameter. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The {TEntity} value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(MySqlTransaction transaction, string commandText,
            CommandType commandType, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null, params MySqlParameter[] mySqlParameters) where TEntity : new()
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(transaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader())
                {
                    return reader.DataReaderToObjectList<TEntity>(fieldsToSkip, propertyInfoDictionary);
                }
                //return dbCommand.ExecuteReader().DataReaderToObjectList<TEntity>(fieldsToSkip, piList);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{TEntity}" /> value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(MySqlTransaction transaction, string commandText, object paramObject = null
            , string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null) where TEntity : new()
        {
            return ExecuteEntities<TEntity>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType
                , fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a <see cref="IEnumerable{TEntity}" /> from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The <see cref="IEnumerable{TEntity}" /> value </returns>
        public IEnumerable<TEntity> ExecuteEntities<TEntity>(MySqlTransaction transaction, string commandText,
            CommandType commandType, object paramObject = null, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null) where TEntity : new()
        {
            return ExecuteEntities<TEntity>(transaction, commandText, commandType
                , fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(string commandText, string fieldsToSkip = null
            , Dictionary<string, PropertyInfo> propertyInfoDictionary = null,
            params MySqlParameter[] mySqlParameters) where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, propertyInfoDictionary, mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(string commandText, CommandType commandType, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null, params MySqlParameter[] mySqlParameters) where TEntity : class, new()
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.HasRows ? reader.DataReaderToObject<TEntity>(fieldsToSkip, propertyInfoDictionary) : null;
                }
                //return dbCommand.ExecuteReader().DataReaderToObject<TEntity>(fieldsToSkip, piList);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(string commandText, object paramObject = null, string fieldsToSkip = null
            , Dictionary<string, PropertyInfo> propertyInfoDictionary = null)
            where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		 (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(string commandText, CommandType commandType, object paramObject = null, 
            string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null) 
            where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(commandText, commandType,
                fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(MySqlTransaction transaction, string commandText, string fieldsToSkip = null,
            Dictionary<string, PropertyInfo> propertyInfoDictionary = null, params MySqlParameter[] mySqlParameters) where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, propertyInfoDictionary, mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(MySqlTransaction transaction, string commandText, CommandType commandType,
            string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null
            , params MySqlParameter[] mySqlParameters) where TEntity : class, new()
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(transaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                using (var reader = dbCommand.ExecuteReader())
                {
                    return reader.HasRows ? reader.DataReaderToObject<TEntity>(fieldsToSkip, propertyInfoDictionary) : null;
                }
                //return dbCommand.ExecuteReader().DataReaderToObject<TEntity>(fieldsToSkip, piList);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(MySqlTransaction transaction, string commandText, object paramObject = null, 
            string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null) where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a TEntity from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <typeparam name="TEntity"> Type of the entity. </typeparam>
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="propertyInfoDictionary">		  (optional) dictionary of property name and PropertyInfo object. </param>
        /// 
        /// <returns> The value of the entity. </returns>
        public TEntity ExecuteEntity<TEntity>(MySqlTransaction transaction, string commandText, CommandType commandType, 
            object paramObject = null, string fieldsToSkip = null, Dictionary<string, PropertyInfo> propertyInfoDictionary = null) 
            where TEntity : class, new()
        {
            return ExecuteEntity<TEntity>(transaction, commandText, commandType,
                fieldsToSkip, propertyInfoDictionary, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(string commandText, string fieldsToSkip = null, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteDynamics(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(string commandText, CommandType commandType, string fieldsToSkip = null,
            params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                return GetDynamicSqlData(dbCommand.ExecuteReader());
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        ///  
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///  
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        ///  
        ///  <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(string commandText, object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamics(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        ///  
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///  
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        ///  
        ///  <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(string commandText, CommandType commandType, object paramObject = null,
            string fieldsToSkip = null)
        {
            return ExecuteDynamics(commandText, commandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(MySqlTransaction transaction, string commandText, string fieldsToSkip = null,
            params MySqlParameter[] mySqlParameters)
        {
            return ExecuteDynamics(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(MySqlTransaction transaction, string commandText, CommandType commandType,
            string fieldsToSkip = null, params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(transaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                return GetDynamicSqlData(dbCommand.ExecuteReader());
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {

                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(MySqlTransaction transaction, string commandText, object paramObject = null, 
            string fieldsToSkip = null)
        {
            return ExecuteDynamics(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a IEnumerable{object} from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> A list of object. </returns>
        public IEnumerable<dynamic> ExecuteDynamics(MySqlTransaction transaction, string commandText, CommandType commandType,
            object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamics(transaction, commandText, commandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(string commandText, string fieldsToSkip = null, params MySqlParameter[] mySqlParameters)
        {
            return ExecuteDynamic(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters"> Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(string commandText, CommandType commandType, string fieldsToSkip = null,
            params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                var reader = dbCommand.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    var result = SqlDataReaderToExpando(reader);
                    reader.Close();
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(string commandText, object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamic(commandText, DefaultSimpleAccessSettings.DefaultCommandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(string commandText, CommandType commandType, object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamic(commandText, commandType, fieldsToSkip,
                BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(MySqlTransaction transaction, string commandText, string fieldsToSkip = null,
            params MySqlParameter[] mySqlParameters)
        {
            return ExecuteDynamic(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, mySqlParameters);
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// <param name="mySqlParameters">  Parameters required to execute CommandText. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(MySqlTransaction transaction, string commandText, CommandType commandType,
            string fieldsToSkip = null, params MySqlParameter[] mySqlParameters)
        {
            MySqlCommand dbCommand = null;
            try
            {
                dbCommand = CreateCommand(transaction, commandText, commandType, mySqlParameters);
                dbCommand.Connection.OpenSafely();
                var reader = dbCommand.ExecuteReader();
                if (reader.Read())
                    return SqlDataReaderToExpando(reader);

                return null;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);
                throw;
            }
            finally
            {

                dbCommand.ClearDbCommand();

            }
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// -<param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(MySqlTransaction transaction, string commandText, object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamic(transaction, commandText, DefaultSimpleAccessSettings.DefaultCommandType,
                fieldsToSkip, BuildSqlParameters(paramObject));
        }

        /// <summary> Sends the CommandText to the Connection and builds a anonymous object from DataReader. </summary>
        /// 
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="paramObject"> The anonymous object as parameters. </param>
        /// <param name="fieldsToSkip"> (optional) the fields to skip. </param>
        /// 
        /// <returns> Result in a anonymous object. </returns>
        public dynamic ExecuteDynamic(MySqlTransaction transaction, string commandText, CommandType commandType,
                object paramObject = null, string fieldsToSkip = null)
        {
            return ExecuteDynamic(transaction, commandText, commandType,
                fieldsToSkip, BuildSqlParameters(paramObject));
        }
        /// <summary>
        /// Execute the CommandText against connection and add or refresh rows in <see cref="DataTable"/>
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dataTable">A <see cref="DataTable"/> to fill with records and, if necessary, schema  </param>
        /// <returns></returns>
        public int Fill(string commandText, DataTable dataTable)
        {
            MySqlCommand dbCommand = null;
            try
            {
                if (dataTable == null)
                    dataTable = new DataTable();
                dbCommand = new MySqlCommand(commandText);
                var mysqlDataAdapter = new MySqlDataAdapter(dbCommand);
                return mysqlDataAdapter.Fill(dataTable);

            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);

                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
        }

        /// <summary>
        /// Execute the CommandText against connection and add or refresh rows in <see cref="DataSet"/>
        /// </summary>
        /// <param name="commandText"> The SQL statement, table name or stored procedure to execute at the data source.</param>
        /// <param name="dataSet"> A <see cref="DataSet"/> to fill with records and, if necessary, schema  </param>
        /// <returns></returns>
        public int Fill(string commandText, DataSet dataSet)
        {
            MySqlCommand dbCommand = null;
            try
            {
                if (dataSet == null)
                    dataSet = new DataSet();

                dbCommand = new MySqlCommand(commandText);

                MySqlDataAdapter mysqlDataAdapter = new MySqlDataAdapter(dbCommand);
                return mysqlDataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex);

                throw;
            }
            finally
            {
                if (_transaction == null && _sqlConnection.State != ConnectionState.Closed)
                    _sqlConnection.CloseSafely();

                dbCommand.ClearDbCommand();
            }
        }

        /// <summary> Gets the new connection. </summary>
        /// <returns> The new connection. </returns>
        public MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_sqlConnection.ConnectionString);
        }

        /// <summary> Close the current open connection. </summary>
        public void CloseDbConnection()
        {
            if (_sqlConnection != null)
                _sqlConnection.CloseSafely();
        }

        /// <summary> Begins a transaction. </summary>
        /// <returns> . </returns>
        public MySqlTransaction BeginTrasaction()
        {
            if (_sqlConnection.State != ConnectionState.Open)
                _sqlConnection.Open();
            //_transaction = _sqlConnection.BeginTransaction();

            //return _transaction;
            return _sqlConnection.BeginTransaction();
        }

        /// <summary> Ends a transaction. </summary>
        /// 
        /// <param name = "transaction" > The SQL transaction. </param>
        /// <param name = "transactionSucceed" > (optional)the transaction succeed. </param>
        /// <param name = "closeConnection" > (optional)the close connection. </param>
        public void EndTransaction(MySqlTransaction transaction, bool transactionSucceed = true, bool closeConnection = true)
        {
            if (transactionSucceed)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            if (closeConnection)
            {
                _sqlConnection.CloseSafely();
            }
        }


        /// <summary> Creates a command. </summary>
        /// 
        /// <param name="commandText"> The query string. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters">Options for controlling the SQL. </param>
        /// 
        /// <returns> The new command. </returns>
        public MySqlCommand CreateCommand(string commandText, CommandType commandType, params MySqlParameter[] mySqlParameters)
        {
            var dbCommand = _sqlConnection.CreateCommand();
            dbCommand.CommandTimeout = DefaultSimpleAccessSettings.DbCommandTimeout;
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            if (mySqlParameters != null)
                dbCommand.Parameters.AddRange(mySqlParameters);

            if (_transaction != null)
                dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        /// <summary> Creates a command. </summary>
        /// 
        /// <param name="transaction"> The SQL transaction. </param>
        /// <param name="commandText"> The query string. </param>
        /// <param name="commandType"> Type of the command. </param>
        /// <param name="mySqlParameters"> Options for controlling the SQL. </param>
        /// 
        /// <returns> The new command. </returns>
        public MySqlCommand CreateCommand(MySqlTransaction transaction, string commandText, CommandType commandType
            , params MySqlParameter[] mySqlParameters)
        {
            var dbCommand = _sqlConnection.CreateCommand();
            dbCommand.CommandTimeout = DefaultSimpleAccessSettings.DbCommandTimeout;
            dbCommand.Transaction = transaction;
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            if (mySqlParameters != null)
                dbCommand.Parameters.AddRange(mySqlParameters);
            if (_transaction != null)
                dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        /// <summary> Gets a object SQL data. </summary>
        /// 
        /// <param name="reader"> The reader. </param>
        /// 
        /// <returns> The object SQL data. </returns>
        public IList<T> GetValues<T>(MySqlDataReader reader)
        {
            var result = new List<T>();

            while (reader.Read())
            {
                result.Add((T)Convert.ChangeType(reader[0], typeof(T)));
            }
            return result;
        }

        /// <summary> SQL data reader to <see cref="ExpandoObject"/>. </summary>
        /// 
        /// <param name="reader"> The reader. </param>
        /// 
        /// <returns> . </returns>
        public dynamic SqlDataReaderToExpando(MySqlDataReader reader)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader[i];
                expandoObject.Add(reader.GetName(i), value == DBNull.Value ? null : value);
            }


            return expandoObject;
        }

        /// <summary> Gets a object SQL data. </summary>
        /// 
        /// <param name="reader"> The reader. </param>
        /// 
        /// <returns> The object SQL data. </returns>
        public IList<dynamic> GetDynamicSqlData(MySqlDataReader reader)
        {
            var result = new List<dynamic>();

            while (reader.Read())
            {
                result.Add(SqlDataReaderToExpando(reader));
            }
            return result;
        }

        /// <summary> Build MySqlParameter Array from anonymous object. </summary>
        ///  <param name="paramObject"> The anonymous object as parameters. </param>
        /// <returns> MySqlParameter[] object and if paramObject is null then return null </returns>
        public MySqlParameter[] BuildSqlParameters(object paramObject)
        {
            if (paramObject == null)
                return null;
            var mySqlParameters = new List<MySqlParameter>();
            mySqlParameters.CreateSqlParametersFromObject(paramObject);

            return mySqlParameters.ToArray();
        }


        /// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources. </summary>
        public void Dispose()
        {
            if (_transaction != null)
                _transaction.Dispose();

            if (_sqlConnection.State != ConnectionState.Closed)
                _sqlConnection.Close();

            DefaultSimpleAccessSettings = null;
        }
    }
}
