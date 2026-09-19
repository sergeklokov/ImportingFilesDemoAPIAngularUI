SELECT TOP (1000) Id
      ,SourceType
      ,LineNumber
      ,RawLine
      ,FileName
      ,CreatedBy
      ,CreatedAt
  FROM Phones.dbo.FileImports
--truncate table dbo.FileImports

select FORMAT(COUNT(*), 'N0') from dbo.FileImports (nolock)