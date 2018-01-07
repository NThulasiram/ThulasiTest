
$(document).ready(function () {


    var btnOpenExcel = document.getElementById('btnOpenExcel');
    var btnfilechange = document.getElementById('btnfilechange');
    var txtmultiline = document.getElementById('lblImportLoanList');
    var ddlSelectedSheet = document.getElementById('Excel');
    var btnImportSearchLoan = document.getElementById('btnImportSearchLoan');
    var $linkTemplateDownload = $('#TemplateDownload');

    //Advance Serch fields
    var btnSave = document.getElementById('#btnSave');
    var ddlFieldList = document.getElementById('#ddlFieldList');
    var ddlOperatorList = document.getElementById('#ddlOperatorList');
    var txtValue = document.getElementById('#txtValue');
    ddlSelectedSheet.disabled = true;
    btnOpenExcel.disabled = true;
    btnImportSearchLoan.disabled = true;


    btnfilechange.onchange = function (e) {
        hideAllMessages();
        ddlSelectedSheet.disabled = true;
        btnOpenExcel.disabled = true;
        btnImportSearchLoan.disabled = true;
        txtmultiline.innerHTML = "";

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
            url: '/SearchLoans/GetSheetFromExcel',
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
        hideAllMessages();
        var sheetName = ddlSelectedSheet.options[ddlSelectedSheet.selectedIndex].text;
        var serverfilepath = $("#btnfilechange")[0].files[0].name;
        var data = new FormData();
        data.append("ExcelFilePath", serverfilepath);
        data.append("SeletedSheetName", sheetName);
        $.ajax({
            url: '/SearchLoans/OpenExcelSheet',
            data: data,
            processData: false,
            contentType: false,
            type: 'POST',
            success: function (response) {
           
                    if (response.errormessage != undefined) {
                        showInfoMsg("ErrorInfo", response.errormessage);
                    }
                    else
                    {
                        btnImportSearchLoan.disabled = false;
                        txtmultiline.innerHTML = response.data;
                    }
                
               
            },
            error: function (response) {
                showInfoMsg("ErrorInfo", "Due to a technical error, we are not able to process your request. Please try again later.  Please contact help desk…");
            }
        });

        };
    btnOpenExcel.onclick = function (e) {
        loadPooldata();
    };

    

    function enablingbtnOpenExcel() {

    
        btnOpenExcel.disabled = true;
        btnImportSearchLoan.disabled = true;
        txtmultiline.innerHTML = "";

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
   
    $("#SearchLoan").submit(function () {
        $('#btnTxtSearchLoans').prop("disabled", "disabled");
        $('#TemplateDownload').hide();
    });
    $("#ImportSearchLoan").submit(function () {
        $('#btnImportSearchLoan').prop("disabled", "disabled");
        $('#TemplateDownload').hide();
    });
   

    function showInfoMsg(infoWrapId, msg) {

        $('#' + infoWrapId).show();
        $('#' + infoWrapId).find('span').html(msg);

    }

    function hideAllMessages() {
        $("#Sucessinfo").css('display', 'none');
        $("#ErrorInfo").css('display', 'none');

    }


});



