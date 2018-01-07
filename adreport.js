$(document).ready(function () {
    $('#RequestSummary').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'excelHtml5'
        ]
    });
    $("#btnSearch").click(function () {
        GenerateReport();
    });
});

function GenerateReport() {
    var data = new FormData();
    var requestForUser = $("#ddlUser").val();
    var selectUserID = $("#ddlUser").val();

    var requestForUsers = [];
    if (requestForUser == '') {
        $('#ddlUser option').each(function () {
            if ($(this).attr('value') != '') {
                requestForUsers.push($(this).attr('value'));
            }
        });
    }
    else {
        requestForUsers.push(requestForUser);
    }

    if (requestForUsers.length > 1) {
        alert('Report will be generated for all the users. This may take some time to generate the report.');
    }

    data.append("requestForUser", requestForUsers);
    data.append("selectUserID", selectUserID);

    $.ajax({
        url: '/ADReports/GenerateReport',
        data: data,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            $('#ADReportPlaceholder').html(response);
        },
        error: function (response) {
        }
    });
}