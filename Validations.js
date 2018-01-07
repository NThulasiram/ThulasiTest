function clearFieldValues() {
    $("#Userid").removeAttr("disabled");
    $('#Sucessinfo').css('display', 'none');
    $('#ErrorInfo').css('display', 'none');
    $('#Userid').val("");
    $('#FirstName').val("");
    $('#LastName').val("");
    $('#Email').val("");
    $('#AccountStatus').val("");
    $('#IsAdmin').prop('checked', false);
}

function DoLookup() {
    
    var userIdValue = $("#Userid").val();
    if (userIdValue === "") {
        alert('UserId Cannot be left empty');
        return false;
    }
    $.ajax(
	{
	    url: "/ManageUser/DoLookup/",
	    data: { userId: userIdValue },
	    success: function (data) {
	        $("#FirstName").val(data.firstName);
	        $("#LastName").val(data.lastName);
	        $("#Email").val(data.email);
	        $("#AccountStatus").val(data.isActive.toString());
	    }

	});
}

function clearUserIdGrid() {
    document.getElementById('searchUserId').value = "";
}
function ValidateUserId() {
    $('#Sucessinfo').css('display', 'none');
    $('#ErrorInfo').css('display', 'none');
    var userIdValue = $("#Userid").val();

    if (userIdValue === "") {

        alert('UserId Cannot be left empty');
        return false;
    }

    if (userIdValue.length > 15) {
        alert("UserId cannot Exceed 15 characters!");
        return false;
    }
}

function RemoveUser() {
    $('#Sucessinfo').css('display', 'none');
    $('#ErrorInfo').css('display', 'none');
    if (!confirm('Are you sure you want to remove this user from the system.'))
        return false;
}

function CheckStatus() {
    $("#Userid").removeAttr("disabled");
    $('#Sucessinfo').css('display', 'none');
    $('#ErrorInfo').css('display', 'none');
    var userIdVal = $("#Userid").val();
    var FirstName = $('#FirstName').val();
   // var accountStatus = document.getElementById('AccountStatus').value;
    var accountStatus = $('#AccountStatus option:selected').val();
    if (accountStatus === "Active") {
        if (!confirm('Are you sure you want to add this user to the system.')) {
            clearFieldValues();
            return false;
        }
    }
    if (userIdVal === "") {

        alert('User Id cannot be empty');
        return false;
    }
    if (accountStatus === "Inactive") {

        alert('Inactive user info cannot be added.');
        return false;
    }

    if (FirstName === "" || FirstName === " ") {
        alert('Please click lookup button to get the user details');
        return false;
    }
}
$(document).ready(function () {
    var firstName = $('#FirstName').val();
    var lastName = $('#LastName').val();
    if (firstName !== "" || firstName !== " " && lastName !== "") {
        $("#Userid").attr("disabled", "disabled");
    }
    else {
        $("#Userid").removeAttr("disabled");
    }
});