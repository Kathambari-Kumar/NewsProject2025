// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ApproveArticle(articleId) {
    var status = "Status" + articleId;
    var comment = "Comment" + articleId;
    var message = document.getElementById(comment).value;
    $.ajax({
        type: 'post',
        url: '/Admin/ApproveArticle',
        dataType: 'json',
        data: { articleId: articleId, message: message },
        success: function (returnmessage) {
            if (returnmessage == "Approved") {
                swal({
                    title: "Digital Dragons",
                    text: "Article is approved!",
                    type: "success"
                }).then(function () {
                    window.location.href = '/Admin/GetUnApprovedArticles';
                    document.getElementById(status).innerText = "Approved";
                });
            }
            else {
                swal({
                    title: "Digital Dragons",
                    text: "Oops, Something went wrong! Try later!!",
                    type: "danger"
                });
            }
        }
    });
}

function ClickForSpeech() {
    $.ajax({
        type: "POST",
        url: "/Home/GetSpeechInput",
        success: function (recognizedText) {
            console.log(recognizedText);
            document.getElementById("searchword").value = recognizedText;
        }
    });
}
function DeleteArticle(articleId) {
    var status = "Status" + articleId;
    var comment = "Comment" + articleId;
    var message = document.getElementById(comment).value;
    $.ajax({
        type: 'post',
        url: '/Admin/DeleteArticle',
        dataType: 'json',
        data: { articleId: articleId, message: message },
        success: function (returnmessage) {
            if (returnmessage == "Deleted") {
                swal({
                    title: "Digital Dragons",
                    text: "Article is deleted!",
                    type: "success"
                }).then(function () {
                    // window.location.href = '/Admin/GetUnApprovedArticles';
                    document.getElementById(status).innerText = "Deleted";
                });
            }
            else {
                swal({
                    title: "Digital Dragons",
                    text: "Oops, Something went wrong!",
                    type: "danger"
                });
            }
        }
    });
}

function RejectArticle(articleId) {
    var status = "Status" + articleId;
    var comment = "Comment" + articleId;
    var message = document.getElementById(comment).value;

    $.ajax({
        type: 'post',
        url: '/Admin/RejectArticle',
        dataType: 'json',
        data: { articleId: articleId, message: message },
        success: function (returnmessage) {
            if (returnmessage == "Rejected") {
                swal({
                    title: "Digital Dragons",
                    text: "Article is Rejected!",
                    type: "success"
                }).then(function () {
                    //window.location.href = '/Admin/GetUnApprovedArticles';
                    document.getElementById(status).innerText = "Rejected";
                });
            }
            else {
                swal({
                    title: "Digital Dragons",
                    text: "Oops, Something went wrong!",
                    type: "danger"
                });
            }
        }
    });
}

function CountLikes(articleId) {
    $.ajax({
        type: 'post',
        url: '/NewsArticle/CountLikes',
        dataType: 'json',
        data: { articleId: articleId },
        success: function (likecount) {
            console.log(likecount);
            document.getElementById("likecount").innerText = likecount;
        }
    });
}
function ContinentVisible() {
    var getelement = document.getElementById("menubar3");
    getelement.style.visibility = "visible";
    $.ajax({
        type: 'post',
        url: '/NewsArticle/CategoryBasedArticles',
        dataType: 'json',
        data: { categoryname: "World" },
        success: function () {
            console.log("Done");
        }
    });
}

function ViewChart() {
    var labels = document.getElementById("monthNameList").innerHTML;
    var datas = document.getElementById("userCountList").innerHTML;
    console.log(datas);
    var dataArray = new Array();
    var labelArray = new Array();
    for (var i = 0; i <= datas.split(',').length; i++) {
        dataArray.push(datas.split(',')[i]);
        labelArray.push(labels.split(',')[i]);
    }
    var barColors = ["red", "green", "blue", "orange", "brown"];

    var lineChart = new Chart("myChart", {
        type: 'bar',
        data: {
            labels: labelArray,
            datasets: [{
                label: 'Readers Subscription Statistics',
                backgroundColor: barColors,
                data: dataArray
            }]
        },
        options: {
            tooltips: { mode: 'index', intersect: false },
            responsive: true,
            scales: {
                xAxes: [{ stacked: true }],
                yAxes: [{ stacked: true }]
            }
        }
    });
}

function AddToCart(productId) {

    $.ajax({

        type: 'post',
        url: '/Product/AddToCart',
        dataType: 'json',
        data: { id: productId },
        success: function (count) {
            console.log(count);
            // The id ‘cartCount’ refers to an HTML-element
            $('#CartCount').html(count);
        }
    })
}

function CheckUserStatus() {
    $.ajax({
        url: '/Product/CheckUserStatus',
        type: 'POST',
        success: function (message) {
            if (message == "Failed") {
                document.getElementById("CartAlert").style.visibility = 'visible';
            }
            else if (message == "SignedIn") {
                window.location.assign('/Product/DisplayCart');
            }
        }
    });
}
function IncreaseCopy(productId) {
    var copyid = 'changecopies' + productId;
    var subtotalid = "changesubtotal" + productId;
    var priceid = "changeprice" + productId;
    var cartcount = Number(document.getElementById("CartCount").innerHTML);
    var totalid = Number(document.getElementById("totalid").innerHTML);
    var oldcopies = Number(document.getElementById(copyid).innerHTML);
    var price = Number(document.getElementById(priceid).innerHTML);
    var newcopies = oldcopies + 1;
    document.getElementById(copyid).innerText = newcopies;
    document.getElementById(subtotalid).innerText = newcopies * price;
    document.getElementById("totalid").innerText = totalid + price;
    document.getElementById("CartCount").innerText = cartcount + 1;
    $.ajax({
        url: '/Product/AddItem',
        data: { productId: productId },
        type: 'POST',
        success: function () {
            console.log("added");
        }
    });
}

function DecreaseCopy(productId) {
    var copyid = 'changecopies' + productId;
    var subtotalid = "changesubtotal" + productId;
    var priceid = "changeprice" + productId;
    var cartcount = Number(document.getElementById('CartCount').innerHTML);
    var totalid = Number(document.getElementById('totalid').innerHTML);
    var rowid = "row" + productId;
    var oldcopies = Number(document.getElementById(copyid).innerHTML);
    var price = Number(document.getElementById(priceid).innerHTML);
    var newcopies = oldcopies - 1;
    document.getElementById('CartCount').innerText = cartcount - 1;
    if (newcopies == 0) {
        document.getElementById(rowid).remove();
        document.getElementById('totalid').innerText = totalid - price;
    }
    else {
        document.getElementById(copyid).innerText = newcopies;
        document.getElementById(subtotalid).innerText = newcopies * price;
        document.getElementById('totalid').innerText = totalid - price;
    }
    $.ajax({
        url: '/Product/ReduceItem',
        data: { productId: productId },
        type: 'POST',
        success: function () {
            console.log("decraesed");
        }
    });
}

function Checkout() {
    $.ajax({
        url: '/Product/PlacingOrder',
        type: 'POST',
        success: function (message) {
            if (message == "success") {
                swal({
                    title: "Digital Dragons",
                    text: "Your Order has been Placed!",
                    type: "success"
                }).then(function () {
                    window.location.href = '/Home/Index';
                });
            }
            else {
                swal({
                    title: "Digital Dragons",
                    text: "Oops, Something went wrong! Try later!!",
                    type: "danger"
                }).then(function () {
                    window.location.href = '/Home/Index';
                });
            }
        }
    });
}

function ClearPreviousEditorsChoice() {
    $.ajax({
        type: 'post',
        url: '/Admin/SetEditorChoiceToFalse',
        success: function (message) {
            document.getElementsByClassName('selectBox').checked = false;
            window.location.href = '/Admin/EditorsChoiceList';
        }
    });
}
function SelectArticle(articleId) {
    var IsSelected = "Selected" + articleId;
    var articleCheckbox = "Checkbox" + articleId;
    $.ajax({
        type: 'post',
        url: '/Admin/SelectEditorChoiceArticle',
        dataType: 'json',
        data: { articleId: articleId },
        success: function (returnmessage) {
            console.log(returnmessage);
            if (returnmessage == "success") {
                document.getElementById(IsSelected).innerText = "Selected";
            }
            else if (returnmessage == "deselected") {
                document.getElementById(articleCheckbox).checked = false;
                document.getElementById(IsSelected).innerText = "UnSelected";
            }
            else {
                document.getElementById(articleCheckbox).checked = false;
                swal({
                    title: "Digital Dragons",
                    text: "Maximum three articles can be selected",
                    type: "danger"
                });
            }
        }
    });
}
function CheckAccessLevel(articleId) {
    $.ajax({
        url: '/Home/CheckAccessLevel',
        type: 'POST',
        data: { Id: articleId },
        success: function (message) {
            if (message == "signedIn") {
                window.location.assign('/NewsArticle/DetailNewsDisplay?id=' + articleId);
            }
            else if (message == "notSignedIn" || message == "noActiveSubscription") {
                document.getElementById("signInBanner").style.visibility = 'visible';
            }
            else if (message == "hasActiveSubscription") {
                window.location.href = '/NewsArticle/DetailNewsDisplay?id=' + articleId;
            }
        }
    });
}

function PlayVoiceText() {
    //var audioId = "audioPlayer" + articleId;
    //console.log(audioId);
    var audioButton = document.getElementById('audioPlayer');
    if (audioButton.paused) {
        document.getElementById('audioPlayer').play();
        $('#audioControl').attr("src", "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/stopbutton.jpg");
    }
    else {
        document.getElementById("audioPlayer").pause();
        $('#audioControl').attr("src","https://dragonsstorage24.blob.core.windows.net/dragoncontainer/playicon.jpg");
    }
}

function Translate(articleId) {
    var selectedLang = $("#langSelect").val();
    $.ajax({
        url: '/NewsArticle/Translate',
        type: 'POST',
        data: { id: articleId, lang: selectedLang },
        success: function (response) {
            if (response.success) {
                //alert(response.translatedContent);
                 document.getElementById('linkText').innerText = response.translatedLinkText;
                 document.getElementById('contentSummary').innerText = response.translatedContentSummary;
                 document.getElementById('content').innerText = response.translatedContent;
                 document.getElementById("translatorModal").style.visibility = 'visible';
            }
            else if(respons.message = "error") {
                document.getElementById('errorMessage').innerText = "Translation failed. Please try again.";
                document.getElementById("translatorModal").style.visibility = 'visible';
            }
        }
    });
}

function SendRequest() {
    var question = document.getElementById("chatRequest").value;
    console.log(question);
    $.ajax({
        url: '/Home/ChatBox',
        type: 'Post',
        data: { request: question },
        success: function (response) {
            console.log(response);
            var chatWindow = document.getElementById('chatWindow');
            const chatLi = document.createElement('p');
            chatLi.className = 'chat';
            chatLi.innerText = response;
            chatWindow.appendChild(chatLi);
        }
    });
}
function SendInteractiveRequest() {
    var question = document.getElementById("chatRequest").value;
    console.log(question);
    $.ajax({
        url: '/Home/InteractiveChat',
        type: 'Post',
        data: { request: question },
        success: function (response) {
            console.log(response);
            document.getElementById("chatRequest").value = "";
            var chatWindow = document.getElementById('chatWindow');
            const chatLi = document.createElement('p');
            chatLi.className = 'chat';
            chatLi.innerText = response;
            chatWindow.appendChild(chatLi);
        }
    });
}

function ChatWithOwnData() {
    var question = document.getElementById("chatRequest").value;
    console.log(question);
    $.ajax({
        url: '/Home/ChatWithOwnData',
        type: 'Post',
        data: { request: question },
        success: function (response) {
            console.log(response);
            var chatWindow = document.getElementById('chatWindow');
            const chatLi = document.createElement('p');
            chatLi.className = 'chat';
            chatLi.innerText = response;
            chatWindow.appendChild(chatLi);
        }
    });
}

