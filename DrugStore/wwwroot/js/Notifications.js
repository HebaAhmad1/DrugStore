var pharmacySignedIn = $("#PharmacySignedIn").val();
var adminSignedIn = $("#AdminSignedIn").val();
var pharmacyNotificationsCount = 0;
var adminNotificationsCount = 0;

//Check If Pharmacy IsSignedIn Or Not To Display Notifications
if (pharmacySignedIn != undefined) {
    $.ajax({
        url: "/Order/PharmacyNotification",
        type: 'GET',
        success: function (notifications) {
            //If Found Notifications
            if (notifications.status) {
                notifications.data.forEach(function (value) {
                    var listItem = `<div class="dropdown-item">
                                                 <div class="row">
                                                   <div class="col-md-10">
                                                    <a href="/Order/PharmacyArchivedOrders?pharmacyOrdersId=${value.pharmacyOrdersId}">
                                                         Your Order That Created In ${value.orderCreateAt}
                                                          Was Processed By The Admin<br/>
                                                       <small>${value.timeOrderCreateAt}</small>
                                                    </a>
                                                     </div>
                                                   </div>
                                                </div>`;
                    //Add Notifications To List
                    $("#PharmacyNotificationMenue").append(listItem);
                });
                pharmacyNotificationsCount = notifications.data.length;
                //Add Count Notifications If Found It
                if (pharmacyNotificationsCount != 0)
                    $("#pharmacyNotificationCount").text(pharmacyNotificationsCount);
            }
            //If Not Found Notifications
            else {
                $("#PharmacyNotificationMenue").append("<li>Not Found Notifications</li>");
            }
        },
        error: function () {
        }
    });
}

//Check If Admin IsSignedIn Or Not To Display Notifications
if (adminSignedIn != undefined) {
    $.ajax({
        url: "/DisplayOrders/AdminNotifications",
        type: 'GET',
        success: function (notifications) {
            //If Found Notifications
            if (notifications.status) {
                notifications.data.forEach(function (value) {
                    var listItem = `<div class="dropdown-item">
                                                 <div class="row">
                                                   <div class="col-md-10">
                                                    <a href="/DisplayOrders/AdminCurrentOrders?pharmacyOrdersId=${value.pharmacyOrdersId}">
                                                        <b>${value.pharmacyName}</b><br/>
                                                         A New Order Was Created By ${value.pharmacyName}
                                                         In ${value.orderCreateAt} And Is Waiting Your Process<br/>
                                                       <small>${value.timeOrderCreateAt}</small>
                                                    </a>
                                                     </div>
                                                   </div>
                                                </div>`;
                    $("#AdminNotificationMenue").append(listItem);
                });
                adminNotificationsCount = notifications.data.length;
                //Add Count Notifications If Found It
                if (adminNotificationsCount != 0)
                    $("#adminNotificationCount").text(adminNotificationsCount);
            }
            //If Not Found Notifications
            else {
                $("#AdminNotificationMenue").append("<li>Not Found Notifications</li>");
            }
        },
        error: function () {
        }
    });
}

//======== Realtime Notifications Using SignalR ======== :
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


//Pharmacy Notifications
connection.on("PharmacyReceiveNotifications", function (notifications) {
    var listItem = `<div class="dropdown-item">
        <div class="row">
            <div class="col-md-10">
                <a href="/Order/PharmacyArchivedOrders?pharmacyOrdersId=${notifications.pharmacyOrdersId}">
                    Your Order That Created In ${notifications.orderCreateAt}
                                                          Was Processed By The Admin<br />
                    <small>${notifications.timeOrderCreateAt}</small>
                </a>
            </div>
        </div>
    </div>`;

    //Add Notifications To List
    $("#PharmacyNotificationMenue").prepend(listItem);
    pharmacyNotificationsCount += 1;
    //Add Count Notifications
    $("#pharmacyNotificationCount").text(pharmacyNotificationsCount);
});

//Admin Notifications
connection.on("AdminReceiveNotifications", function (notifications) {
    var listItem = `<div class="dropdown-item">
        <div class="row">
            <div class="col-md-10">
                <a href="/DisplayOrders/AdminCurrentOrders?pharmacyOrdersId=${notifications.pharmacyOrdersId}">
                    <b>${notifications.pharmacyName}</b><br />
                                                         A New Order Was Created By ${notifications.pharmacyName}
                                                         In ${notifications.orderCreateAt} And Is Waiting Your Process<br />
                    <small>${notifications.timeOrderCreateAt}</small>
                </a>
            </div>
        </div>
    </div>`;
    $("#AdminNotificationMenue").prepend(listItem);
    adminNotificationsCount += 1;
    //Add Count Notifications
    $("#adminNotificationCount").text(adminNotificationsCount);
});

connection.start();

//Change Pharmacy Notification Status To Become IsReaded
$("#MainPharmacyNotifications").on("click", function () {
    $.ajax({
        url: "/Order/ChangePharmacyNotificationStatus",
        type: 'GET',
        success: function (notifications) {
            $("#pharmacyNotificationCount").text("");
        },
        error: function () {
        }
    });
});

//Change Admin Notification Status To Become IsReaded
$("#MainAdminNotifications").on("click", function () {
    $.ajax({
        url: "/DisplayOrders/ChangeAdminNotificationStatus",
        type: 'GET',
        success: function (notifications) {
            $("#adminNotificationCount").text("");
        },
        error: function () {
        }
    });
});

//Check If Not Found Notification Will Hide Notification Dropdown Menu
$(".Notification ").on("click", function () {
    if (adminNotificationsCount == 0) {
        $("#AdminNotificationMenue").addClass("d-none");
    } else {
        $("#AdminNotificationMenue").removeClass("d-none");
    }

    if (pharmacyNotificationsCount == 0) {
        $("#PharmacyNotificationMenue").addClass("d-none");
    } else {
        $("#PharmacyNotificationMenue").removeClass("d-none");
    }
});
