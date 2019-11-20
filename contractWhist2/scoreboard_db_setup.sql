
/*
drop table games
drop table gameRounds
drop table playerRounds
go
*/
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'suits')
drop table suits
go
create table suits (shortname char(1) primary key clustered,name varchar(10))
go
insert into suits(shortname, name)
select 'C', 'Clubs' union
select 'D', 'Diamonds' union
select 'H', 'Hearts' union
select 'S', 'Spades'
go
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'cardsInSuit')
drop table cardsInSuit
go
create table cardsInSuit (shortName char(1) primary key clustered, longName varchar(10), valueInSuit int)
go
insert into cardsInSuit(shortname, longname, valueInSuit)
select '2', 'Two', 1 union
select '3', 'Three', 2 union
select '4', 'Four', 3 union
select '5', 'Five', 4 union
select '6', 'Six', 5 union
select '7', 'Seven', 6 union
select '8', 'Eight', 7 union
select '9', 'Nine', 8 union
select 'T', 'Ten', 9 union
select 'J', 'Jack', 10 union
select 'Q', 'Queen', 11 union
select 'K', 'King', 12 union
select 'A', 'Ace', 13
go
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'cardsInDeck')
drop table cardsInDeck
go
create table cardsInDeck (shortName char(2) primary key clustered, id int identity, longName varchar(50), valueInSuit int, suit char(1))
go
INSERT INTO cardsInDeck
select c.shortname + s.shortname, 'The ' + lower(c.longName) + ' of ' + lower(s.name ), c.valueInSuit, s.shortname
from cardsInSuit c, Suits s
order by s.name, c.valueInSuit
go
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'games')
drop table games
go
create table games (id bigint identity primary key clustered, crdate datetime)
go
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'gameRounds')
drop table gameRounds
go
create table gameRounds (roundid bigint identity primary key, gameId bigint, roundNumber int, numCards bigint, trumpCard char(2), trumps char(1))
---------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'playerRounds')
drop table playerRounds
go
create table playerRounds (prID bigint identity primary key clustered, gameId bigint, roundId bigint, playerId int, target int, tricksWon int, roundPoints bigint, gamePoints bigint, roundNumber int)
go
create index roundId_GameId on playerRounds (roundId, GameId)
go
create index gameId_roundid on playerRounds (GameId, roundId)
go
------------------------------------------------------------------------------------------------------

if exists (select 'tabs',* from sysobjects where type = 'U' and name like 'playerRoundCards')
drop table playerRoundCards
go
create table playerRoundCards (prcID bigint identity primary key clustered, prID bigint, playerId int, roundid bigint, cardShortName char(2), valueInSuit int, cardPlayed char(1) DEFAULT 'N')
go
create index prid_playerId on playerRoundCards (prID, playerId, cardPlayed)
go
create index roundid_playerId on playerRoundCards (roundid, playerId, cardPlayed)
go


------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_newGame' )
drop proc Scoreboard_newGame
go
create proc Scoreboard_newGame @rounds int, @gameID bigint output, @fDebug char (1) = 'Y'
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

INSERT INTO playerRounds (roundid, playerId, gameId, tricksWon, target, roundNumber)
select roundid, 1, gameId, 0, -1, roundNumber from gameRounds where gameId = @gameID union
select roundid, 2, gameId, 0, -1, roundNumber from gameRounds where gameId = @gameID union
select roundid, 3, gameId, 0, -1, roundNumber from gameRounds where gameId = @gameID union
select roundid, 4, gameId, 0, -1, roundNumber from gameRounds where gameId = @gameID 


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

if not exists (select 1 from playerRounds where playerid = @userid and roundid = @roundid)
INSERT INTO playerRounds (roundId, playerId, target, tricksWon)
select @roundid, @userId, @target , 0
else
update playerRounds set target = @target, tricksWon = 0 where playerid = @userid and roundid = @roundid

select @totalOfTargets = sum(isnull(target,0)) 
from playerRounds
where roundid in (select roundId from gameRounds where gameId = @gameID and roundNumber = @roundNumber and roundnumber = @roundnumber)

if @fDebug = 'Y'
begin
SELECT @totalOfTargets '@totalOfTargets'

select 'playerRounds',*
from playerRounds
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

declare @prID bigint

select @prID = prID
from playerRounds prs 
join gameRounds gr on gr.roundId = prs.roundId
where prs.playerID = @playerID
and gr.gameId = @gameID and gr.roundNumber = @roundNumber

update prs set trickswon = @tricksWon, 
roundPoints = @tricksWon + (case when @tricksWon = target then 10 else 0 end)
from playerRounds prs 
where prID = @prID

select sum(roundPoints) 
from playerRounds 
where playerId = @playerID
and roundid in (select roundid from gameRounds where gameId = @gameID)

update prs set gamePoints = 
--
	(select sum(roundPoints) 
	from playerRounds 
	where playerId = @playerID
	and roundid in (select roundid from gameRounds where gameId = @gameID))
--
from playerRounds prs 
where prID = @prID

go
-------------------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'Scoreboard_showGameScoreboard' )
drop proc Scoreboard_showGameScoreboard
go
create proc Scoreboard_showGameScoreboard @gameID bigint, @full char(1) = 'N'
as

if @full != 'Y'

select gr.roundNumber, 
isnull(prs1.target, -1) 'p1_Target', isnull(prs1.roundPoints, -1) 'p1_Round_Points', isnull(prs1.gamePoints, -1) 'p1_Game_points',
isnull(prs2.target, -1) 'p2_Target', isnull(prs2.roundPoints, -1) 'p2_Round_Points', isnull(prs2.gamePoints, -1) 'p2_Game_points',
isnull(prs3.target, -1) 'p3_Target', isnull(prs3.roundPoints, -1) 'p3_Round_Points', isnull(prs3.gamePoints, -1) 'p3_Game_points',
isnull(prs4.target, -1) 'p4_Target', isnull(prs4.roundPoints, -1) 'p4_Round_Points', isnull(prs4.gamePoints, -1) 'p4_Game_points'
from games g
join gameRounds gr on g.id = gr.gameid
left join playerRounds prs1 on gr.roundid = prs1.roundId and prs1.playerId = 1
left join playerRounds prs2 on gr.roundid = prs2.roundId and prs2.playerId = 2
left join playerRounds prs3 on gr.roundid = prs3.roundId and prs3.playerId = 3
left join playerRounds prs4 on gr.roundid = prs4.roundId and prs4.playerId = 4
where g.id = @gameID
order by gr.roundNumber

else

select gr.roundNumber, 
isnull(prs1.target, -1) 'p1_Target', isnull(prs1.roundPoints, -1) 'p1_Round_Points', isnull(prs1.gamePoints, -1) 'p1_Game_points', prs1.tricksWon 'p1.tricksWon',
isnull(prs2.target, -1) 'p2_Target', isnull(prs2.roundPoints, -1) 'p2_Round_Points', isnull(prs2.gamePoints, -1) 'p2_Game_points', prs2.tricksWon 'p2.tricksWon',
isnull(prs3.target, -1) 'p3_Target', isnull(prs3.roundPoints, -1) 'p3_Round_Points', isnull(prs3.gamePoints, -1) 'p3_Game_points', prs3.tricksWon 'p3.tricksWon',
isnull(prs4.target, -1) 'p4_Target', isnull(prs4.roundPoints, -1) 'p4_Round_Points', isnull(prs4.gamePoints, -1) 'p4_Game_points', prs4.tricksWon 'p4.tricksWon'
from games g
join gameRounds gr on g.id = gr.gameid
left join playerRounds prs1 on gr.roundid = prs1.roundId and prs1.playerId = 1
left join playerRounds prs2 on gr.roundid = prs2.roundId and prs2.playerId = 2
left join playerRounds prs3 on gr.roundid = prs3.roundId and prs3.playerId = 3
left join playerRounds prs4 on gr.roundid = prs4.roundId and prs4.playerId = 4
where g.id = @gameID
order by gr.roundNumber

go
-------------------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'dealCards' )
drop proc dealCards
go
create proc dealCards @gameID bigint, @roundNumber int, @numPlayers int = 4, @numCardsToDeal int = 7, @fDebug char(1) = 'N'
as
/*
drop table #ourDeck
SELECT * FROM #ourDeck
*/

if not exists (SELECT 1 FROM gameRounds where gameid = @gameid and roundnumber = @roundNumber)
return -1

declare @deckCount int, @myCard char(2),@myCardvalueInSuit int, @randomNumber int, @playerCounter int, @totalCardsToDeal int, @roundID bigint

SELECT @roundID = roundid, @numCardsToDeal = numCards FROM gameRounds where gameid = @gameid and roundnumber = @roundNumber

select @totalCardsToDeal = @numPlayers * @numCardsToDeal

--get a new copy of the deck....
select * into #ourDeck from cardsInDeck order by id

--empty the dealt cards for this round....
delete from playerRoundCards where @roundID = roundid

if @fDebug = 'Y' 
begin
select @numCardsToDeal '@numCardsToDeal', count(1) from #ourDeck 
select count(1) from playerRoundCards where @roundID = roundid group by playerId
select '#ourDeck',* from #ourDeck order by id
end
----

while ( (select count(1) from playerRoundCards where roundid = @roundid)  < @totalCardsToDeal)
		begin

		select @playerCounter = 1

		while (@playerCounter < 5)

			begin

			--get one card
			select @deckCount = count(1) from #ourDeck
			select @randomNumber = floor(RAND() * (@deckCount)) + 1;
			
			with myCTE as (SELECT  row_number() OVER (order by id desc) row_num,* FROM #ourDeck )
			select @myCard = shortname, @myCardvalueInSuit = valueInSuit from myCTE where row_num = @randomNumber


			if @fDebug = 'Y' select @myCard '@myCard', @deckCount '@deckCount', @randomNumber '@randomNumber', @myCardvalueInSuit '@myCardvalueInSuit'
			
			DELETE FROM #ourDeck where shortName = @myCard

			INSERT INTO playerRoundCards (prID, playerId, cardShortname, roundid, valueInSuit)
			select prID,playerid,@myCard, roundid, @myCardvalueInSuit
			FROM playerRounds where roundid = @roundid and playerid = @playerCounter

			select @playerCounter += 1

			end

		end
--
if @fDebug = 'Y' 
SELECT 'playerRoundCards',* 
FROM playerRoundCards prc
join cardsInDeck cid on prc.cardShortName = cid.shortname
where roundid = @roundID
order by playerid, cid.suit, cid.valueInSuit desc

--finally set the trump card for the game:

			--get one card
			select @deckCount = count(1) from #ourDeck
			select @randomNumber = floor(RAND() * (@deckCount)) + 1;
			
			with myCTE as (SELECT  row_number() OVER (order by id desc) row_num,* FROM #ourDeck )
			select @myCard = shortname from myCTE where row_num = @randomNumber

			if @fDebug = 'Y' select @myCard '@myCard', @deckCount '@deckCount', @randomNumber '@randomNumber'

update gameRounds set trumpCard = @myCard, trumps = right(@myCard,1) where roundid = @roundid

go
-------------------------------------------------------------------------------------------------------------------

if exists (select 'procs',* from sysobjects where type = 'P' and name like 'getUserCards' )
drop proc getUserCards
go
create proc getUserCards @gameID bigint, @roundNumber bigint, @fDebug char(1) = 'N'
as

declare @roundID bigint
SELECT @roundID = roundid FROM gameRounds where gameid = @gameid and roundnumber = @roundNumber

if @fDebug = 'Y'
begin
select @roundID '@roundID'
end

SELECT x.playerid, cards = STUFF((
    SELECT N', ' + cardshortname FROM playerRoundCards
    WHERE playerid = x.playerid 
	and roundid = @roundID
	and cardPlayed = 'n'
	order by right(cardshortname,1), valueInSuit desc
    FOR XML PATH(''), TYPE).value(N'.[1]', N'nvarchar(max)'), 1, 2, N'' ), target, tricksWon,
	playedcards = STUFF((
    SELECT N', ' + cardshortname FROM playerRoundCards
    WHERE playerid = x.playerid 
	and roundid = @roundID
	and cardPlayed = 'y'
	order by right(cardshortname,1), valueInSuit desc
    FOR XML PATH(''), TYPE).value(N'.[1]', N'nvarchar(max)'), 1, 2, N'' ), prsub.roundPoints
FROM playerRoundCards x
join playerRounds pr on x.prID = pr.prID
left join (SELECT pr2.gameid, playerId, roundPoints FROM playerRounds pr2 where roundnumber in (select max(roundnumber) from playerRounds pr3 where roundPoints is not null and pr3.gameid = pr2.gameId and pr3.playerId = pr2.playerid )) prsub on prsub.gameId = pr.gameid and prsub.playerId = pr.playerId
where x.roundid = @roundID
GROUP BY x.playerid, target, tricksWon, prsub.roundPoints
union
select '9' 'playerid',trumpCard 'cards', 0, 0, '', 0 from gameRounds where roundid = @roundID
ORDER BY playerid

go
-------------------------------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'userPlaysCard' )
drop proc userPlaysCard
go
create proc userPlaysCard @gameID bigint, @roundNumber bigint, @cardPlayed varchar(100), @fDebug char(1) = 'N'
as

declare @roundID bigint
SELECT @roundID = roundid FROM gameRounds where gameid = @gameid and roundnumber = @roundNumber

if not exists (select 1 where @cardPlayed like '%,%')
begin
create table #cardslist (card char(2) )


end

if @fDebug = 'Y'
	begin

	select @gameid '@gameid', @roundNumber '@roundNumber', @roundID '@roundID', @cardPlayed '@cardPlayed'

	select 'played card = ',* 
	FROM playerRoundCards x
	where x.roundid = @roundID
	and cardShortName = @cardPlayed

	select 'playerRoundCards (unplayed)',* 
	FROM playerRoundCards x
	where x.roundid = @roundID
	and cardPlayed = 'n'

	end

update x set cardPlayed = 'Y'
FROM playerRoundCards x
where x.roundid = @roundID
and cardShortName = @cardPlayed

go
-----------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'writeTrickWinner' )
drop proc writeTrickWinner
go
create proc writeTrickWinner @gameID bigint, @roundNumber bigint, @playerid int, @fDebug char(1) = 'N'
as
declare @roundID bigint
SELECT @roundID = roundid FROM gameRounds where gameid = @gameid and roundnumber = @roundNumber

if @fDebug = 'Y'
select 'playerRounds',@playerid '@playerid', * from playerRounds where @roundID = roundID 

-- update the scores as we go....
--round scores...
update playerRounds 
set tricksWon = tricksWon + 1
where @roundID = roundID and @playerid = playerid

if @fDebug = 'Y'
select 'playerRounds',* from playerRounds where @roundID = roundID 

go
-----------------------------------------------------------------------------------------------
if exists (select 'procs',* from sysobjects where type = 'P' and name like 'calcGameScores' )
drop proc calcGameScores
go
create proc calcGameScores @gameID bigint, @roundnumber int, @fDebug char(1) = 'N'
as

if @fDebug = 'Y'
SELECT 'before round score calc',* 
from playerRounds pr 
where pr.gameid = @gameID
order by pr.roundNumber, pr.playerId

update pr set roundPoints = 0
from playerRounds pr 
where pr.gameid = @gameID
and pr.roundNumber = @roundnumber
and roundPoints < 0

update pr set pr.roundPoints = case when trickswon = target then trickswon + 10 else trickswon end
from playerRounds pr 
where pr.gameid = @gameID
and pr.roundNumber = @roundnumber

if @fDebug = 'Y'
SELECT 'after round score calc',* 
from playerRounds pr 
where pr.gameid = @gameID
order by pr.roundNumber, pr.playerId

declare @roundCounter int, @playerCounter int, @totalRoundPoints int
select @roundCounter = 1
while (@roundCounter < @roundnumber + 1 )
begin

		select @playerCounter  = 1

		while (@playerCounter < 5 )
		begin
				select @totalRoundPoints = 0

				select @totalRoundPoints = sum(isnull(roundPoints,0))
				from playerRounds pr 
				where pr.gameid = @gameID
				and playerId = @playerCounter
				and pr.roundNumber <= @roundCounter

				if @fDebug = 'Y'
				begin
				select 'loop params', @gameID '@gameID', @playerCounter '@playerCounter', @roundCounter '@roundCounter', @totalRoundPoints '@totalRoundPoints'
				select 'playerRounds',* 
				from playerRounds pr 
				where pr.gameid = @gameID
				and playerId = @playerCounter
				and pr.roundNumber <= @roundCounter
				end

				update pr set gamePoints = @totalRoundPoints
				from playerRounds pr 
				where pr.gameid = @gameID
				and playerId = @playerCounter
				and pr.roundNumber = @roundCounter

				select @playerCounter += 1
		end

		select @roundCounter += 1

end
go
---------------------------