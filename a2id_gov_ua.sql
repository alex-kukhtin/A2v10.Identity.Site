/*
  Copyright © 2020 Alex Kukhtin. All rights reserved.
  id.gov.ua support
*/

------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2id')
begin
	exec sp_executesql N'create schema a2id';
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2id' and TABLE_NAME=N'Sessions')
begin
	create table a2id.[Sessions] (
		Id uniqueidentifier not null constraint PK_Sessions primary key nonclustered (Id),
		[User] bigint not null,
		[Date] datetime not null constraint DF_Uploads_Date default(getutcdate())
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2id' and TABLE_NAME=N'UserInfos')
begin
	create table a2id.[UserInfos] (
		[Id] bigint identity(100, 1) not null constraint PK_UserInfos primary key (Id),
		[Session] uniqueidentifier not null 
			constraint FK_UserInfos_Session_Sessions foreign key references a2id.[Sessions](Id),
		[Name] nvarchar(255) null,
		[Value] nvarchar(255) null
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2id' and TABLE_NAME=N'UserIdentity')
begin
	create table a2id.[UserIdentity] (
		[User] bigint not null constraint PK_Identity primary key nonclustered,
		[Session] uniqueidentifier not null 
			constraint FK_UserIdentity_Session_Sessions foreign key references a2id.[Sessions](Id),
		[Date] datetime not null constraint DF_UserIdentity_Date default(getutcdate())
	);
end
go
------------------------------------------------
create or alter procedure a2id.[State.Create]
@UserId bigint
as
begin
	set nocount on;
	set xact_abort on;

	declare @out table(id uniqueidentifier);
	insert into a2id.[Sessions] (Id, [User]) 
	output inserted.Id into @out(id)
	values
		(newid(), @UserId);
	select [State]=id from @out;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2id' and ROUTINE_NAME=N'UserInfo.Save')
	drop procedure a2id.[UserInfo.Save]
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'UserInfo.TableType' AND ss.name = N'a2id')
	drop type a2id.[UserInfo.TableType];
go
------------------------------------------------
create type a2id.[UserInfo.TableType] as table
(
	[Name] nvarchar(255),
	[Value] nvarchar(255)
)
go
------------------------------------------------
create or alter procedure a2id.[UserInfo.Save]
@UserId bigint,
@Session uniqueidentifier,
@Rows a2id.[UserInfo.TableType] readonly
as
begin
	set nocount on;
	set xact_abort on;

	begin tran;
	delete from a2id.UserInfos where [Session]=@Session;

	insert into a2id.UserInfos([Session], [Name], [Value])
		select @Session, [Name], [Value] from @Rows;

	update a2id.UserIdentity set [Session] = @Session where [User]=@UserId;

	if @@rowcount = 0
		insert into a2id.UserIdentity ([User], [Session]) values (@UserId, @Session);
	commit tran;
end
go
------------------------------------------------
begin
	set nocount on;
	grant execute on schema ::a2id to public;
end
go

