$(document).ready(function () {

    //Show File Name When Uploaded
    $('#drugFileValue').on('change', function (e) {
        //get the file name
        var fileName = e.target.files[0].name
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    });

    //Upload Drugs
    $("form#drugData").submit(function (e) {
        e.preventDefault();
        //Close Modal
        $("#closeDrugMoal").click();
        //Show Loader To User
        $("#showDrugLaoder").click();
        var formData = new FormData();
        formData.append("File", $("#drugFileValue").get(0).files[0]);
        $.ajax({
            url: "/DisplayOrders/ImportDrugsInfoAsExcel",
            type: 'POST',
            data: formData,
            success: function (data) {
                $("#closeDrugLaoder").click();
                if (data.status)
                    Swal.fire('Upload Drugs Succeeded');
                else
                    Swal.fire('Upload Drugs Failed');
            },
            error: function () {
                $("#closeDrugLaoder").click();
                Swal.fire('Upload Drugs Failed');
            },
            cache: false,
            contentType: false,
            processData: false
        });
    });

    //Show File Name When Uploaded
    $('#pharmacyFileValue').on('change', function (e) {
        //get the file name
        var fileName = e.target.files[0].name;
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    });

    //Upload Pharmacies
    $("form#pharData").submit(function (e) {
        e.preventDefault();
        //Close Modal
        $("#closePharmacyMoal").click();
        //Show Loader To User
        $("#showPharmacyLaoder").click();
        var formData = new FormData();
        formData.append("File", $("#pharmacyFileValue").get(0).files[0]);
        $.ajax({
            url: "/DisplayOrders/ImportPharmacysInfoAsExcel",
            type: 'POST',
            data: formData,
            success: function (data) {
                $("#LoaderPharmacyModal2").click();
                if (data.status)
                    Swal.fire('Upload Pharmacies Succeeded');
                else
                    Swal.fire('Upload Pharmacies Failed');
            },
            error: function () {
                $("#LoaderPharmacyModal2").click();
                Swal.fire('Upload Pharmacies Failed');
            },
            cache: false,
            contentType: false,
            processData: false
        });
    });


    //Show File Name When Uploaded
    $('#LamarDataFileValue').on('change', function (e) {
        //get the file name
        var fileName = e.target.files[0].name;
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    });

    //Upload Pharmacies
    $("form#LamarLocationsDataForm").submit(function (e) {
        e.preventDefault();
        //Close Modal
        $("#closePharmacyMoal").click();
        //Show Loader To User
        $("#showPharmacyLaoder2").click();
        var formData = new FormData();
        formData.append("File", $("#LamarDataFileValue").get(0).files[0]);
        $.ajax({
            url: "/Map/ImportSpecialLocations",
            type: 'POST',
            data: formData,
            success: function (data) {
                $("#LoaderPharmacyModal2").click();
                Swal.fire('Upload Locations Succeeded');
            },
            error: function () {
                $("#LoaderPharmacyModal2").click();
                Swal.fire('Upload Locations Failed');
            },
            cache: false,
            contentType: false,
            processData: false
        });
    });
});