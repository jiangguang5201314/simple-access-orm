# SqlSimpleAccess.ExecuteReader Method (String, CommandBehavior, SqlParameter[])
 

Executes the commandText and return TDbDataReader.

**Namespace:**&nbsp;<a href="N_SimpleAccess_SqlServer">SimpleAccess.SqlServer</a><br />**Assembly:**&nbsp;SimpleAccess.SqlServer (in SimpleAccess.SqlServer.dll) Version: 0.2.3.0 (0.2.8.0)

## Syntax

**C#**<br />
``` C#
public SqlDataReader ExecuteReader(
	string commandText,
	CommandBehavior commandBehavior,
	params SqlParameter[] sqlParameters
)
```

**VB**<br />
``` VB
Public Function ExecuteReader ( 
	commandText As String,
	commandBehavior As CommandBehavior,
	ParamArray sqlParameters As SqlParameter()
) As SqlDataReader
```

**C++**<br />
``` C++
public:
virtual SqlDataReader^ ExecuteReader(
	String^ commandText, 
	CommandBehavior commandBehavior, 
	... array<SqlParameter^>^ sqlParameters
) sealed
```

**F#**<br />
``` F#
abstract ExecuteReader : 
        commandText : string * 
        commandBehavior : CommandBehavior * 
        sqlParameters : SqlParameter[] -> SqlDataReader 
override ExecuteReader : 
        commandText : string * 
        commandBehavior : CommandBehavior * 
        sqlParameters : SqlParameter[] -> SqlDataReader 
```


#### Parameters
&nbsp;<dl><dt>commandText</dt><dd>Type: System.String<br />The SQL statement, table name or stored procedure to execute at the data source.</dd><dt>commandBehavior</dt><dd>Type: System.Data.CommandBehavior<br />The CommandBehavior of executing DbCommand</dd><dt>sqlParameters</dt><dd>Type: System.Data.SqlClient.SqlParameter[]<br />Parmeters rquired to execute CommandText.</dd></dl>

#### Return Value
Type: SqlDataReader<br />The TDbDataReader

#### Implements
<a href="M_SimpleAccess_Core_ISimpleAccess_6_ExecuteReader">ISimpleAccess(TDbConnection, TDbTransaction, TDbCommand, TDataParameter, TDbDataReader, TParameterBuilder).ExecuteReader(String, CommandBehavior, TDataParameter[])</a><br />

## Exceptions
&nbsp;<table><tr><th>Exception</th><th>Condition</th></tr><tr><td>Exception</td><td>Thrown when an exception error condition occurs.</td></tr></table>

## See Also


#### Reference
<a href="T_SimpleAccess_SqlServer_SqlSimpleAccess">SqlSimpleAccess Class</a><br /><a href="Overload_SimpleAccess_SqlServer_SqlSimpleAccess_ExecuteReader">ExecuteReader Overload</a><br /><a href="N_SimpleAccess_SqlServer">SimpleAccess.SqlServer Namespace</a><br />