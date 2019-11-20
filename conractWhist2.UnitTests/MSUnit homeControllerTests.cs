using Microsoft.VisualStudio.TestTools.UnitTesting;
using contractWhist2.Controllers;
using static contractWhist2.Controllers.HomeController;
using System;
using System.Collections.Generic;

namespace contractWhist2.MSUnitTests
{
    [TestClass]
    public class homeControllerTests
    {
        [TestMethod]
        public void findWinningTrump_robotCanOverTrump_returnsLowestOvertrump()
        {
            var hc = new contractWhist2.Controllers.HomeController();

            //Arrange
            var myPlayer = new player(1);
            addGenericCardList(myPlayer);
            hc.trumpcard = "2H";
            hc.winningTrump = new card("8H");

            //Act
            card winningTrump = hc.findWinningTrump(myPlayer);

            //Assert
            Assert.AreEqual(winningTrump.name, "9H");
        }

        [TestMethod]
        public void findWinningTrump_robotCannotOverTrump_returnsNone()
        {
            var hc = new contractWhist2.Controllers.HomeController();

            //Arrange
            var myPlayer = new player(1);
            addGenericCardList(myPlayer);
            hc.trumpcard = "2H";
            hc.winningTrump = new card("JH");

            //Act
            card winningTrump = hc.findWinningTrump(myPlayer);

            //Assert
            Assert.AreEqual(winningTrump.name, "NONE");
        }

        [TestMethod]
        public void findWinningTrump_robotHasNoTrumps_returnsNone()
        {
            var hc = new contractWhist2.Controllers.HomeController();

            //Arrange
            var myPlayer = new player(1);
            addGenericCardList(myPlayer);
            hc.trumpcard = "2D";
            hc.winningTrump = new card("JD");

            //Act
            card winningTrump = hc.findWinningTrump(myPlayer);

            //Assert
            Assert.AreEqual(winningTrump.name, "NONE");
        }

        [TestMethod]
        public void findWinningTrump_noTrumpPlayed_returnsLowestTrump()
        {
            var hc = new contractWhist2.Controllers.HomeController();

            //Arrange
            var myPlayer = new player(1);
            addGenericCardList(myPlayer);
            hc.trumpcard = "2H";
//            hc.winningTrump = new card("JD");

            //Act

            card winningTrump = hc.findWinningTrump(myPlayer);

            //Assert
            Assert.AreEqual(winningTrump.name, "3H");
        }


        public void addGenericCardList(player myPlayer)
            {
            myPlayer.cards = new List<card>();
            myPlayer.cards.Add(new card("9H"));
            myPlayer.cards.Add(new card("7H"));
            myPlayer.cards.Add(new card("5H"));
            myPlayer.cards.Add(new card("3H"));
            myPlayer.cards.Add(new card("7C"));
            myPlayer.cards.Add(new card("7D"));
            myPlayer.cards.Add(new card("7S"));
            }

    }
}
