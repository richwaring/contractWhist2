
/*
drop table games
drop table gameRounds
drop table playerRoundScores
go
*/
if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'games')
drop table games
go
create table games (id bigint identity primary key, crdate datetime)
go

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'gameRounds')
drop table gameRounds
go
create table gameRounds (roundid bigint identity primary key, gameId bigint, roundNumber int, numCards bigint)

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'playerRoundScores')
drop table playerRoundScores
go
create table playerRoundScores (prsID bigint identity primary key, roundId bigint, playerId int, target int, tricksWon int, roundPoints bigint, gamePoints bigint)
go
------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_newGame' )
drop proc Scoreboard_newGame
go
create proc Scoreboard_newGame @rounds int, @gameID bigint output
as
declare @roundCounter int

INSERT INTO games (crdate) select getdate()
select @gameID = @@identity

select @roundCounter = 1

while @roundCounter <= @rounds
		begin

		insert into gameRounds (gameId, roundNumber, numCards)
		select @gameID, @roundCounter,  @rounds - @roundCounter + 1 

		select @roundCounter += 1

		end

return @gameID

go
------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_userTarget' )
drop proc Scoreboard_userTarget
go
create proc Scoreboard_userTarget @gameID bigint, @roundNumber bigint, @userId int, @target int, @totalOfTargets int output, @fDebug char = 'N'
as

if @target is null select @target = 0
if @target < 0 select @target = 0

declare @roundid bigint
select @roundid = roundId from gameRounds where gameId = @gameID and roundNumber = @roundNumber

if not exists (select 1 from playerRoundScores where playerid = @userid and roundid = @roundid)
INSERT INTO playerRoundScores (roundId, playerId, target)
select @roundid, @userId, @target 
else
update playerRoundScores set target = @target where playerid = @userid and roundid = @roundid

select @totalOfTargets = sum(isnull(target,0)) 
from playerRoundScores
where roundid in (select roundId from gameRounds where gameId = @gameID and roundNumber = @roundNumber and roundnumber = @roundnumber)

if @fDebug = 'Y'
begin
SELECT @totalOfTargets '@totalOfTargets'

select 'playerRoundScores',*
from playerRoundScores
where roundid in (select roundId from gameRounds where gameId = @gameID and roundNumber = @roundNumber and roundnumber = @roundnumber)

end

return @totalOfTargets

go
------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_userRoundScore' )
drop proc Scoreboard_userRoundScore
go
create proc Scoreboard_userRoundScore @gameID bigint, @roundNumber bigint, @playerID int, @tricksWon int
as

declare @prsID bigint

select @prsID = prsid
from playerRoundScores prs 
join gameRounds gr on gr.roundId = prs.roundId
where prs.playerID = @playerID
and gr.gameId = @gameID and gr.roundNumber = @roundNumber

update prs set trickswon = @tricksWon, 
roundPoints = @tricksWon + (case when @tricksWon = target then 10 else 0 end)
from playerRoundScores prs 
where prsID = @prsID

select sum(roundPoints) 
from playerRoundScores 
where playerId = @playerID
and roundid in (select roundid from gameRounds where gameId = @gameID)

update prs set gamePoints = 
--
	(select sum(roundPoints) 
	from playerRoundScores 
	where playerId = @playerID
	and roundid in (select roundid from gameRounds where gameId = @gameID))
--
from playerRoundScores prs 
where prsID = @prsID

go
-------------------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_showGameScoreboard' )
drop proc Scoreboard_showGameScoreboard
go
create proc Scoreboard_showGameScoreboard @gameID bigint
as

select gr.roundNumber, 
isnull(prs1.target, -1) 'p1_Target', isnull(prs1.roundPoints, -1) 'p1_Round_Points', isnull(prs1.gamePoints, -1) 'p1_Game_points',
isnull(prs2.target, -1) 'p2_Target', isnull(prs2.roundPoints, -1) 'p2_Round_Points', isnull(prs2.gamePoints, -1) 'p2_Game_points',
isnull(prs3.target, -1) 'p3_Target', isnull(prs3.roundPoints, -1) 'p3_Round_Points', isnull(prs3.gamePoints, -1) 'p3_Game_points',
isnull(prs4.target, -1) 'p4_Target', isnull(prs4.roundPoints, -1) 'p4_Round_Points', isnull(prs4.gamePoints, -1) 'p4_Game_points'
from games g
join gameRounds gr on g.id = gr.gameid
left join playerRoundScores prs1 on gr.roundid = prs1.roundId and prs1.playerId = 1
left join playerRoundScores prs2 on gr.roundid = prs2.roundId and prs2.playerId = 2
left join playerRoundScores prs3 on gr.roundid = prs3.roundId and prs3.playerId = 3
left join playerRoundScores prs4 on gr.roundid = prs4.roundId and prs4.playerId = 4
where g.id = @gameID
order by gr.roundNumber

go
-------------------------------------------------------------------------------------------------------------------

SELECT * FROM games order by id desc


exec Scoreboard_newGame @rounds = 7, @gameid = null --check
go
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 1, @userId = 1, @target = 6, @totaloftargets = null --check* (capture output param?)
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 1, @userId = 2, @target = 5, @totaloftargets = null
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 1, @userId = 3, @target = 4, @totaloftargets = null
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 1, @userId = 4, @target = 3, @totaloftargets = null
go
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 1, @playerID = 1, @tricksWon = 2
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 1, @playerID = 2, @tricksWon = 3
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 1, @playerID = 3, @tricksWon = 4
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 1, @playerID = 4, @tricksWon = 5
go

declare @mytotaloftargets int 
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 2, @userId = 1, @target = 1, @totaloftargets = @mytotaloftargets output, @fDebug = 'Y'
select @mytotaloftargets

exec Scoreboard_userTarget @gameID = 1, @roundNumber = 2, @userId = 2, @target = 0, @totaloftargets = null
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 2, @userId = 3, @target = 3, @totaloftargets = null
exec Scoreboard_userTarget @gameID = 1, @roundNumber = 2, @userId = 4, @target = 2, @totaloftargets = null
go
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 2, @playerID = 1, @tricksWon = 1
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 2, @playerID = 2, @tricksWon = 2
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 2, @playerID = 3, @tricksWon = 3
exec Scoreboard_userRoundScore @gameID = 1, @roundNumber = 2, @playerID = 4, @tricksWon = 4
go
exec Scoreboard_showGameScoreboard @gameid = 1




SELECT * FROM playerRoundScores


select * from gameRounds where roundid = 2

SELECT 'games',* FROM games order by crdate desc
SELECT 'gameRounds',* FROM gameRounds
SELECT roundid, playerId, count(1) FROM playerRoundScores group by roundid, playerId




