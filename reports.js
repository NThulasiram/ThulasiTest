$(document).ready(function () {
    $("#divManagerSelection").hide();
    $("#selectUserName").hide();
    $("#IncludeGenericUserAccounts").hide();
    $('#btnSearch').prop("disabled", true);
    bindManagerAutocomplete();
    $("#btnSearch").click(function () {
        GenerateReport();
    });
    $("#ddlReportTypes").change(function () {
        $('#ReportPlaceholder').html('');
        if ($("#ddlReportTypes option:selected").text() === "Select Report Type") {
            $('#btnSearch').prop("disabled", true);
            $("#divManagerSelection").hide();
            $("#selectUserName").hide();
            $("#IncludeGenericUserAccounts").hide();
            return;
        }
        else
        {
            $('#btnSearch').prop("disabled", false);
        }
        var selectReportType = $("#ddlReportTypes").val();
        if (selectReportType === "Active Directory - Manager" || selectReportType === "Encompass - Manager") {
            ResetManagerReportSelection(selectReportType);
        } else {
            ResetUserReportSelection();
        }
    });
    jQuery('#txtSelectManager').on('input propertychange paste', function () {
        if (!$('#txtSelectManager').val().trim()) {
            $("#Reportees :input").prop("disabled", true);
            $('input[name="Reportees"]').prop('checked', false);
        } else {
            $("#Reportees :input").prop("disabled", false);
            $('input:radio[name=Reportees]').filter('[value=Direct]').prop('checked', true);
        }
    });
});

function ShowHideOncheck() {
 
    $("#selectUserName").hide();
    $("#availableManagers").show();
      
}
function ShowHideOnUncheck() {
    $("#selectUserName").show();
    $("#availableManagers").hide();
}

function PopulateUserDropdown() {
    var data = new FormData();
    var selectReportType = $("#ddlReportTypes").val();
    data.append("selectReportType", selectReportType);
    $.ajax({
        url: '/Reports/PopulateUserDropdown',
        data: data,
        processData: false,
        contentType: false,
        dataType: "json",
        type: "POST",
        error: function () {
        },
        success: function (response) {
            $('#ddlADUser').empty();
            var optionhtml1 = '<option value="' + 0 + '">' + "Select User" + '</option>';
            $("#ddlADUser").append(optionhtml1);
            var users = JSON.parse(response.users);
            $.each(users, function (index, value) {
                    var optionhtml = '<option value="' + value.UserID + '">' + value.UserName + '</option>';
                    $("#ddlADUser").append(optionhtml);
                   
            });
            $('#btnSearch').prop("disabled", false);
        }
    });
}
function GenerateReport() {
    var selectUserId = $("#ddlADUser").val();
    var reportType = $("#ddlReportTypes").val();
    var selectedManager = $("#txtSelectManager").val();
    var reporteesType = $("input[name='Reportees']:checked").val();
    var selectedStatus = $("#ddlStatus").val();
    var includeGenericUserAccounts = $('#chkIncludeGenericUserAccounts').prop('checked');
    var requestForUsers = [];

    if (reportType === "") {
        alert('Please select Report type!');
        return;
    }
    // start specific script for old reports(encompass and ADuser report)
    if (reportType === 'Active Directory' || reportType === 'Encompass') {
     
        if (selectUserId == 0) {
            $('#ddlADUser option')
                .each(function () {
                    if ($(this).attr('value') != '') {
                        requestForUsers.push($(this).attr('value'));
                    }
                });
        } else {
            requestForUsers.push(selectUserId);
        }

        if (requestForUsers.length > 1) {
            alert('Report will be generated for all available users. This may take some time to generate the report.');
        }
    }
    //end specific script for old reports(encompass and ADuser report)
    startLoadingIcon();
  
    $.ajax({
        url: '/Reports/GenerateReport',
        data: {
            requestForUsers: requestForUsers,
            selectUserID: selectUserId,
            reportType: reportType,
            selectedManager: selectedManager,
            reporteesType: reporteesType,
            selectedStatus: selectedStatus,
            includeGenericAccounts:includeGenericUserAccounts
        },
        datatype: "text",
        type: 'POST',
        success: function (response) {
            if (typeof (response) !== "string") {
                if (response.ErrorMessage !== 'undefined') {
                    alert(response.ErrorMessage);
                }
            } else {
                $('#ReportPlaceholder').html(response);
                $('#RequestSummary')
                    .DataTable({
                        dom: 'Bfrtip',
                        buttons: [
                            'excelHtml5'
                        ],
                        "scrollX": true
                    });
            }
        },
        error: function(response) {
        }
    });

    stopLoadingIcon();
}

function bindManagerAutocomplete() {
    $.ajax({
        url: '/Reports/BindManagerIdTextBox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.managersToBind;
            $("#txtSelectManager")
                .autocomplete({
                    source: users
                });
        },
        error: function (response) {
            alert('"Due to a technical error, we are not able to process your request. Please try again later.  Please contact help desk…"');
        }
    });
}
function startLoadingIcon() {
    $(document).ajaxStart(function () {
        $("#loading").show();
    });
}

function stopLoadingIcon() {
    $(document).ajaxStop(function () {
        $("#loading").hide();
    });
}

function ResetManagerReportSelection(selectReportType) {
    $("#divManagerSelection").show();
    $("#txtSelectManager").val('');
    $("#Reportees :input").prop("disabled", true);
    $('input[name="Reportees"]').prop('checked', false);
    $("#selectUserName").hide();
    $('#ddlStatus').prop('selectedIndex', 0);
    if (selectReportType === "Encompass - Manager") {
        $("#IncludeGenericUserAccounts").show();
    } else {
        $("#IncludeGenericUserAccounts").hide();
    }
}

function ResetUserReportSelection() {
    $("#divManagerSelection").hide();
    $("#IncludeGenericUserAccounts").hide();
    $("#selectUserName").show();
    PopulateUserDropdown();
}