# SqlSimpleAccess.ExecuteDynamics Method (String, CommandType, String, Object)
 

Sends the CommandText to the Connection and builds a IEnumerable{dynamic} from DataReader.

**Namespace:**&nbsp;<a href="N_SimpleAccess_SqlServer">SimpleAccess.SqlServer</a><br />**Assembly:**&nbsp;SimpleAccess.SqlServer (in SimpleAccess.SqlServer.dll) Version: 0.2.3.0 (0.2.8.0)

## Syntax

**C#**<br />
``` C#
public IEnumerable<Object> ExecuteDynamics(
	string commandText,
	CommandType commandType,
	string fieldsToSkip = null,
	Object paramObject = null
)
```

**VB**<br />
``` VB
Public Function ExecuteDynamics ( 
	commandText As String,
	commandType As CommandType,
	Optional fieldsToSkip As String = Nothing,
	Optional paramObject As Object = Nothing
) As IEnumerable(Of Object)
```

**C++**<br />
``` C++
public:
virtual IEnumerable<Object^>^ ExecuteDynamics(
	String^ commandText, 
	CommandType commandType, 
	String^ fieldsToSkip = nullptr, 
	Object^ paramObject = nullptr
) sealed
```

**F#**<br />
``` F#
abstract ExecuteDynamics : 
        commandText : string * 
        commandType : CommandType * 
        ?fieldsToSkip : string * 
        ?paramObject : Object 
(* Defaults:
        let _fieldsToSkip = defaultArg fieldsToSkip null
        let _paramObject = defaultArg paramObject null
*)
-> IEnumerable<Object> 
override ExecuteDynamics : 
        commandText : string * 
        commandType : CommandType * 
        ?fieldsToSkip : string * 
        ?paramObject : Object 
(* Defaults:
        let _fieldsToSkip = defaultArg fieldsToSkip null
        let _paramObject = defaultArg paramObject null
*)
-> IEnumerable<Object> 
```


#### Parameters
&nbsp;<dl><dt>commandText</dt><dd>Type: System.String<br />The SQL statement, table name or stored procedure to execute at the data source.</dd><dt>commandType</dt><dd>Type: System.Data.CommandType<br />Type of the command.</dd><dt>fieldsToSkip (Optional)</dt><dd>Type: System.String<br />(optional) the fields to skip.</dd><dt>paramObject (Optional)</dt><dd>Type: System.Object<br />The dynamic object as parameters.</dd></dl>

#### Return Value
Type: IEnumerable(Object)<br />A list of dynamic.

#### Implements
<a href="M_SimpleAccess_Core_ISimpleAccess_6_ExecuteDynamics">ISimpleAccess(TDbConnection, TDbTransaction, TDbCommand, TDataParameter, TDbDataReader, TParameterBuilder).ExecuteDynamics(String, CommandType, String, Object)</a><br />

## Exceptions
&nbsp;<table><tr><th>Exception</th><th>Condition</th></tr><tr><td>Exception</td><td>Thrown when an exception error condition occurs.</td></tr></table>

## See Also


#### Reference
<a href="T_SimpleAccess_SqlServer_SqlSimpleAccess">SqlSimpleAccess Class</a><br /><a href="Overload_SimpleAccess_SqlServer_SqlSimpleAccess_ExecuteDynamics">ExecuteDynamics Overload</a><br /><a href="N_SimpleAccess_SqlServer">SimpleAccess.SqlServer Namespace</a><br />