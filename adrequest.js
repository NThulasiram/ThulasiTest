$(document).ready(function () {
    selectAddRadio();
    showhide();
    showDatePicker();
    bindUserAutoComplete();
    bindManagerAutocomplete();
    bindMimicFromAutocomplete();
   

    $("#btnSubmit").click(function () {
        createADRequest();
    });
    $("#btnCancel").click(function () {
        window.location.href = '/ADRequest/Index';
    });
    $("#btnSearch").click(function () {
        searchUser();
    });
});

function showDatePicker() {
    $("#txtStartDate").datepicker();
}
function selectAddRadio() {
    $("#chkYes").prop("checked", true);
    $("#newuser").show();
    $(".user-show").hide();
    $("#editUser").hide();
}

function showhide()
{
    $("input[name='newuser']").click(function () {
        if ($("#chkYes").is(":checked")) {
            $("#newuser").show();
            $(".user-show").hide();
            $("#editUser").hide();
        }
        else if ($("#chkEdit").is(":checked")) {
            $("#editUser").show();
            $("#newuser").hide();
            $(".user-show").show();
        }
        else {
            $("#newuser").hide();
            $(".user-show").show();
            $("#editUser").hide();

        }
    });
}

function bindUserAutoComplete()
{
    $.ajax({
        url: '/ADRequest/BindUserIdTextBox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.usersToBind;
            $("#userid").autocomplete({
                source: users
            });
        },
        error: function (response) {
        }
    });
}
function bindManagerAutocomplete() {
    $.ajax({
        url: '/ADRequest/BindUserIdTextBox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.usersToBind;
            $("#txtManager").autocomplete({
                source: users
            });
        },
        error: function (response) {
        }
    });
}

function bindMimicFromAutocomplete() {
    $.ajax({
        url: '/ADRequest/BindUserIdTextBox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.usersToBind;
            $("#mimic-form").autocomplete({
                source: users
            });
        },
        error: function (response) {
        }
    });
}

function createADRequest() {
    var newUser = $("#chkYes").is(":checked");
    var editUser = $("#chkEdit").is(":checked");
    var deleteUser = $("#chkNo").is(":checked");
    var loading = $("#loading");
    if (newUser == false && editUser == false && deleteUser == false) {
        alert('Please select Request type.')
        return;
    }
    if (newUser) {
        if (!validateAddRequest()) {
            return false;
        }
    }
    if (editUser) {
        if (!validateEditRequest()) {
            return false;
        }
    }
    if (deleteUser) {
        if (!validateDeleteRequest()) {
            return false;
        }
    }

    if (confirm('Are you sure you want to submit the request?')) {
        $(document).ajaxStart(function () {
            loading.show();
        });
        if (newUser) {
            newUserADRequest();
        }
        else if (editUser) {
            editUserADRequest();
        }
        else if (deleteUser) {
            deleteUserADRequest();
        }
        $(document).ajaxStop(function () {
            loading.hide();
        });
    }
}

function newUserADRequest() {
        var data = new FormData();
        var firstname = $("#first-name").val();
        var lastname = $("#last-name").val();
        var mimicfrom = $("#mimic-form").val();
        var title = $("#txtTitle").val();
        var branch = $("#ddlBranchAdd").val();
        var manager = $("#txtManager").val();
        var dept = $("#txtDepartment").val();
        var startDate = $("#txtStartDate").val();
        var comment = $("#txtComment").val();

        data.append("firstname", firstname);
        data.append("lastname", lastname);
        data.append("mimicfrom", mimicfrom);
        data.append("title", title);
        data.append("branch", branch);
        data.append("manager", manager);
        data.append("dept", dept);
        data.append("startDate", startDate);
        data.append("comment", comment);

        $.ajax({
            url: '/ADRequest/CheckADUserNameExists',
            data: data,
            processData: false,
            contentType: false,
            type: 'POST',
            success: function (response) {
                var message = response.msg;
                if (message.length > 0) {
                    alert(message);
                }
                //Ajax call to create new AD User
                $.ajax({
                    url: '/ADRequest/NewUserADRequest',
                    data: data,
                    processData: false,
                    contentType: false,
                    type: 'POST',
                    success: function (response) {
                        var message = response.msg;
                        if (message.length>0) {
                            alert(message);
                        }
                        var redirectUrl = response.returnurl;
                        if (redirectUrl.length > 0) {
                            window.location.href = redirectUrl;
                        }
                    },
                    error: function (response) {
                    }
                });
            },
            error: function (response) {
            }
        });
}

function editUserADRequest() {
    var data = new FormData();
    var requestForUser = $("#userid").val();
    var branch = $("#ddlBranchEdit").val();
    var comment = $("#txtComment").val();

    data.append("requestForUser", requestForUser);
    data.append("branch", branch);
    data.append("comment", comment);

    $.ajax({
        url: '/ADRequest/EditUserADRequest',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var message = response.msg;
            if (message.length > 0) {
                alert(message);
            }
            var redirectUrl = response.returnurl;
            if (redirectUrl.length > 0) {
                window.location.href = redirectUrl;
            }
        },
        error: function (response) {
        }
    });
}

function deleteUserADRequest() {
    var data = new FormData();
    var requestForUser = $("#userid").val();
    var comment = $("#txtComment").val();

    data.append("requestForUser", requestForUser);
    data.append("comment", comment);

    $.ajax({
        url: '/ADRequest/DeleteUserADRequest',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var message = response.msg;
            if (message.length > 0) {
                alert(message);
            }
           
            var redirectUrl = response.returnurl;
            if (redirectUrl.length > 0) {
                window.location.href = redirectUrl;
            }
        },
        error: function (response) {
        }
    });
}

function searchUser() {
    var data = new FormData();
    var requestForUser = $("#userid").val();
    data.append("requestForUser", requestForUser);
    $.ajax({
        url: '/ADRequest/SearchUser',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            if (response.msg.length > 0)
            {
                alert(response.msg);
            }
            else
            {
                var grpName = response.groupname;
                $("#ddlBranchEdit").val(grpName);
            }
        },
        error: function (response) {
        }
    });
}

function validateAddRequest() {
    var msg = 'Please enter values for mandatory fields!';
    if ($("#first-name").val() === "") {
        alert(msg);
        return false;
    }
    if ($("#last-name").val() === "") {
        alert(msg);
        return false;
    }
    if ($("#txtTitle").val() === "") {
        alert(msg);
        return false;
    } if ($("#ddlBranchAdd").val() === "") {
        alert(msg);
        return false;
    } if ($("#txtManager").val() === "") {
        alert(msg);
        return false;
    } if ($("#txtDepartment").val() === "") {
        alert(msg);
        return false;
    } if ($("#txtStartDate").val() === "") {
        alert(msg);
        return false;
    }
    return true;
}

function validateEditRequest() {
    var msg = 'Please enter values for mandatory fields!';
    if ($("#ddlBranchEdit").val() === "") {
        alert(msg);
        return false;
    }
    if ($("#userid").val() === "") {
        alert(msg);
        return false;
    }
    return true;
}

function validateDeleteRequest() {
    var msg = 'Please enter values for mandatory fields!';
    if ($("#userid").val() === "") {
        alert(msg);
        return false;
    }
    return true;
}