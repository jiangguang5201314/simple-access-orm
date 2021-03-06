# Repository.Update(*TEntity*) Method (SqlTransaction, StoredProcedureParameters)
 

Updates the given sqlParameters.

**Namespace:**&nbsp;<a href="N_SimpleAccess_Repository">SimpleAccess.Repository</a><br />**Assembly:**&nbsp;SimpleAccess.SqlServer (in SimpleAccess.SqlServer.dll) Version: 0.2.3.0 (0.2.8.0)

## Syntax

**C#**<br />
``` C#
public int Update<TEntity>(
	SqlTransaction sqlTransaction,
	StoredProcedureParameters storedProcedureParameters
)
where TEntity : class

```

**VB**<br />
``` VB
Public Function Update(Of TEntity As Class) ( 
	sqlTransaction As SqlTransaction,
	storedProcedureParameters As StoredProcedureParameters
) As Integer
```

**C++**<br />
``` C++
public:
generic<typename TEntity>
where TEntity : ref class
virtual int Update(
	SqlTransaction^ sqlTransaction, 
	StoredProcedureParameters^ storedProcedureParameters
) sealed
```

**F#**<br />
``` F#
abstract Update : 
        sqlTransaction : SqlTransaction * 
        storedProcedureParameters : StoredProcedureParameters -> int  when 'TEntity : not struct
override Update : 
        sqlTransaction : SqlTransaction * 
        storedProcedureParameters : StoredProcedureParameters -> int  when 'TEntity : not struct
```


#### Parameters
&nbsp;<dl><dt>sqlTransaction</dt><dd>Type: System.Data.SqlClient.SqlTransaction<br />The SQL transaction.</dd><dt>storedProcedureParameters</dt><dd>Type: <a href="T_SimpleAccess_StoredProcedureParameters">SimpleAccess.StoredProcedureParameters</a><br />Options for controlling the stored procedure.</dd></dl>

#### Type Parameters
&nbsp;<dl><dt>TEntity</dt><dd>Type of the entity.</dd></dl>

#### Return Value
Type: Int32<br />.

#### Implements
<a href="M_SimpleAccess_Repository_IRepository_Update__1_2">IRepository.Update(TEntity)(SqlTransaction, StoredProcedureParameters)</a><br />

## See Also


#### Reference
<a href="T_SimpleAccess_Repository_Repository">Repository Class</a><br /><a href="Overload_SimpleAccess_Repository_Repository_Update">Update Overload</a><br /><a href="N_SimpleAccess_Repository">SimpleAccess.Repository Namespace</a><br />