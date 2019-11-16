// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

picClickCounter = 2;
ajax = 0;
goPauseTimer = 500;
thisGameRound = document.getElementById("gameRound").innerText.split(", ")
thisGameId = thisGameRound[0].substr(5);
thisRoundId = Number(thisGameRound[1].substr(6));
sumOfTargets = -1;
p1Target = "NONE";
p2Target = "NONE";
p3Target = "NONE";


function enterEqualsSubmit() {
    if (event.keyCode === 13) {
        userTargetClick();
    }
}

function userTargetClick()
{
    var userInput = document.getElementById("userTarget").value;

    if (isNaN(userInput) || userInput < 0 || userInput == sumOfTargets || userInput > (8 - thisRoundId)) {
        document.getElementById("modalValidationMsg").innerHTML =
            "Please enter a number between 0 and " + (8 - thisRoundId) + ", (e.g. not \"" + userInput + "\")";
    }
    else
    {
        collectRobotTargets();

        document.getElementById("p4Target").innerHTML = userInput;

        document.getElementById("modalValidationMsg").innerHTML = "";
        document.getElementById('targetModal').style.display = 'none';
        
        submitUserTarget(userInput);
    }
}

function collectRobotTargets()
{
    p1Target = collectRobotTarget("p1Target");
    p2Target = collectRobotTarget("p2Target");
    p3Target = collectRobotTarget("p3Target");

    console.log("collectRobotTargets p1Target = " + p1Target + ", p2Target = " + p2Target + ", p3Target = " + p3Target);
}

function collectRobotTarget(elementId ) {

    var robotTarget = document.getElementById(elementId).innerHTML;
    robotTarget = robotTarget == "_" ? "NONE" : robotTarget;
    return robotTarget
}

function submitUserTarget(userTarget) {

    var url = "home/userSubmitsTarget?id=" + thisGameId + "-" + thisRoundId + "-" + userTarget + 
        "-" +p1Target + "-" + p2Target + "-" + p3Target ;

    console.log("submitUserTarget fired, url = " + url);

    submitAjaxrequest(url, handleTargetSubmitResponse);
}

function handleTargetSubmitResponse()
{
    if (ajax.readyState == "4" && ajax.status == "200") {

        console.log("handleTargetSubmitResponse fired, ajax.responseText = " + ajax.responseText);

        var jsonResponse = JSON.parse(ajax.responseText);

        //console.log("jsonResponse.nextTrickCards = " + jsonResponse.nextTrickCards + ", jsonResponse.nextTrickCards.length = " + jsonResponse.nextTrickCards.length);

        document.getElementById("p1Target").innerHTML = jsonResponse.p1AimsFor;
        document.getElementById("p2Target").innerHTML = jsonResponse.p2AimsFor;
        document.getElementById("p3Target").innerHTML = jsonResponse.p3AimsFor;

        if (jsonResponse.nextTrickCards.length > 0) {
//            console.log("Robots should lead");
            letRobotsLead(jsonResponse);
        }

    }
}

function dblClickMyCard(imgElement){
    var p4CardPlayed = imgElement.id;
    console.log("p4CardPlayed = " + p4CardPlayed);
    document.getElementById(p4CardPlayed).style = "display:none";
    setImageSource("p4PlaysCard", p4CardPlayed);

    var p1CardPlayed = document.getElementById("p1PlaysCard").alt;
    var p2CardPlayed = document.getElementById("p2PlaysCard").alt;
    var p3CardPlayed = document.getElementById("p3PlaysCard").alt;

    var url = "home/playCard?id=" + thisGameId + "-" + thisRoundId + "-" + p4CardPlayed +
        "-" + p1CardPlayed + "-" + p2CardPlayed + "-" + p3CardPlayed;

    console.log("dblClickMyCard fired, url = " + url);

    submitAjaxrequest(url, handlePlayedCardResponse)

    toggleUserCardDblClick("off");
}

function submitAjaxrequest(url, handlingFunction) {
    ajax = new XMLHttpRequest();
    ajax.onreadystatechange = handlingFunction;
    ajax.open("GET", url, true);
    ajax.send();
}

function letRobotsLead(jsonResponse){

    var delayIncrement = 1

    displayNextTrickCard(jsonResponse, 1, delayIncrement);
    delayIncrement = jsonResponse.nextTrickCards[0] != "NONE" ? delayIncrement + 1 : delayIncrement;

    displayNextTrickCard(jsonResponse, 2, delayIncrement);
    delayIncrement = jsonResponse.nextTrickCards[1] != "NONE" ? delayIncrement + 1 : delayIncrement;

    displayNextTrickCard(jsonResponse, 3, delayIncrement);
    delayIncrement = jsonResponse.nextTrickCards[2] != "NONE" ? delayIncrement + 1 : delayIncrement;

    setTimeout(function () {
        setImageSource("goArrow", "p4Arrow.png");
        enableUserCardDblClick(jsonResponse.validUserFollows);
    }, delayIncrement * 500)
};

function handlePlayedCardResponse()
{
    if (ajax.readyState == "4" && ajax.status == "200") {

        console.log("handlePlayedCardResponse fired, ajax.responseText = " + ajax.responseText);

        var jsonResponse = JSON.parse(ajax.responseText);
//        var winner = JSON.parse(ajax.responseText).winner

        var delayIncrement = 1

        displayReceivedCard(jsonResponse, 1, delayIncrement);
        delayIncrement = jsonResponse.thisTrickCards[0] != "NONE" ? delayIncrement + 1 : delayIncrement;
        
        displayReceivedCard(jsonResponse, 2, delayIncrement);
        delayIncrement = jsonResponse.thisTrickCards[1] != "NONE" ? delayIncrement + 1 : delayIncrement;
        
        displayReceivedCard(jsonResponse, 3, delayIncrement);
        delayIncrement = jsonResponse.thisTrickCards[2] != "NONE" ? delayIncrement + 1 : delayIncrement;

        setTimeout(function () {
            resetAllPlayCards();
            setImageSource("goArrow", JSON.parse(ajax.responseText).winner + "Arrow.png");
            updateUserScores(1, jsonResponse.playerTricksWon[0], jsonResponse.playerTargetList[0]);
            updateUserScores(2, jsonResponse.playerTricksWon[1], jsonResponse.playerTargetList[1]);
            updateUserScores(3, jsonResponse.playerTricksWon[2], jsonResponse.playerTargetList[2]);
            updateUserScores(4, jsonResponse.playerTricksWon[3], jsonResponse.playerTargetList[3]);
        }, 500 * delayIncrement++);

        delayIncrement ++;

        displayNextTrickCard(jsonResponse, 1, delayIncrement);
        delayIncrement = jsonResponse.nextTrickCards[0] != "NONE" ? delayIncrement + 1 : delayIncrement;

        displayNextTrickCard(jsonResponse, 2, delayIncrement);
        delayIncrement = jsonResponse.nextTrickCards[1] != "NONE" ? delayIncrement + 1 : delayIncrement;

        displayNextTrickCard(jsonResponse, 3, delayIncrement);
        delayIncrement = jsonResponse.nextTrickCards[2] != "NONE" ? delayIncrement + 1 : delayIncrement;

        setTimeout(function () {
            setImageSource("goArrow", "p4Arrow.png");
            enableUserCardDblClick(jsonResponse.validUserFollows);
        }, delayIncrement * 500)

        delayIncrement += 1.5;

        setTimeout(function () {
            if (jsonResponse.initialiseNewRound == "Y") { window.location.href = "?id=" + thisGameId + "-" + (thisRoundId + 1); }
        }, delayIncrement * 500)
        
    }
}

function enableUserCardDblClick(validCardList) {

    if (validCardList.length > 0) {

        for (x = 0; x < validCardList.length; x++) {

            document.getElementById(validCardList[x]).setAttribute("ondblclick", "dblClickMyCard(this)");
        }
    }

    else {
        toggleUserCardDblClick("on");
    }
}

function toggleUserCardDblClick(onOff)
{
    var elements = document.getElementsByClassName("img1 userCard")

    var clickFunctionString = onOff == "on" ? "dblClickMyCard(this)" : "";

    for (x = 0; x < elements.length; x++)
    {
        elements[x].setAttribute("ondblclick", clickFunctionString);
    }
}

function updateUserScores(playerId, newscore, target)
{
    document.getElementById("p" + playerId + "TargetAndScore").innerHTML = "Target: " + target + "<br /> Tricks Won: " + newscore; 
    document.getElementById("Rnd" + thisRoundId + "p" + playerId + "Tgt").innerHTML = target;
//    document.getElementById("Rnd" + thisRoundId + "p" + playerId + "Pts").innerHTML = whateverTheScoreIs;
}


function displayReceivedCard(jsonResponse, playerId, delayIncrement)
{
    if (jsonResponse.thisTrickCards[playerId - 1] != "NONE") {
        setTimeout(function () {
            setImageSource("p" + playerId + "PlaysCard", jsonResponse.thisTrickCards[playerId - 1] );
            setImageSource("goArrow", "p" + playerId + "Arrow.png");
        }, delayIncrement * 500);
    }
}

function displayNextTrickCard(jsonResponse, playerId, delayIncrement) {
    if (jsonResponse.nextTrickCards[playerId - 1] != "NONE") {
        setTimeout(function () {
            setImageSource("p" + playerId + "PlaysCard", jsonResponse.nextTrickCards[playerId - 1]);
            setImageSource("goArrow", "p" + playerId + "Arrow.png");
        }, delayIncrement * 500);
    }
}

function setImageSource(elementId, card)
{
    var cardPath = (right(card, 4) != ".png") ? "pics/cards/" + card + ".jpg" : "pics/cards/" + card;
//    console.log(  `setImageSource - elementId = ${elementId},card = ${card}, cardPath = ${cardPath})`)
    document.getElementById(elementId).src = cardPath; 
    document.getElementById(elementId).alt = card; 
}

function right(str, chr) {
    return str.slice(str.length - chr, str.length);
}

function resetAllPlayCards()
{
    resetPlayCard("p1PlaysCard");
    resetPlayCard("p2PlaysCard");
    resetPlayCard("p3PlaysCard");
    resetPlayCard("p4PlaysCard");
}

function resetPlayCard(imgElementId) {
   // console.log("resetPlayCard, imgElementId = " + imgElementId)
    document.getElementById(imgElementId).alt = "NONE";
    document.getElementById(imgElementId).src = "pics/cards/Red_back.jpg";
}

function nextPic() {
    picClickCounter++;

    console.log("next pic fired");

    switch (picClickCounter % 3) {
        case 0:
            var thisPic = "../pics/stMalo2.jpg";
            break;
        case 1:
            var thisPic = "../pics/stMalo3.jpg";
            break;
        case 2:
            var thisPic = "../pics/stMalo1.jpg";
            break;
    }

    document.getElementById("picChooser").src = thisPic;
}
