$(document).ready(function () {
   $("#UpdatePass").on("click", function () {
        $("#showChangepassModal").click();
    });
    $("#saveNewPassword").on("click", function () {
        $(".btn-danger").click();
        var currentPassword = $("#currentPassword").val();
        var newPassword = $("#newPassword").val();
        $.ajax({
            method: "Post",
            url: "/Order/EditPassword",
            data: { CurrentPassword: currentPassword, NewPassword: newPassword },
            success: function (data) {
                if (data.status) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Done',
                        text: 'Password Changed Succeeded'
                    });
                }
                else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Invalid Current Password!'
                    });
                }
                $("#currentPassword").val("");
                $("#newPassword").val("");
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Invalid Current Password!aaaa'
                });
            }
        });
    });
});
