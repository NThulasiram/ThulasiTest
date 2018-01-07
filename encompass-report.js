$(document).ready(function () {
    $('#RequestSummary').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'excelHtml5'
        ]
    });
    $("#ddlEnvironment").change(function () {
        bindUserAutoComplete();
    });

    $("#btnSearch").click(function () {
        GenerateReport();
    });

});

function bindUserAutoComplete() {
    var data = new FormData();
    var environment = $("#ddlEnvironment").val();
    data.append("environment", environment);
    $.ajax({
        url: '/EncompassReport/PopulateUsers',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            $('#EncompassReportPlaceholder').html(response);
        },
        error: function (response) {
        }
    });
}

function GenerateReport() {
    var data = new FormData();

    var requestForUser = $("#ddlUser").val();
    var environment = $("#ddlEnvironment").val();
    var selectUserID = $("#ddlUser").val();

    if (environment == '')
    {
        alert('Please select an environment.');
        return;
    }

    var requestForUsers = [];
    if (requestForUser == '')
    {
        $('#ddlUser option').each(function () {
            if ($(this).attr('value') != '') {
                requestForUsers.push($(this).attr('value'));
            }
        });
    }
    else
    {
        requestForUsers.push(requestForUser);
    }

    if (requestForUsers.length > 1)
    {
        alert('Report will be generated for all the users. This may take some time to generate the report.');
    }

    data.append("requestForUser", requestForUsers);
    data.append("environment", environment);
    data.append("selectUserID", selectUserID);

    $.ajax({
        url: '/EncompassReport/GenerateReport',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            $('#EncompassReportPlaceholder').html(response);
        },
        error: function (response) {
        }
    });
}