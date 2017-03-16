ALTER PROC [dbo].[WebsiteTeams_Insert]
			@Id int OUTPUT
			,@Name nvarchar(200)
			,@WebsiteId int
			,@Description nvarchar(300)
			,@ParentTeamId int
			,@AddressId nvarchar(200)
			,@WebsiteIds AS [dbo].[IntIdTable] READONLY
			,@ZipCodes AS [dbo].[NVarCharTable] READONLY



AS

BEGIN

INSERT INTO [dbo].[WebsiteTeams]
           ([Name]
           ,[WebsiteId]
           ,[Description]
           ,[ParentTeamId]
		   ,[AddressId])
     VALUES
           (@Name
           ,@WebsiteId
           ,@Description
           ,@ParentTeamId
	   ,@AddressId)

		  Set @Id = SCOPE_IDENTITY();

DELETE FROM [dbo].[WebsiteTeamsZipCodes]
     WHERE TeamId = @Id

INSERT INTO [dbo].[WebsiteTeamsZipCodes] (TeamId, ZipCode)
     SELECT @Id, Data FROM @ZipCodes;
	
DELETE FROM [dbo].[WebsiteTeamsWebsites]
     WHERE TeamId = @Id

INSERT INTO [dbo].[WebsiteTeamsWebsites] (TeamId, WebsiteId)
     SELECT @Id, Data FROM @WebsiteIds;
END