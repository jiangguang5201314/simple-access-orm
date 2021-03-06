# SqlRepository.Delete(*TEntity*) Method (Int64)
 

Deletes the given ID.

**Namespace:**&nbsp;<a href="N_SimpleAccess_SqlServer_Repository">SimpleAccess.SqlServer.Repository</a><br />**Assembly:**&nbsp;SimpleAccess.SqlServer (in SimpleAccess.SqlServer.dll) Version: 0.2.3.0 (0.2.8.0)

## Syntax

**C#**<br />
``` C#
public int Delete<TEntity>(
	long id
)
where TEntity : class

```

**VB**<br />
``` VB
Public Function Delete(Of TEntity As Class) ( 
	id As Long
) As Integer
```

**C++**<br />
``` C++
public:
generic<typename TEntity>
where TEntity : ref class
virtual int Delete(
	long long id
) sealed
```

**F#**<br />
``` F#
abstract Delete : 
        id : int64 -> int  when 'TEntity : not struct
override Delete : 
        id : int64 -> int  when 'TEntity : not struct
```


#### Parameters
&nbsp;<dl><dt>id</dt><dd>Type: System.Int64<br />The identifier.</dd></dl>

#### Type Parameters
&nbsp;<dl><dt>TEntity</dt><dd>Type of the entity.</dd></dl>

#### Return Value
Type: Int32<br />.

#### Implements
<a href="M_SimpleAccess_Repository_ISqlRepository_Delete__1_3">ISqlRepository.Delete(TEntity)(Int64)</a><br />

## See Also


#### Reference
<a href="T_SimpleAccess_SqlServer_Repository_SqlRepository">SqlRepository Class</a><br /><a href="Overload_SimpleAccess_SqlServer_Repository_SqlRepository_Delete">Delete Overload</a><br /><a href="N_SimpleAccess_SqlServer_Repository">SimpleAccess.SqlServer.Repository Namespace</a><br />