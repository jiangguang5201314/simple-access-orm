# IRepository.SoftDelete(*TEntity*) Method 
 

Soft delete.

**Namespace:**&nbsp;<a href="N_SimpleAccess_Repository">SimpleAccess.Repository</a><br />**Assembly:**&nbsp;SimpleAccess.SqlServer (in SimpleAccess.SqlServer.dll) Version: 0.2.3.0 (0.2.8.0)

## Syntax

**C#**<br />
``` C#
int SoftDelete<TEntity>(
	long id
)
where TEntity : class

```

**VB**<br />
``` VB
Function SoftDelete(Of TEntity As Class) ( 
	id As Long
) As Integer
```

**C++**<br />
``` C++
generic<typename TEntity>
where TEntity : ref class
int SoftDelete(
	long long id
)
```

**F#**<br />
``` F#
abstract SoftDelete : 
        id : int64 -> int  when 'TEntity : not struct

```


#### Parameters
&nbsp;<dl><dt>id</dt><dd>Type: System.Int64<br />The identifier.</dd></dl>

#### Type Parameters
&nbsp;<dl><dt>TEntity</dt><dd>Type of the entity.</dd></dl>

#### Return Value
Type: Int32<br />Number of rows affected (integer)

## See Also


#### Reference
<a href="T_SimpleAccess_Repository_IRepository">IRepository Interface</a><br /><a href="N_SimpleAccess_Repository">SimpleAccess.Repository Namespace</a><br />