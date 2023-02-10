var mapChangeLocation;
var marker;
var info;
var innerMap = function () {
    return {
        main: function () {
            let mapLocation = new google.maps.Map(document.getElementById('ChangeLocationDiv'), {
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
                    zIndex: 10,
                },
            });
            mapChangeLocation = mapLocation;
        },
        actualLocation: function () {
            $.get("/Map/PharmacyCoordinates", function (done) {
                if (done.status && done.data.latitude != undefined && done.data.latitude != null) {
                    let Marker = new google.maps.Marker({
                        position: new google.maps.LatLng(done.data.latitude, done.data.longitude),
                        map: mapChangeLocation,
                        title: "Location",
                    });
                    marker = Marker;
                    google.maps.event.clearListeners(mapChangeLocation, 'click');
                    marker.addListener('click', function (e) {
                        innerMap().openInfoWindow(marker);
                        innerMap().markerEvent();
                    });
                }
            });
        },
        getMarker: function (position) {
            let Marker = new google.maps.Marker({
                position: position,
                map: mapChangeLocation,
                title: "Location",
            });
            mapChangeLocation.setCenter(position);
            marker = Marker;
            google.maps.event.clearListeners(mapChangeLocation, 'click');
            marker.addListener('click', function (e) {
                innerMap().openInfoWindow(marker);
                innerMap().markerEvent();
            });
        },
        ChangePharmacyMarker: function () {
            mapChangeLocation.addListener('click', function (e) {
                innerMap().changeInputs(e.latLng.lat(), e.latLng.lng());
                innerMap().getMarker(e.latLng);
            });
            
        },
        InfoWindow: function (content) {
            info = new google.maps.InfoWindow({
                content: content,
            });
        },
        openInfoWindow: function (marker) {
            info.open({
                anchor: marker,
                map: mapChangeLocation,
                shouldFocus: false,
            });
        },
        markerEvent: function () {
            $('#deleteMarkerLocation').off();
            $('body').on('click', '#deleteMarkerLocation', function () {
                if (marker != null) {
                    marker.setMap(null);
                    marker = null;
                    innerMap().ChangePharmacyMarker();
                    $('#LongitudeInputLocationChanger').val("");
                    $('#LatitudeInputLocationChanger').val("");
                    mapChangeLocation.setCenter({ lat: 31.437480, lng: 34.376295 });
                }
            });
        },
        changeInputs: function (lat,lng) {
            $('#LatitudeInputLocationChanger').val(lat);
            $('#LongitudeInputLocationChanger').val(lng);
        },
        inputsData: function () {
            $('#LatitudeInputLocationChanger').on('change', function () {
                if ($('#LongitudeInputLocationChanger').val() != "" && $('#LongitudeInputLocationChanger').val() != null && $('#LatitudeInputLocationChanger').val() != "") {
                    if (marker != null) {
                        marker.setMap(null);
                        marker = null;
                        innerMap().ChangePharmacyMarker();
                    }
                    innerMap().getMarker(new google.maps.LatLng($('#LatitudeInputLocationChanger').val() ,$('#LongitudeInputLocationChanger').val()));
                }
            });
            $('#LongitudeInputLocationChanger').on('change', function () {
                if ($('#LatitudeInputLocationChanger').val() != "" && $('#LatitudeInputLocationChanger').val() != null && $('#LongitudeInputLocationChanger').val() != "") {
                    if (marker != null) {
                        marker.setMap(null);
                        marker = null;
                        innerMap().ChangePharmacyMarker();
                    }
                    innerMap().getMarker(new google.maps.LatLng($('#LatitudeInputLocationChanger').val(), $('#LongitudeInputLocationChanger').val()));
                }
            });
        },
        submitLocation: function () {
            $('#changeLocationMapBtn').on('click', function () {
                if ($('#LatitudeInputLocationChanger').val() != null &&
                    $('#LongitudeInputLocationChanger').val() != null &&
                    $('#LongitudeInputLocationChanger').val() != "" &&
                    $('#LatitudeInputLocationChanger').val() != "") {

                    $.post("/Map/ChangePharmacyCoordinates", { Latitude : $('#LatitudeInputLocationChanger').val(), Longitude : $('#LongitudeInputLocationChanger').val() }, function () {
                        $(".close").click();
                    });
                } else {
                }
            });
        }
    }
}

$(document).ready(function () {
    innerMap().main();
    innerMap().actualLocation();
    innerMap().ChangePharmacyMarker();
    innerMap().inputsData();
    innerMap().submitLocation();
    innerMap().InfoWindow(`<div class="container"><h4>Change Location?! ..</h4> <br> <button id="deleteMarkerLocation" class="btn btn-danger">Change</button></div>`);
});