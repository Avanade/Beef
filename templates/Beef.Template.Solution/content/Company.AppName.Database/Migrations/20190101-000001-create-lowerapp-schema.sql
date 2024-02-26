//#if (implement_sqlserver || implement_database)
CREATE SCHEMA [AppName]
    AUTHORIZATION [dbo];
//#endif
//#if (implement_postgres)
CREATE SCHEMA "lowerapp";
//#endif