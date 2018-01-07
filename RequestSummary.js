function showInfoMsg(infoWrapId, msg) {

    $('#' + infoWrapId).show();
    $('#' + infoWrapId).find('span').html(msg);

}

$(function () {
    $("#RequestSummary").dataTable();
});

$('#btnRequestSearch').click(function () {
    var requestType = $('#ddlRequestType').val();
    var MeStatus = $('#ddlMEStatus').val();
    startLoadingIcon();
    $.ajax({
        type: "POST",
        url: '/RequestSummary/GetSelectedTypeRequest',
        datatype: "text",
        data: {
            requestType: requestType,
            meStatus: MeStatus
        },
        success: function (response) {
            $('#DynamicRequestPlace').html(response);
        },
        error: function() {
            alert('Smething went Wrong !!!. Please try agian');
            window.location.reload(true);
        }
    });
    stopLoadingIcon();
});

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