
$(document).ready(function () {
    $("#Sucessinfo").show();
    var btnExtractDocument = document.getElementById('BtnExtractDocument');
    var ddlLoanLogicReportType = document.getElementById('LoanLogicReportType');
    var $PostClosedReportType = $('#PostClosedReportType');
    var $lblPostClosedReportType = $('#lblPostClosedReportTypes');
   
    
    $PostClosedReportType.hide();
    $lblPostClosedReportType.hide();

    $("#LoanInfoTable").dataTable(
{
    "aoColumnDefs": [{
        "bSortable": false
    }],
}
);

    var loading = $("#loading");
    $("#BtnGenerateReports").click(function () {
       
        hideAllMessages();
        var loanNumbers = '';
        var chkCopyFlieMergeFailed = true;
        var continueProcessOnConversionFailure = true;

        $(document).ajaxStart(function () {
            loading.show();
        });
       
        var LoanInfoTable = $("#LoanInfoTable").dataTable();
        var rows1 = LoanInfoTable.fnGetNodes();
        for (var i = 0; i < rows1.length; i++) {
            loanNumbers += ($(rows1[i]).find("td:eq(0)").html()) + ',';
        }
        var reportType = $('#LoanLogicReportType :selected').val();
        if (reportType === "undefined" || reportType === "0" || reportType==="")
        {
            alert("Please Select Loan Logic Report Type.");
            return;
        }
       
        var postClosedReportType = $('#PostClosedReportType :selected').val();
        if (reportType === "2002" && (postClosedReportType === "undefined" || postClosedReportType === "0" || postClosedReportType === ""))
        {
            alert("Please Select Post Close Report Type.");
            return;
        }
        var monthid = $('#ddlMonths :selected').val();
        if (monthid === "undefined" || monthid === "0")
        {
            alert("Please Select Month.");
            return;
        }
    
        chkCopyFlieMergeFailed = $('#chkCopyFlieMergeFailed').prop('checked');
        continueProcessOnConversionFailure = $('#chkContinueProcessOnConversionFailure').prop('checked');
        var data = new FormData();
        data.append("loanNumbers", loanNumbers);
        data.append("reportType", reportType);
        data.append("postClosedReportType", postClosedReportType);
        data.append("monthid", monthid);
        data.append("chkCopyFlieMergeFailed", chkCopyFlieMergeFailed);
        data.append("continueProcessOnConversionFailure", continueProcessOnConversionFailure);
        $.ajax({
            type: "POST",
            url: "/LoanSearchResults/ExtractAndGenerateReports",
            data: data,
            processData: false,
            contentType: false,
            success: function (response) {

                if (response.errormessage != undefined) {
                   
                    if (response.status != undefined && response.status === 401)
                    {
                        window.location.href = '/Account/Login';
                    }
                    else {
                        showInfoMsg("ErrorInfo", response.errormessage);
                    }

                }
                else if (response.sucessmessage != undefined) {
                    if (window.confirm('Your request for generating the loan logics report and extracting documents will be processed & copied into the loan logics. In addition,to download the report locally,please click on save button else cancel to continue....')) {
                        window.location.href = "/LoanSearchResults/DownloadExcel?loanNumbers=" + loanNumbers + "&reportType=" + reportType + "&postClosedReportType=" + postClosedReportType;
                       
                    }
                    var loanInfoTable = $("#LoanInfoTable").dataTable();
                    loanInfoTable.fnClearTable();;
                    showInfoMsg("Sucessinfo", response.sucessmessage);
                    $('#LoanLogicReportType').prop('selectedIndex', 0);
                    $('#PostClosedReportType').prop('selectedIndex', 0);
                    $('#ddlMonths').prop('selectedIndex', 0);
                    $("#LoanLogicReportType").attr("disabled", "disabled");
                    $("#PostClosedReportType").attr("disabled", "disabled");
                    $("#ddlMonths").attr("disabled", "disabled");
                    $("#BtnGenerateReports").attr("disabled", "disabled");
                    showInfoMsg("Sucessinfo", response.sucessmessage);

                }
            },
            error: function (response) {
                
                showInfoMsg("ErrorInfo", response);
            }
        });

        $(document).ajaxStop(function () {

            loading.hide();
        });

    });

    ddlLoanLogicReportType.onchange = function (e) {
        //value 2002 is Loan Logics Post Close Report
        if ((ddlLoanLogicReportType.options[ddlLoanLogicReportType.selectedIndex].value) == '2002') {
            $PostClosedReportType.show();
            $lblPostClosedReportType.show();

        } else {
            $PostClosedReportType.hide();
            $lblPostClosedReportType.hide();
        }
    };
   

    function showInfoMsg(infoWrapId, msg) {

        $('#' + infoWrapId).show();
        $('#' + infoWrapId).find('span').html(msg);
    }

    function hideAllMessages() {
        $("#Sucessinfo").css('display', 'none');
        $("#ErrorInfo").css('display', 'none');

    }

});
