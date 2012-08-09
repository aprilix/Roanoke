create table [dbo].[FactBuildDropSize]
(
    [BuildDropSizeSK]       int identity primary key,
    [BuildDropSizeBK]       nvarchar(512) not null,
    [FileCount]				int not null,
    [FileSize]				bigint not null,
    [LastUpdatedDateTime]	datetime not null,
    [BuildBK]				nvarchar(64) not null
)
