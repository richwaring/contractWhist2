using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using contractWhist2.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using static contractWhist2.Data.sqlQueries;

namespace contractWhist2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public int currentGameId = 0;
        public int currentRoundNumber = 0;
        public string trumpcard = "";
        public List<List<long>> scoreboardValues = new List<List<long>>();
        public int totaloftargets;

        public player p1 = new player(1);
        public player p2 = new player(2);
        public player p3 = new player(3);
        public player p4 = new player(4);

        public string p1PlaysCard = "Red_back";
        public string p2PlaysCard = "Red_back";
        public string p3PlaysCard = "Red_back";
        public string p4PlaysCard = "Red_back";
        public int whoseGoIsIt = 0;
        public card leadCard = new card ("NONE");
        public card winningCard;
        public card winningTrump;
        public List<string> usersInSuitOptions = new List<string>();

        public IActionResult Index(string id = "")
        {
            if (currentGameId == 0 && id == "")
            {
                newWhistGame();
                dealCards();
            }
            else
            {
                string[] idArray = id.Split("-");
                currentGameId = Int32.Parse(idArray[0]);
                currentRoundNumber = Int32.Parse(idArray[1]);
            }

            getUserCardsAndMotivation();
            if (trumpcard == "") 
            {   dealCards();
                getUserCardsAndMotivation();
            }

            getGameScoreBoardInfo();

            whoseGoIsIt = ((currentRoundNumber + 2) % 4) + 1;

            robotsSetTargets("NONE", "NONE", "NONE");

            setViewbagVariables();

            return View();
        }

        public IActionResult userSubmitsTarget(string id = "")
        {
            //1. get the variables where we can use them more easily...
            var userTargetArray = id.Split("-").ToArray();
            currentGameId = Int32.Parse(userTargetArray[0]);
            currentRoundNumber = Int32.Parse(userTargetArray[1]);
            string p4Target = userTargetArray[2];
            whoseGoIsIt = ((currentRoundNumber + 2) % 4) + 1;

            //2. write the user's target to the db...
            writeUserTargetToDatabase("4", p4Target);

            //3. get the robots to say how may tricks they think they'll win this time....
            getUserCardsAndMotivation();
            allRobotsSetTargets();

            //4. if robots need to lead, let them lead....
            List<string> newTrickLeaders = new List<string>();
            if (whoseGoIsIt != 4)
            {
                newTrickLeaders = leadCards().ToList();
                getUsersValidFollows();
            }

            return new JsonResult(new { runningTotal = totaloftargets, p1AimsFor = p1.target, p2AimsFor = p2.target, p3AimsFor = p3.target, nextTrickCards = newTrickLeaders, validUserFollows = usersInSuitOptions });
        }

        public void allRobotsSetTargets()
        {
                    setRobotTarget("NONE", p1);
                    setRobotTarget("NONE", p2);
                    setRobotTarget("NONE", p3);
        }

        public void robotsSetTargets(string p1Target, string p2Target, string p3Target)
        {
            switch (whoseGoIsIt)
            {
                case 1:
                    setRobotTarget(p1Target, p1);
                    setRobotTarget(p2Target, p2);
                    setRobotTarget(p3Target, p3);
                    break;
                case 2:
                    setRobotTarget(p2Target, p2);
                    setRobotTarget(p3Target, p3);
                    break;
                case 3:
                    setRobotTarget(p3Target, p3);
                    break;
            }
        }

        public void setRobotTarget(string pTarget, player thisPlayer)
        {
            var target = 0;

            if (pTarget != "NONE") {thisPlayer.target = Int32.Parse(pTarget);}
            else 
            {
                List<card> winners = new List<card>();

                winners.AddRange(thisPlayer.cards.Where(x => (x.suit == trumpcard.Substring(1, 1) 
                                                                || x.valueInSuit > 11 )));
                thisPlayer.target = winners.Count > 0 ? winners.Count - 1 : 0;
                writeUserTargetToDatabase(thisPlayer.playerId.ToString(), thisPlayer.target.ToString());
            }

        }

        public IActionResult playCard(string id)

        {
            //0. split the inbound id string into parameters
            var playedCardArray = id.Split("-").ToArray();
            currentGameId = Int32.Parse(playedCardArray[0]);
            currentRoundNumber = Int32.Parse(playedCardArray[1]);

            //1. Old trick: follow suit and figure out who won... 
            tellDbCardPlayed(currentGameId, currentRoundNumber, playedCardArray[2]);
            getUserCardsAndMotivation();

            String[] oldTrickFollowers = robotsFollowLead(playedCardArray);

            //2. new trick: play robot cards
            getUserCardsAndMotivation();

            string lastTrick = "N";
            string lastRound = "N";

            List<string> newTrickLeaders = new List<string>();
            if (p1.cards != null) {
                newTrickLeaders = leadCards().ToList();

                //3. get the user's valid following cards if a robot has led...
                getUsersValidFollows();
            }
            else {
                newTrickLeaders.Add("NONE");
                newTrickLeaders.Add("NONE");
                newTrickLeaders.Add("NONE");
                lastTrick = "Y";
                calculateRoundScores();
                lastRound = currentRoundNumber == 7 ? "Y" : "N";

            }

            List<long> finalScores = new List<long>();

            if (lastRound == "Y" )          {
                getGameScoreBoardInfo();
                finalScores.Add(scoreboardValues[6][3]);
                finalScores.Add(scoreboardValues[6][6]);
                finalScores.Add(scoreboardValues[6][9]);
                finalScores.Add(scoreboardValues[6][12]);
 }

            //4. gets lists of scores and targets to report back to the front end...
            int[] playerTricksWon = { p1.tricksWon, p2.tricksWon , p3.tricksWon , p4.tricksWon };
            int[] playerTargets = { p1.target, p2.target, p3.target, p4.target };
            int[] playerScores = { p1.scoreSoFar, p2.scoreSoFar, p3.scoreSoFar, p4.scoreSoFar };
            

            //4. return results...
            return new JsonResult(new { 
                                        thisTrickCards = oldTrickFollowers, 
                                        nextTrickCards = newTrickLeaders, 
                                        winner = "p" + whoseGoIsIt.ToString(), 
                                        playerTricksWon = playerTricksWon, 
                                        playerTargetList = playerTargets, 
                                        validUserFollows = usersInSuitOptions, 
                                        initialiseNewRound = lastTrick, 
                                        endGame = lastRound, 
                                        latestPlayerScores = playerScores, 
                                        finalScoreBoardValues = finalScores
            });
        }

        public void getUsersValidFollows()
        {
            usersInSuitOptions = whoseGoIsIt == 4 ?
                    new List<string>() :
                    turnCardsIntoStrings(filterCardsBySuit(p4.cards, leadCard.suit));
        }

        public String[] leadCards()
        {

            string p1plays = "NONE";
            string p2plays = "NONE";
            string p3plays = "NONE";

            //getUserCards(currentGameId, currentRoundNumber);

            switch (whoseGoIsIt)
            {
                case 1:
                    p1plays = playLeadCard(p1);
                    p2plays = chooseAndPlayCard(p2).name;
                    p3plays = chooseAndPlayCard(p3).name;
                    break;
                case 2:
                    p2plays = playLeadCard(p2);
                    p3plays = p3plays = chooseAndPlayCard(p3).name;
                    break;
                case 3:
                    p3plays = playLeadCard(p3);
                    break;
                case 4:
                    break;
            }

            String[] leadingCards = { p1plays, p2plays, p3plays };

            return leadingCards;
        }

        public string playLeadCard(player thisPlayer)
        {
            card cardToPlay;

            if (thisPlayer.wantsToWinTricks)
                // if you want to win trcks, play your highest card....
            {
                cardToPlay = thisPlayer.cards.Aggregate((l, r) => r.valueInSuit > l.valueInSuit ? r : l);
            }
            else
            {
                //...otherwise play your lowest card
                cardToPlay = thisPlayer.cards.Aggregate((l, r) => r.valueInSuit > l.valueInSuit ? l : r);
            }

            thisPlayer.cards.Remove(cardToPlay);
            tellDbCardPlayed(currentGameId, currentRoundNumber, cardToPlay.name);
            leadCard = cardToPlay;
            return cardToPlay.name;
        }


        public card chooseAndPlayFollowingCard(int leadUser, player thisPlayer)
        {
            // if you've already played a card, return "NONE" card....
            if (leadUser <= thisPlayer.playerId) { return new card("NONE"); }

            card cardToPlay = chooseAndPlayCard(thisPlayer);

            return cardToPlay;
        }

        public card followSuit(player thisPlayer) {

            card cardToPlay;

            //find your highest card in suit...
            var myHighestCardInSuit = highestCardInSuit(thisPlayer.cards, leadCard.suit);

            //does it beat the current lead card?
            bool weCanBeatIt = winningCard != null ? myHighestCardInSuit.valueInSuit > winningCard.valueInSuit :
                myHighestCardInSuit.valueInSuit > leadCard.valueInSuit;

            //1a. if you want to win, and you can beat this card, and no-one's trumped, play high...
            if (thisPlayer.wantsToWinTricks && weCanBeatIt && winningTrump == null)
            { 
                cardToPlay = myHighestCardInSuit;
                winningCard = cardToPlay;
            }

            //1b... otherwise play low
            else { cardToPlay = lowestCardInSuit(thisPlayer.cards, leadCard.suit); }

            return cardToPlay;
        }

        public card chooseAndPlayCard (player thisPlayer)
        {
            card cardToPlay;

            bool youCanFollowSuit = filterCardsBySuit(thisPlayer.cards, leadCard.suit).Count() > 0;

            //1. if you can follow suit, follow suit
            if (youCanFollowSuit)
            {
                cardToPlay = followSuit(thisPlayer);
            }
            else
            {
                //2. do you have a winning trump?
                card myGoodTrump = findWinningTrump(thisPlayer);

                //2b. if you want to win, and you have a winning trump, play it
                if (thisPlayer.wantsToWinTricks == true && myGoodTrump.name != "NONE")
                {
                    cardToPlay = myGoodTrump;
                    winningTrump = myGoodTrump;
                }

                //2b. otherwise throw away your lowest card
                else
                {
                    cardToPlay = findThrowAwayCard(thisPlayer); 
                }
            }

            thisPlayer.cards.Remove(cardToPlay);
            tellDbCardPlayed(currentGameId, currentRoundNumber, cardToPlay.name);
            return cardToPlay;
        }

        public card findWinningTrump(player thisPlayer)
        {
            card myGoodTrump = new card("NONE");
            List<card> myTrumps = filterCardsBySuit(thisPlayer.cards, trumpcard);
            List<card> cardsThatBeatTrumper = new List<card>();

            //if you have trumps and someone else has trumped, can you beat it?...
            if (myTrumps.Count > 0 && winningTrump != null)
            {
                cardsThatBeatTrumper.AddRange(myTrumps.Where(x => x.valueInSuit > winningTrump.valueInSuit));

                myGoodTrump = lowestCardInSuit(cardsThatBeatTrumper, trumpcard);
                myGoodTrump = myGoodTrump == null ? new card("NONE") : myGoodTrump;
            }

            //if you have trumps and no-one's played a trump, what's your lowest trump?...
            if (myTrumps.Count > 0 && winningTrump == null)
            {
                myGoodTrump = lowestCardInSuit(thisPlayer.cards, trumpcard);
            }

            return myGoodTrump;
        }

        public int figureOutWhoLead(string p1Input, string p2Input, string p3Input)
        {

            int leadUser = 4;

            leadUser = p3Input != "NONE" ? 3 : leadUser; //if p3 played smth, he might have gone 1st...
            leadUser = p2Input != "NONE" ? 2 : leadUser; //if p2 played smth, he might have gone 1st (and p3 didn't)...
            leadUser = p1Input != "NONE" ? 1 : leadUser; //if p1 played smth, he must have gone 1st (and the other two didn't)...

            return leadUser;

        }

        public String[] robotsFollowLead(string[] playedCardArray)
        {
            string p4plays = playedCardArray[2];
            string p1plays = playedCardArray[3];
            string p2plays = playedCardArray[4];
            string p3plays = playedCardArray[5];

            //1. Who lead?
            int leadUser = figureOutWhoLead(p1plays, p2plays, p3plays);

            //2. What was the lead card?
            switch (leadUser)
            {
                case 4: leadCard.setCardIdentity(p4plays); break;
                case 3: leadCard.setCardIdentity(p3plays); break;
                case 2: leadCard.setCardIdentity(p2plays); break;
                case 1: leadCard.setCardIdentity(p1plays); break;
            }

            //3. What cards were played?
            List<card> cardsSoFar = new List<card> { new card(p1plays), new card(p2plays), new card(p3plays), new card(p4plays) };
            List<card> cardsFollowingSuitSoFar = filterCardsBySuit(cardsSoFar, leadCard.suit);

            List<card> trumpsPlayedSoFar = filterCardsBySuit(cardsSoFar, trumpcard);
            winningTrump = highestCardInSuit(trumpsPlayedSoFar, trumpcard.Substring(1, 1));

            //4.  Players who need to play a card, do so...
            card p1FollowsWith = chooseAndPlayFollowingCard(leadUser, p1);
            card p2FollowsWith = chooseAndPlayFollowingCard(leadUser, p2);
            card p3FollowsWith = chooseAndPlayFollowingCard(leadUser, p3);

            //5. Who won the trick?
            whoseGoIsIt = figureOutWhoWon(leadCard, playedCardArray, new List<card> { p1FollowsWith, p2FollowsWith, p3FollowsWith });
            tellDbTrickWon(currentGameId, currentRoundNumber, whoseGoIsIt);

            return new[] { p1FollowsWith.name, p2FollowsWith.name, p3FollowsWith.name };
        }

        public int figureOutWhoWon(card leadCard, string[] playedCardArray, List<card> followingCardList)
        {
            //1. bring together the cards values for this trick (from robots who played before and after the user)...
            string p4plays = playedCardArray[2];
            string p1plays = playedCardArray[3];
            string p2plays = playedCardArray[4];
            string p3plays = playedCardArray[5];

            Dictionary<card, int> cardsPlayed = new Dictionary<card, int> {
                { p1plays == "NONE" ? followingCardList[0] : new card(p1plays), 1},
                { p2plays == "NONE" ? followingCardList[1] : new card(p2plays), 2},
                { p3plays == "NONE" ? followingCardList[2] : new card(p3plays), 3},
                { new card(p4plays), 4}
            };

            //2. if trumps were played, the winner is whoever played the highest trump...
            Dictionary<card, int> trumps = new Dictionary<card, int>();
            foreach (var x in cardsPlayed)
            {
                if (x.Key.suit == trumpcard.Substring(1, 1))
                { trumps.Add(x.Key, x.Value); }
            }

            if (trumps.Count > 0)
            {
                return trumps.Aggregate((l, r) => l.Key.valueInSuit > r.Key.valueInSuit ? l : r).Value;
            }
            else
            //3. otherwise, the winner is the person who followed suit with the highest card....
            {
                Dictionary<card, int> cardsInSuit = new Dictionary<card, int>();
                foreach (var x in cardsPlayed)
                {
                    if (x.Key.suit == leadCard.suit)
                    { cardsInSuit.Add(x.Key, x.Value); }
                }

                return cardsInSuit.Aggregate((l, r) => l.Key.valueInSuit > r.Key.valueInSuit ? l : r).Value;
            }
        }

        public card findThrowAwayCard(player thisPlayer)
        {
            List<card> nonTrumps = filterCardsBySuit(thisPlayer.cards, "nontrumps");

            if (nonTrumps.Count() > 0)
            {
                if (nonTrumps.Count() == 1) { return nonTrumps[0]; }

                return nonTrumps.Aggregate((l, r) => l.valueInSuit < r.valueInSuit? l : r);
            }
            else
            {
                List<card> trumps = filterCardsBySuit(thisPlayer.cards, trumpcard);
                if (trumps.Count == 1) { return trumps[0]; }

                return trumps.Aggregate((l, r) => l.valueInSuit < r.valueInSuit ? l : r);
            }
        }

        public List<card> filterCardsBySuit(List<card> cardsToBeFiltered, string cardOrSuitToMatch)
        {
            List<card> filteredList = new List<card>();

            if (cardOrSuitToMatch == "nontrumps")
            {
                filteredList.AddRange(cardsToBeFiltered.Where(x => x.suit != trumpcard.Substring(1,1)));
            }
            else
            {
                cardOrSuitToMatch = cardOrSuitToMatch.Length == 1 ? cardOrSuitToMatch : cardOrSuitToMatch.Substring(1, 1);

                filteredList.AddRange(cardsToBeFiltered.Where(x => x.suit == cardOrSuitToMatch));
            }
            return filteredList;
        }

        public void tellDbCardPlayed(int thisGameId, int thisRoundNumber, string playedCard)
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", thisGameId.ToString());
            sp_params.Add("@roundNumber", thisRoundNumber.ToString());
            sp_params.Add("@cardPlayed", playedCard);
            executeWriteOnlySqlSp("userPlaysCard", sp_params);
        }

        public void calculateRoundScores()
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", currentGameId.ToString());
            sp_params.Add("@roundnumber", currentRoundNumber.ToString());
            executeWriteOnlySqlSp("calcGameScores", sp_params);
        }

        public void tellDbTrickWon(int thisGameId, int thisRoundNumber, int winningPlayerId)
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", thisGameId.ToString());
            sp_params.Add("@roundNumber", thisRoundNumber.ToString());
            sp_params.Add("@playerid", winningPlayerId.ToString());
            executeWriteOnlySqlSp("writeTrickWinner", sp_params);
        }

        public void setViewbagVariables()
        {
            ViewBag.p1Cardlist = p1.cardsListAsString();
            ViewBag.p2Cardlist = p2.cardsListAsString();
            ViewBag.p3Cardlist = p3.cardsListAsString();
            ViewBag.p4Cardlist = p4.cardsListAsString();
            ViewBag.p4Card1 = p4.cards.Count < 1 ? null : p4.cards[0].name;
            ViewBag.p4Card2 = p4.cards.Count < 2 ? null : p4.cards[1].name;
            ViewBag.p4Card3 = p4.cards.Count < 3 ? null : p4.cards[2].name;
            ViewBag.p4Card4 = p4.cards.Count < 4 ? null : p4.cards[3].name;
            ViewBag.p4Card5 = p4.cards.Count < 5 ? null : p4.cards[4].name;
            ViewBag.p4Card6 = p4.cards.Count < 6 ? null : p4.cards[5].name;
            ViewBag.p4Card7 = p4.cards.Count < 7 ? null : p4.cards[6].name;
            ViewBag.p1PlaysCard = p1PlaysCard;
            ViewBag.p2PlaysCard = p2PlaysCard;
            ViewBag.p3PlaysCard = p3PlaysCard;
            ViewBag.p4PlaysCard = p4PlaysCard;

            ViewBag.p1Target = p1.target;
            ViewBag.p1TricksWon = p1.tricksWon;
            ViewBag.p2Target = p2.target;
            ViewBag.p2TricksWon = p2.tricksWon;
            ViewBag.p3Target = p3.target;
            ViewBag.p3TricksWon = p3.tricksWon;
            ViewBag.p4Target = p4.target;
            ViewBag.p4TricksWon = p4.tricksWon;

            ViewBag.whoseGoIsIt = "p" + whoseGoIsIt.ToString();
            ViewBag.game = currentGameId;
            ViewBag.trumpcard_img = "pics/cards/" + trumpcard + ".jpg";
            ViewBag.round = currentRoundNumber;
            ViewBag.scoreboardValues = scoreboardValues;
            ViewBag.totaloftargets = totaloftargets;
        }



        public void dealCards()
        {
            runSql(string.Format("exec dealCards {0}, {1}", currentGameId, currentRoundNumber));
        }

        public void getUserCardsAndMotivation()
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", currentGameId.ToString());
            sp_params.Add("@roundNumber", currentRoundNumber.ToString());

            DataTable myDataTable = readSqlSp("getUserCards", sp_params);

            foreach (DataRow row in myDataTable.Rows)
            {
                switch (row[0])
                {
                    case 1: p1.cards = getCardsFromRow(row);
                            readRobotMotivation(row, p1);
                            break;
                    case 2: p2.cards = getCardsFromRow(row);
                            readRobotMotivation(row, p2);
                            break;
                    case 3: p3.cards = getCardsFromRow(row);
                            readRobotMotivation(row, p3);
                            break;
                    case 4: p4.cards = getCardsFromRow(row);
                            readRobotMotivation(row, p4);
                            break;
                    case 9:
                        if (row[1] == null) { trumpcard = null; }
                        else
                        { trumpcard = row[1].ToString(); }
                        break;
                };
            }
        }

        public List<card> getCardsFromRow(DataRow row)
        {
            List<string> stringCardList = new List<string>();

            if (row.IsNull("cards")) { return null; }
            else
            { stringCardList = row[1].ToString().Split(", ").ToList(); }

            return turnStringsIntoCards(stringCardList);
        }

        public void readRobotMotivation(DataRow row, player thisPlayer)
        {
            thisPlayer.wantsToWinTricks = Int32.Parse(row[2].ToString()) != Int32.Parse(row[3].ToString());
            thisPlayer.target = Int32.Parse(row[2].ToString());
            thisPlayer.tricksWon = Int32.Parse(row[3].ToString());
            thisPlayer.scoreSoFar = Int32.Parse(row[3].ToString());
        }

        public List<card> turnStringsIntoCards(List<string> stringList)
        {
            List<card> cardList = new List<card>();

            foreach (var y in stringList)
            {
                cardList.Add(new card(y));
            }
                return cardList;
        }

        public List<string> turnCardsIntoStrings(List<card> cardList)
        {
            List<string> stringList = new List<string>();

            foreach (var y in cardList)
            {
                stringList.Add(y.name);
            }
            return stringList;
        }

        public void writeUserTargetToDatabase(string userId, string target)
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", currentGameId.ToString());
            sp_params.Add("@roundNumber", currentRoundNumber.ToString());
            sp_params.Add("@userId", userId);
            sp_params.Add("@target", target);
            totaloftargets = executeSqlSp("Scoreboard_userTarget", sp_params, "@totaloftargets");
        }

        public void newWhistGame()
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@rounds", "7");
            currentGameId = executeSqlSp("Scoreboard_newGame", sp_params, "@gameid");
            currentRoundNumber = 1;
        }

        public card topOrBottomInSuit(List<card> hand, string followsuit, string highestOrLowest)

        {
            List<card> myCardsInSuit = filterCardsBySuit(hand, followsuit);
            if (myCardsInSuit.Count == 0) { return null; }

            if (myCardsInSuit.Count == 1) { return myCardsInSuit[0]; }

            if (highestOrLowest == "highest")
            { return myCardsInSuit.Aggregate((l, r) => l.valueInSuit > r.valueInSuit ? l : r); }
            else
            { return myCardsInSuit.Aggregate((l, r) => l.valueInSuit < r.valueInSuit ? l : r); }
        }

        public card highestCardInSuit(List<card> hand, string inSuit)
        {
            return topOrBottomInSuit(hand, inSuit, "highest");
        }

        public card lowestCardInSuit(List<card> hand, string inSuit)
        {
            return topOrBottomInSuit(hand, inSuit, "lowest");
        }

        public IActionResult CatPictures()
        {
            return View();
        }


        public void getGameScoreBoardInfo()
        {
            Dictionary<string, string> sp_params = new Dictionary<string, string>();
            sp_params.Add("@gameID", currentGameId.ToString());

            DataTable myDataTable = readSqlSp("Scoreboard_showGameScoreboard", sp_params);

            scoreboardValues.Clear();

            foreach (DataRow row in myDataTable.Rows)
            {
                List<long> Row = new List<long>();

                //round number
                Row.Add(Int32.Parse(row[0].ToString()));

                //P1 target, roundpoints, totalpoints
                Row.Add(Int32.Parse(row[1].ToString()));
                Row.Add(Int32.Parse(row[2].ToString()));
                Row.Add(Int32.Parse(row[3].ToString()));

                //P2 target, roundpoints, totalpoints
                Row.Add(Int32.Parse(row[4].ToString()));
                Row.Add(Int32.Parse(row[5].ToString()));
                Row.Add(Int32.Parse(row[6].ToString()));

                //P3 target, roundpoints, totalpoints
                Row.Add(Int32.Parse(row[7].ToString()));
                Row.Add(Int32.Parse(row[8].ToString()));
                Row.Add(Int32.Parse(row[9].ToString()));

                //P4 target, roundpoints, totalpoints
                Row.Add(Int32.Parse(row[10].ToString()));
                Row.Add(Int32.Parse(row[11].ToString()));
                Row.Add(Int32.Parse(row[12].ToString()));

                scoreboardValues.Add(Row);
            }
        }

        public class player
        {
            public bool wantsToWinTricks = true;
            public int target = -1;
            public int tricksWon = 0;
            public int pointsWon = 0;
            public List<card> cards;
            public int playerId;
            public int scoreSoFar = -1;

            public player(int PlayerId)
            {
                playerId = PlayerId;
            }

            public string cardsListAsString()
            {
                string listOfCardNames = "";

                if (cards.Count > 0)
                {
                    foreach (var x in cards)
                    {
                        listOfCardNames += x.name + ", ";
                    }
                }
                return listOfCardNames;
            }

            public List<string> cardsList()
            {
                List<string> listOfCardNames = new List<string>();

                foreach (var x in cards)
                {
                    listOfCardNames.Add(x.name);
                }

                return listOfCardNames;
            }
        }

        public class card
        {
            public string name;
            public int valueInSuit;
            public string suit;

            public card(string cardname)
            {
                setCardIdentity(cardname);
            }

            public void setCardIdentity(string cardname)
            {
                name = cardname;
                suit = cardname.Substring(1, 1);

                string cardKind = cardname.Substring(0, 1);

                switch (cardKind)
                {
                    case "N": valueInSuit = 0; break;
                    case "T": valueInSuit = 10; break;
                    case "J": valueInSuit = 11; break;
                    case "Q": valueInSuit = 12; break;
                    case "K": valueInSuit = 13; break;
                    case "A": valueInSuit = 14; break;
                    default: valueInSuit = Int32.Parse(cardKind); break;
                }
            }

        }
    }
}
