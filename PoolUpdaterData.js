var lbGridNext = 'NEXT';
var lbGridPrevious = 'PREV';
var lbGridEmpty = 'GRIDEMPTY';
var oTable;
$(document).ready(function () {

    var btnfilechange = document.getElementById('btnfilechange');
    var ddlSelectedSheet = document.getElementById('Excel');
    var btnOpenExcel = document.getElementById('btnOpenExcel');
    var btnUpload = document.getElementById('btnUpload');

    $('#Exporttab').hide();
    ddlSelectedSheet.disabled = true;
    btnOpenExcel.disabled = true;
    btnUpload.disabled = true;

    btnfilechange.onchange = function (e) {
        var oimportPool = $("#importPool").dataTable();
        oimportPool.fnClearTable();
        oimportPool.fnDraw();
        oimportPool.fnDestroy();
        $('#Exporttab').hide();
        hideAllMessages();
        ddlSelectedSheet.disabled = true;
        btnOpenExcel.disabled = true;
        btnUpload.disabled = true;
        $('#Excel').empty();
        var val = $('.placeholder').attr('rel');
        var ext = this.value.match(/\.([^\.]+)$/)[1];
        switch (ext) {
            case 'xlsx':
            case 'xls':
                break;
            default:
                showInfoMsg("ErrorInfo", "Please select an excel template.");
                return false;
        }


        var data = new FormData();
        var input = $("#btnfilechange")[0].files[0];
        data.append("browsefile", input);

        $.ajax({
            url: '/PoolUpdater/ImportPoolData',
            data: data,

            processData: false,
            contentType: false,
            type: 'POST',
            success: function (response) {
                ddlSelectedSheet.disabled = false;

                $('#Excel').empty();


                var optionhtml1 = '<option value="' +
                    0 + '">' + "Select Excel Sheet" + '</option>';
                $(".ddlProjectvalue").append(optionhtml1);

                $.each(response.selectsheetlist, function (i) {

                    var optionhtml = '<option value="' +
                        response.selectsheetlist[i].Value + '">' + response.selectsheetlist[i].Text + '</option>';
                    $(".ddlProjectvalue").append(optionhtml);
                });
            },
            error: function (response) {
                showInfoMsg("ErrorInfo", "Due to a technical error, we are not able to process your request. Please try again later.  Please contact help desk…");
            }
        });

        return false;
    };

    function loadPooldata() {

        var selectSheetName = ddlSelectedSheet.options[ddlSelectedSheet.selectedIndex].text;
        var excelFilename = $("#btnfilechange")[0].files[0].name;
        btnUpload.enable = true;
        var oimportPool = $("#importPool").dataTable();
        oimportPool.fnDestroy();
        oimportPool.DataTable({
            "ajax": {
                "url": "/PoolUpdater/LoadPoolData",
                "type": "POST",
                "datatype": "json"
            },
            "fnServerParams": function (aData) {
                aData.push({ "name": "excelFilename", "value": excelFilename });
                aData.push({ "name": "sheetName", "value": selectSheetName });
            },
            "fnDrawCallback": function (data) {

                if (data.json != undefined) {
                    if (data.json.data.length > 0) {
                        btnUpload.disabled = false;
                    } else {
                        btnUpload.disabled = true;
                    }
                    if (data.json.errormessage != undefined) {
                        showInfoMsg("ErrorInfo", data.json.errormessage);
                    }
                    else if (data.json.sucessmessage != undefined) {
                        showInfoMsg("Sucessinfo", data.json.sucessmessage);
                    }

                }

            },
            "aoColumnDefs": [{
                "bSortable": false,
                "aTargets": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34]
            }],
            "columns": [
            {
                render: function () {
                    return '<input type="checkbox">';
                }, "width": "25px",
                "orderable": false
            },
            { "data": "Column1", "autowidth": true },
            { "data": "Column2", "autowidth": true },
            { "data": "Column3", "autowidth": true },
            { "data": "Column4", "autowidth": true },
            { "data": "Column5", "autowidth": true },
            { "data": "Column6", "autowidth": true },
            { "data": "Column7", "autowidth": true },
            { "data": "Column8", "autowidth": true },
            { "data": "Column9", "autowidth": true },
            { "data": "Column10", "autowidth": true },
            { "data": "Column11", "autowidth": true },
            { "data": "Column12", "autowidth": true },
            { "data": "Column13", "autowidth": true },
            { "data": "Column14", "autowidth": true },
            { "data": "Column15", "autowidth": true },
            { "data": "Column16", "autowidth": true },
            { "data": "Column17", "autowidth": true },
            { "data": "Column18", "autowidth": true },
            { "data": "Column19", "autowidth": true },
            { "data": "Column20", "autowidth": true },
            { "data": "Column21", "autowidth": true },
            { "data": "Column22", "autowidth": true },
            { "data": "Column23", "autowidth": true },
            { "data": "Column24", "autowidth": true },
            { "data": "Column25", "autowidth": true },
            { "data": "Column26", "autowidth": true },
            { "data": "Column27", "autowidth": true },
            { "data": "Column28", "autowidth": true },
            { "data": "Column29", "autowidth": true },
            { "data": "Column30", "autowidth": true },
            { "data": "Column31", "autowidth": true },
            { "data": "Column32", "autowidth": true },
            { "data": "Column33", "autowidth": true },
            { "data": "Column34", "autowidth": true },
            { "data": "Column35", "autowidth": true },
            { "data": "Column36", "autowidth": true }]
        });

    }

    function exportFailedLoans() {
        hideAllMessages();
        var excelFilename = $("#btnfilechange")[0].files[0].name;
        window.location = '@Url.Action("ExportFail", "PoolUpdater", new {sheetname = "ID"})'.replace("ID", excelFilename);
    }

    function enablingbtnOpenExcel() {

        var oimportPool = $("#importPool").dataTable();
        oimportPool.fnClearTable();
        oimportPool.fnDraw();
        oimportPool.fnDestroy();
        hideAllMessages();
        btnOpenExcel.disabled = true;
        btnUpload.disabled = true;

        var selectvalue = $('#Excel :selected').text();
        if (selectvalue === "Select Excel Sheet") {
            btnOpenExcel.disabled = true;
        } else {

            btnOpenExcel.disabled = false;
        }
    }

    ddlSelectedSheet.onchange = function (e) {

        enablingbtnOpenExcel();

    };

    btnOpenExcel.onclick = function (e) {

        hideAllMessages();
        loadPooldata();

    };

    $(function () {
        // Reference the auto-generated proxy for the hub.
        var server = $.connection.notificationHub;
        // Create a function that the hub can call back to display messages.
        server.client.notifyLoanUpdateStatus = function (message) {
            // Add the message to the page.
            var messageDiv = $('#notifications');

            if (message.indexOf("failed") !== -1 || message.indexOf("Failed") !== -1 || message.indexOf("failure") !== -1) {
                messageDiv.append('<li><font color="red">' + htmlEncode(message) + '</font></li>');
            }
            else if (message.indexOf("Vendor address could not be updated") !== -1) {
                messageDiv.append('<li><font color="green">' + htmlEncode(message) + '</font></li>');
            }
            else {
                messageDiv.append('<li>' + htmlEncode(message) + '</li>');
            }

            messageDiv.animate({ scrollTop: messageDiv.prop("scrollHeight") - messageDiv.height() });
        };

        server.client.stopHub = function (message) {
            // Add the message to the page.
            $('#notifications').append('<li>' + htmlEncode(message) + '</li>');            
            $.connection.hub.stop();
        };

        // Start the connection.
        var loading = $("#loading");
        $('#btnUpload').click(function () {
            $.connection.hub.start().done(function () {
                $('#notifications').html("");
                hideAllMessages();
                var isconfirm = confirm("Are you sure you wish to upload this pool data.");
                if (isconfirm) {
                    $('#notifications').html("<li>Connecting to Encompass server...</li>");
                    var selectSheetName = ddlSelectedSheet.options[ddlSelectedSheet.selectedIndex].text;
                    var excelFilename = $("#btnfilechange")[0].files[0].name;
                    $("body").css("cursor", "wait");
                    btnOpenExcel.disabled = true;
                    btnUpload.disabled = true;
                    ddlSelectedSheet.disabled = true;
                    btnfilechange.disabled = true;
                    $('#Exporttab').hide();
                    $("#importPool_wrapper :input").prop("disabled", true);
                    $('#importPool_paginate').hide();

                    $(document).ajaxStart(function () {
                        loading.show();
                    });

                    var excludedLoans = '';
                    var table = $("#importPool").dataTable();
                    var rows = table.fnGetNodes();

                    for (var i = 0; i < rows.length; i++) {
                        if ($(rows[i]).children("td:first-child").children("input[type='checkbox']").is(':checked')) {
                            excludedLoans += ($(rows[i]).find("td:eq(1)").html()) + ',';
                        }
                    }

                    var connectionId = $.connection.hub.id;
                    $("#ExportPool").dataTable().fnDestroy();
                    $('#ExportPool').DataTable({
                        "ajax": {
                            "url": "/PoolUpdater/UploadPoolData",
                            "type": "POST",
                            "datatype": "json"
                        },
                        "fnServerParams": function(aData) {
                            aData.push({ "name": "excludedLoans", "value": excludedLoans });
                            aData.push({ "name": "connectionId", "value": connectionId });
                            aData.push({ "name": "selectSheetName", "value": selectSheetName });
                            aData.push({ "name": "excelFilename", "value": excelFilename });
                        },
                        "fnDrawCallback": function(data) {

                            if (data.json != undefined) {

                                var oimportPool = $("#importPool").dataTable();
                                oimportPool.fnClearTable();
                                oimportPool.fnDraw();
                                oimportPool.fnDestroy();
                                hideAllMessages();
                                btnUpload.disabled = true;
                                ddlSelectedSheet.disabled = false;
                                btnfilechange.disabled = false;

                                if (data.json.data.length > 0) {
                                    $('#Exporttab').show();
                                    showInfoMsg("ExportSucessinfo", "Below loans failed to import per the reason provided.");
                                } else {
                                    $('#Exporttab').hide();
                                }
                                if (data.json.errormessage != undefined) {
                                    showInfoMsg("ErrorInfo", data.json.errormessage);
                                } else if (data.json.sucessmessage != undefined) {
                                    showInfoMsg("Sucessinfo", data.json.sucessmessage);
                                }
                                $('#notifications').append('<li>' + 'Loan update process completed...' + '</li>');
                            }
                        },
                        "dom": "Bfrtip",
                        "buttons": [
                            {
                                extend: 'excel',
                                text: 'Export Pool Data(Excel)'
                            }
                        ],
                        "aoColumnDefs": [
                            {
                                "bSortable": false,
                                "aTargets": [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34]
                            }
                        ],
                        "columns": [
                            { "data": "Column1", "width": "20%;" },
                            { "data": "Column2", "autowidth": true },
                            { "data": "Column3", "autowidth": true },
                            { "data": "Column4", "autowidth": true },
                            { "data": "Column5", "autowidth": true },
                            { "data": "Column6", "autowidth": true },
                            { "data": "Column7", "autowidth": true },
                            { "data": "Column8", "autowidth": true },
                            { "data": "Column9", "autowidth": true },
                            { "data": "Column10", "autowidth": true },
                            { "data": "Column11", "autowidth": true },
                            { "data": "Column12", "autowidth": true },
                            { "data": "Column13", "autowidth": true },
                            { "data": "Column14", "autowidth": true },
                            { "data": "Column15", "autowidth": true },
                            { "data": "Column16", "autowidth": true },
                            { "data": "Column17", "autowidth": true },
                            { "data": "Column18", "autowidth": true },
                            { "data": "Column19", "autowidth": true },
                            { "data": "Column20", "autowidth": true },
                            { "data": "Column21", "autowidth": true },
                            { "data": "Column22", "autowidth": true },
                            { "data": "Column23", "autowidth": true },
                            { "data": "Column24", "autowidth": true },
                            { "data": "Column25", "autowidth": true },
                            { "data": "Column26", "autowidth": true },
                            { "data": "Column27", "autowidth": true },
                            { "data": "Column28", "autowidth": true },
                            { "data": "Column29", "autowidth": true },
                            { "data": "Column30", "autowidth": true },
                            { "data": "Column31", "autowidth": true },
                            { "data": "Column32", "autowidth": true },
                            { "data": "Column33", "autowidth": true },
                            { "data": "Column34", "autowidth": true },
                            { "data": "Column35", "autowidth": true },
                            { "data": "Column36", "autowidth": true },
                            { "data": "FailedReason", "autowidth": true }
                        ]
                    });
                    $("body").css("cursor", "default");
                }

            });
        
        });

        $(document).ajaxStop(function () {
            loading.hide();
        });
    });

});


function showInfoMsg(infoWrapId, msg) {

    $('#' + infoWrapId).show();
    $('#' + infoWrapId).find('span').html(msg);
}

function hideAllMessages() {
    $("#Sucessinfo").css('display', 'none');
    $("#ErrorInfo").css('display', 'none');
}

// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}