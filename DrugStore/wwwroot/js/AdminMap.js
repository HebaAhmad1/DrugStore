var MainMap;
var markersPh1 = [];
var markersPh2 = [];
var markersinfoPh1 = [];
var markersinfoPh2 = [];
var currentAnimatedMarkers = [];
var hiddenMarkers = [];
var hiddenMarkers2 = [];
var infowindow;
var lastPharmcyNote;
var clusterMap;
var clusterMap1;
var AdminMap = function () {
    function status0Marker() {
        let Markers = markersinfoPh1.filter(x => x.status == 0);
        for (let q = 0; q < Markers.length; q++) {
            let index = markersinfoPh1.findIndex(x => {
                return x.pharmacyId == Markers[q].pharmacyId;
            });
            markersPh1[index].setVisible(false);
            hiddenMarkers.push(markersPh1[index]);
        }
    }
    function PrepareEvents() {
        $("#PharmcySearchMap").change(() => {
            if (currentAnimatedMarkers.length > 0) {
                for (var j = 0; j < currentAnimatedMarkers.length; j++) {
                    currentAnimatedMarkers[j].setAnimation(null);
                }
            }
            let index = markersinfoPh1.findIndex(x => {
                return x.pharmacyId == $("#PharmcySearchMap").val();
            });
            if (index != -1 && markersPh1[index].visible == true) {
                MainMap.setCenter(new google.maps.LatLng(markersinfoPh1[index].latitude, markersinfoPh1[index].longitude));
                MainMap.setZoom(18)
                markersPh1[index].setAnimation(google.maps.Animation.BOUNCE);
                currentAnimatedMarkers.push(markersPh1[index]);
                setTimeout(() => {
                    markersPh1[index].setAnimation(null);
                    currentAnimatedMarkers.pop();
                }, 15000);
            }
        });
        $('#AllMarkers').change(() => {
            debugger;
            let specialCompanies = markersinfoPh2.filter(x => x.cid != null);
            if ($("#AllMarkers").is(':checked')) {
                $("#CompletedMarkers").prop('checked', true);
                $("#PendingMarkers").prop('checked', true);
                $("#HoldMarkers").prop('checked', true);
                /*************************/
                $("#SpecialCompanies").prop('checked', true);
                /*************************/
                if (hiddenMarkers.length > 0) {
                    for (let t = 0; t < hiddenMarkers.length; t++) {
                        if (hiddenMarkers[t] != undefined)
                            hiddenMarkers[t].setVisible(true);
                    }
                }
                if (specialCompanies.length > 0) {
                    for (let q = 0; q < specialCompanies.length; q++) {
                        let index = markersinfoPh2.findIndex(x => {
                            return x.id == specialCompanies[q].id;
                        });
                        markersPh2[index].setVisible(true);
                    }
                    clusterMap1.addMarkers(markersPh2);
                }
            } else {
                $("#CompletedMarkers").prop('checked', false);
                $("#PendingMarkers").prop('checked', false);
                $("#HoldMarkers").prop('checked', false);
                /*************************/
                $("#SpecialCompanies").prop('checked', false);
                /*************************/
                for (let t = 0; t < markersPh1.length; t++) {
                    markersPh1[t].setVisible(false);
                    hiddenMarkers.push(markersPh1[t]);
                }
                for (let q = 0; q < specialCompanies.length; q++) {
                    let index = markersinfoPh2.findIndex(x => {
                        return x.id == specialCompanies[q].id;
                    });
                    markersPh2[index].setVisible(false);
                    hiddenMarkers.push(markersPh1[index]);
                }
                clusterMap1.clearMarkers();
            }
        });
        $('#CompletedMarkers').change(() => {
            let completeMarkers = markersinfoPh1.filter(x => x.status == 2);
            if ($("#CompletedMarkers").is(':checked')) {
                if (completeMarkers.length > 0) {
                    for (let q = 0; q < completeMarkers.length; q++) {
                        let index = markersinfoPh1.findIndex(x => {
                            return x.pharmacyId == completeMarkers[q].pharmacyId;
                        });
                        markersPh1[index].setVisible(true);
                    }
                }
            } else {
                $("#AllMarkers").prop('checked', false);
                status0Marker();
                for (let q = 0; q < completeMarkers.length; q++) {
                    let index = markersinfoPh1.findIndex(x => {
                        return x.pharmacyId == completeMarkers[q].pharmacyId;
                    });
                    markersPh1[index].setVisible(false);
                    hiddenMarkers.push(markersPh1[index]);
                }
            }
        });
        $('#PendingMarkers').change(() => {
            let pendingMarkers = markersinfoPh1.filter(x => x.status == 1);
            if ($("#PendingMarkers").is(':checked')) {
                console.log(markersinfoPh1);
                if (pendingMarkers.length > 0) {
                    for (let q = 0; q < pendingMarkers.length; q++) {
                        let index = markersinfoPh1.findIndex(x => {
                            return x.pharmacyId == pendingMarkers[q].pharmacyId;
                        });
                        markersPh1[index].setVisible(true);
                    }
                }
            } else {
                $("#AllMarkers").prop('checked', false);
                status0Marker();
                for (let q = 0; q < pendingMarkers.length; q++) {
                    let index = markersinfoPh1.findIndex(x => {
                        return x.pharmacyId == pendingMarkers[q].pharmacyId;
                    });
                    markersPh1[index].setVisible(false);
                    hiddenMarkers.push(markersPh1[index]);
                }
            }
        });

        ///****************
        $('#SpecialCompanies').change(() => {
            let specialCompanies = markersinfoPh2.filter(x => x.cid != null);
            console.log(specialCompanies);
            if ($("#SpecialCompanies").is(':checked')) {
                if (specialCompanies.length > 0) {
                    for (let q = 0; q < specialCompanies.length; q++) {
                        let index = markersinfoPh2.findIndex(x => {
                            return x.id == specialCompanies[q].id;
                        });
                        markersPh2[index].setVisible(true);
                    }
                    clusterMap1.addMarkers(markersPh2);
                    clusterMap.clearMarkers();
                }
            } else {
                $("#AllMarkers").prop('checked', false);
                status0Marker();
                for (let q = 0; q < specialCompanies.length; q++) {
                    let index = markersinfoPh2.findIndex(x => {
                        return x.id == specialCompanies[q].id;
                    });
                    markersPh2[index].setVisible(false);
                    hiddenMarkers.push(markersPh1[index]);
                }
                clusterMap1.clearMarkers();
            }
        });
            ///****************

        $('#HoldMarkers').change(() => {
            let holdMarkers = markersinfoPh1.filter(x => x.status == 3);
            if ($("#HoldMarkers").is(':checked')) {
                console.log(markersinfoPh1);
                if (holdMarkers.length > 0) {
                    for (let q = 0; q < holdMarkers.length; q++) {
                        let index = markersinfoPh1.findIndex(x => {
                            return x.pharmacyId == holdMarkers[q].pharmacyId;
                        });
                        markersPh1[index].setVisible(true);
                    }
                }
            } else {
                $("#AllMarkers").prop('checked', false);
                status0Marker();
                for (let q = 0; q < holdMarkers.length; q++) {
                    let index = markersinfoPh1.findIndex(x => {
                        return x.pharmacyId == holdMarkers[q].pharmacyId;
                    });
                    markersPh1[index].setVisible(false);
                    hiddenMarkers.push(markersPh1[index]);
                }
            }
        });
    }
    return {
        initOnce: () => {
            PrepareEvents();
        },
        main: function () {
            let HomeMap = new google.maps.Map(document.getElementById('AdminMap'), {
                center: { lat: 31.437480, lng: 34.376295 },
                zIndex: 10,
                zoom: 12,
                minZoom: 7,
                disableDefaultUI: true,
                zoomControl: true,
                mapTypeControl: true,
                scaleControl: false,
                streetViewControl: false,
                rotateControl: true,
                fullscreenControl: false,
                mapTypeControlOptions: {
                    style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
                    position: google.maps.ControlPosition.TOP_RIGHT,
                },
            });
            MainMap = HomeMap;
            var ChangeStyle = document.getElementById('ChangeStyle');
            MainMap.controls[google.maps.ControlPosition.TOP_LEFT].push(ChangeStyle);
        },
        PharmacyMarker: function () {
            $.get("/Map/AdminMap", function (rows) {
                console.log(rows);
                var notes = "";
                for (var i = 0; i < rows.data.length; i++) {
                    if (rows.data[i].latitude != null && rows.data[i].longitude != null) {
                        let phmarker = new google.maps.Marker({
                            position: new google.maps.LatLng(rows.data[i].latitude, rows.data[i].longitude),
                            label: {
                                text: rows.data[i].pharmacyName,
                                color: "white",
                                fontSize: "15px"
                            },
                            icon: {
                                url: rows.data[i].status == 0 ? '/images/markers/blue.png' : (rows.data[i].status == 1 ? '/images/markers/yellow.png' : rows.data[i].status == 2? '/images/markers/green.png': null),
                                scaledSize: new google.maps.Size(30, 40), // size
                                labelOrigin: { x: 18, y: -10 }
                            }
                        });
                        phmarker.addListener("click", (e) => {
                            let index = markersPh1.findIndex(x => {
                                return x == phmarker;
                            });
                            notes = "";
                            lastPharmcyNote = index;
                            if (markersinfoPh1[index] != undefined && markersinfoPh1[index].pharmacyNotes != null && markersinfoPh1[index].pharmacyNotes.length > 0) {
                                for (var j = 0; j < markersinfoPh1[index].pharmacyNotes.length; j++) {
                                    notes += '<br><div class="row"><b style="font-size: 16px; color: #4E4E4E;" class="col-4 pt-2">' + markersinfoPh1[index].pharmacyNotes[j].noteTitle + '</b> <input type="text" class="form-control col-8" disabled value="' + markersinfoPh1[index].pharmacyNotes[j].noteBody + '"></div>';
                                }
                            }
                            var content = '<div class="row"><b style="font-size: 16px; color: #4E4E4E;" class="col-4 pt-2">Pharmacy Name </b> <input type="text" class="form-control col-8" disabled value="' + markersinfoPh1[index].pharmacyName + '"></div><br>' +
                                '<div class="row"><b style="font-size: 16px; color: #4E4E4E;" class="col-4 pt-2">Supplier Name </b> <input type="text" class="form-control col-8" disabled value="' + markersinfoPh1[index].supplierName + '"></div>' + (notes == "" ? "" : notes);
                            AdminMap().openInfoWindow(markersPh1[index], content, markersinfoPh1[index].pharmacyId);
                        });
                        markersPh1.push(phmarker);
                        markersinfoPh1.push(rows.data[i]);
                    }
                }
                let clusterMarker = new MarkerClusterer(MainMap, markersPh1, {
                    styles: [{
                        url: '/images/markers/m1.png',
                        width: 53,
                        height: 53,
                        textColor: "white"
                    }],
                    maxZoom: 16,
                    ignoreHidden: true
                });
                clusterMap = clusterMarker;
                clusterMap1.clearMarkers();

            });
            /*****************/
            $.get('/Map/SpecialCompanies', (rows) => {
                console.log(rows);
               // debugger;
                for (var i = 0; i < rows.length; i++) {
                    if (rows[i].latitude != null && rows[i].longitude != null) {
                        let phmarker = new google.maps.Marker({
                            position: new google.maps.LatLng(rows[i].latitude, rows[i].longitude),
                            label: {
                                text: rows[i].locationName,
                                color: "white",
                                fontSize: "15px"
                            },
                            icon: {
                                url: '/Images/markers/b2.png',
                                scaledSize: new google.maps.Size(55, 55), // size
                                labelOrigin: { x: 18, y: -10 }
                            }
                        });
                        phmarker.addListener("click", (e) => {
                            let index = markersPh2.findIndex(x => {
                                return x == phmarker;
                            });
                            var content = '<div class="row"><b style="font-size: 16px; color: #4E4E4E;" class="col-4 pt-2">Company Name </b> <input type="text" class="form-control col-8" disabled value="' + markersinfoPh2[index].username + '"></div><br>' +
                                '<div class="row"><b style="font-size: 16px; color: #4E4E4E;" class="col-4 pt-2">Location Name </b> <input type="text" class="form-control col-8" disabled value="' + markersinfoPh2[index].locationName + '"></div>';
                            var data = `<div style=" padding: 20px; overflow-y: hidden;"><h5 style="display: flex; justify-content: center; font-size: 24px;">Company Details</h5><br>` + content;
                            infowindow.setContent(data);
                            infowindow.open({
                                anchor: markersPh2[index],
                                map: MainMap,
                                shouldFocus: false,
                            });
                        });
                        markersPh2.push(phmarker);
                        markersinfoPh2.push(rows[i]);

                    }
                }

                let clusterMarker2 = new MarkerClusterer(MainMap, markersPh2, {
                    styles: [{
                        url: '/Images/markers/m2.png',
                        width: 53,
                        height: 53,
                        textColor: "white"
                    }],
                    maxZoom: 16,
                    ignoreHidden: true
                });
                clusterMap1 = clusterMarker2;
                clusterMap1.clearMarkers();
            });



            /*****************/
        },
        InfoWindow: function () {
            infowindow = new google.maps.InfoWindow({});
        },
        closeInfowindow: function () {
            infowindow.close();
        },
        openInfoWindow: function (marker, content, id) {
            var data = `<div style=" padding: 20px; overflow-y: hidden;"><h5 style="display: flex; justify-content: center; font-size: 24px;">Pharmacy Details</h5><br>` + content;
            data += `<br><div><h5 style="padding-top: 22px; font-size: 16px; color: #4E4E4E;">Notes</h5><input type="text" class="form-control" id="topicNote" placeholder="Title"/><textarea placeholder="Enter pharmacy notes here" id="NotesBody" class="form-control" style="margin-top: 13px !important;" rows="4" cols="50" ></textarea><br><div class="row justify-content-center"><button id="AddNewTopic" class="btn btn-success" data-marker="${id}">Save</button></div></div>`;
            infowindow.setContent(data);
            infowindow.open({
                anchor: marker,
                map: MainMap,
                shouldFocus: false,
            });
        },
        saveNotes: function () {
            $('body').off('click', '#AddNewTopic');
            $('body').on('click', '#AddNewTopic', function (e) {
                e.preventDefault();
                var data = {};
                data.PharmacyId = $(this).attr("data-marker");
                data.NoteTitle = $("#topicNote").val();
                data.NoteBody = $("#NotesBody").val();
                if (data.PharmacyId != null && data.NoteTitle != "" && data.NoteBody != "") {
                    $.post("/Map/AddMapNotes", { note: data }, function (done) {
                        if (done.status) {
                            if (lastPharmcyNote != null && lastPharmcyNote != undefined) {
                                markersinfoPh1[lastPharmcyNote].pharmacyNotes.push({ noteBody: data.NoteBody, noteTitle: data.NoteTitle });
                            }
                            AdminMap().closeInfowindow();
                        }
                    });
                } else {

                }
            });
        },
        loadPharmceySelect: () => {
            $.get('/DisplayOrders/GetAllPharmacy', (done) => {
                console.log(done);
                for (var i = 0; i < done.results.length; i++) {
                    $('#PharmcySearchMap').append($('<option>', {
                        value: done.results[i].id,
                        text: done.results[i].text,
                    }));
                }
            });
        }
    }
}


$(document).ready(function () {
    AdminMap().initOnce();
    AdminMap().main();
    AdminMap().InfoWindow();
    AdminMap().PharmacyMarker();
    AdminMap().saveNotes();
    AdminMap().loadPharmceySelect();
});