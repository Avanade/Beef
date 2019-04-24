# 'CodeGeneration' element (table-driven)

The **`CodeGeneration`** element defines global attributes that are used to drive the underlying database table-driven stored procedure generation. 

An example is as follows:
```xml
<CodeGeneration xmlns="http://schemas.beef.com/codegen/2015/01/database" RefDatabaseSchema="Ref" ConnectionString="Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True">
```

***

# Attributes

The **`CodeGeneration`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes:

Attribute | Description
-|-
**`RefDatabaseSchema`** | The schema name to identify the reference data tables.
**`ConnectionString`** | The connection string used to connect to the database to query the underlying table definitions/schema.