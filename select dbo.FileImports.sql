SELECT TOP (1000) Id
      ,SourceType
      ,LineNumber
      ,RawLine
      ,FileName
      ,CreatedBy
      ,CreatedAt
  FROM Phones.dbo.FileImports
--truncate table dbo.FileImports

select count(*) from dbo.FileImports (nolock)