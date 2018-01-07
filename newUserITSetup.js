var idSelector = function () { return this.id; };

$(document).ready(function () {
    $(document).tooltip();
    showDatePicker();
    bindManagerAutocomplete();
    bindRequesterAndModelE360AfterAutocomplete();
    //Toggle glyphicons
    $('.panel-group').on('hidden.bs.collapse', toggleIcon);
    $('.panel-group').on('shown.bs.collapse', toggleIcon);

    //submit
    $("#btnSubmit").click(function () {
        createNewRequest();
    });

    $("#fgmc-taining-yes").change(function () {
        var isTrainingYes = $("#fgmc-taining-yes").is(":checked");
        if (isTrainingYes) {
            $("#traininglocation,#trainingdate").addClass("required");
        }
    });
    $("#fgmc-taining-no").change(function () {
        var isTrainingNo = $("#fgmc-taining-no").is(":checked");
        if (isTrainingNo) {
            $("#traininglocation,#trainingdate").removeClass("required");
        }
    });

    $("[data-labelfor]").click(function () {
        var control = $('#' + $(this).attr("data-labelfor"));
        control.prop('checked',
            function (i, oldVal) {
                if (control.is(":checkbox")) {
                    return !oldVal;
                }
                if (control.is(":radio")) {
                    control.prop('checked', "checked");
                    control.triggerHandler("click");
                }
            });
        if (control.is(":checkbox")) {
            control.triggerHandler("click");
            control.triggerHandler("change");
        }
    });

    $("#requesteremail").blur(function () {
        var thisObject = $(this);
        var requesteremailValue = thisObject.val().trim();
        if (requesteremailValue !== '') {
            ///^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/
            var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/igm;
            if (!requesteremailValue.match(re)) {
                //thisObject.popover({
                //    html: true,
                //    content: '<label style="color: #b94a48;">Invalid format. Please use valid format.</label>',

                //});
                //thisObject.popover("show");
                $("#requesterEmailInvalidFormatErrorMessage").css("display", "block");
            }
        }
        else {
           // $("#requesteremail").popover('hide');
            $("#requesterEmailInvalidFormatErrorMessage").css("display", "none");

        }
    });
    $("#requesteremail").focus(function () {
       // $("#requesteremail").popover('hide');
        $("#requesterEmailInvalidFormatErrorMessage").css("display", "none");

    });
});

function showDatePicker() {
    $("#txtStartDate").datepicker();
    $("#trainingdate").datepicker();
}

function toggleIcon(e) {
    $(e.target).prev('.panel-heading').find(".more-less").toggleClass('glyphicon-plus glyphicon-minus');
}

function createNewRequest() {
    //validate
    var loading = $("#loading");
    if (ValidateMandatoryFields()) {
        if (confirm('Are you sure you want to submit the request?')) {
            $(document).ajaxStart(function () {
                loading.show();
            });

            submitRequest();

            $(document).ajaxStop(function () {
                loading.hide();
            });
        }
    }
}

function submitRequest() {
    var data = new FormData();
    //General Information 
    var firstname = $("#first-name").val();
    var lastname = $("#last-name").val();
    var title = $("#title").val();
    var branch = $("#branch").val();
    var dept = $("#department").val();
    var manager = $("#manager").val();
    var startDate = $("#txtStartDate").val();
    var requester = $("#requester").val();
    var requesteremail = $("#requesteremail").val();
    var isTrainingReqd = $("#fgmc-taining-yes").is(":checked");
    var traininglocation = $("#traininglocation").val();
    var trainingdate = $("#trainingdate").val();

    data.append("firstname", firstname);
    data.append("lastname", lastname);
    data.append("title", title);
    data.append("branch", branch);
    data.append("manager", manager);
    data.append("dept", dept);
    data.append("startDate", startDate);
    data.append("requester", requester);
    data.append("requesteremail", requesteremail);
    data.append("isTrainingReqd", isTrainingReqd);
    data.append("traininglocation", traininglocation);
    data.append("trainingdate", trainingdate);


    //Applications/External Systems
    var selectedExternalSystems = $("input:checkbox[name=external-systems]:checked").map(idSelector).get();
    data.append("selectedExternalSystems", selectedExternalSystems);
    var modelE360after = $("#modelE360after").val();
    data.append("modelE360after", modelE360after);

    //If Investor Portal(s) Access is selected :
    var selectedBanks = $("input:checkbox[name=bank]:checked").map(idSelector).get();
    data.append("selectedBanks", selectedBanks);

    //After Investors are selected
    var selectedstatus = $("input:radio[name=status]:checked").map(idSelector).get();
    data.append("selectedstatus", selectedstatus);

    var extenalSystemsOther = $("#extenalSystemsOther").val();
    var emailGroups = $("#emailGroups").val();
    data.append("extenalSystemsOther", extenalSystemsOther);
    data.append("emailGroups", emailGroups);

    //Automated Underwriting Systems 
    var automatedUnderwritingSystems = $("input:checkbox[name=automated-underwriting]:checked").map(idSelector).get();
    data.append("automatedUnderwritingSystems", automatedUnderwritingSystems);
    var automatedunderwritingsystemsNotes = $("#automatedunderwritingsystemsNotes").val();
    data.append("automatedunderwritingsystemsNotes", automatedunderwritingsystemsNotes);

    //Computer/Network Resources 
    var compOrNetworkResources = $("input:checkbox[name=network-resources]:checked").map(idSelector).get();
    data.append("compOrNetworkResources", compOrNetworkResources);

    var printerName = $("#printerName").val();
    data.append("printerName", printerName);

    var securedSharedDrives = $("#securedSharedDrives").val();
    data.append("securedSharedDrives", securedSharedDrives);

    var isRemoteAccessReqd = $("#remote-access-yes").is(":checked");
    data.append("isRemoteAccessReqd", isRemoteAccessReqd);

    //Phone Resources 
    var phoneResources = $("input:checkbox[name=phone-resources]:checked").map(idSelector).get();
    data.append("phoneResources", phoneResources);

    //Misc. Requirements/Notes 
    var isOffSiteEmp = $("#offsite-yes").is(":checked");
    var streetAddress = $("#streetAddress").val();
    var addressLine2 = $("#addressLine2").val();
    var city = $("#city").val();
    var state = $("#state").val();
    var zipcode = $("#zipcode").val();
    var country = $("#country").val();
    var miscRequirementsNotes = $("#miscRequirementsNotes").val();

    data.append("isOffSiteEmp", isOffSiteEmp);
    data.append("streetAddress", streetAddress);
    data.append("addressLine2", addressLine2);
    data.append("city", city);
    data.append("state", state);
    data.append("zipcode", zipcode);
    data.append("country", country);
    data.append("miscRequirementsNotes", miscRequirementsNotes);


    //Submit
    $.ajax({
        url: '/newuseritsetup/createnewrequest',
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
function DisplayMandatoryMessage() {
    alert("Please enter values for mandatory fields!");
}
function ValidateMandatoryFields() {


    var isTrainingReqd = $("#fgmc-taining-yes").is(":checked");
    var traininglocation = $("#traininglocation").val();
    var trainingdate = $("#trainingdate").val();
    if ($("#first-name").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#last-name").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#title").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#branch").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#department").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#manager").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#txtStartDate").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    if ($("#requester").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }
    var requesterEmail = $("#requesteremail").val();
    if (requesterEmail.trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }

    if (isTrainingReqd) {
        if ($("#traininglocation").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
        if ($("#trainingdate").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
    }
    else if (!isTrainingReqd && !$("#fgmc-taining-no").is(":checked")) {
        DisplayMandatoryMessage();
        return false;
    }


    if ($("#emailGroups").val().trim() == '') {
        DisplayMandatoryMessage();
        return false;
    }

    //if Encompass 360 is selected then provide the Model E360 after
    if ($("#dynamic_1").is(":checked")) {
        if ($("#modelE360after").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
    }

    //If Investor Portal(s) Access is selected user must select at least one Investor Portal
    if ($("#dynamic_3").is(":checked")) {
        var selectedBanks = $("input:checkbox[name=bank]:checked").map(idSelector).get();
        if (selectedBanks.length == 0) {
            DisplayMandatoryMessage();
            return false;
        }
        else {
            var selectedstatus = $("input:radio[name=status]:checked").map(idSelector).get();
            if (selectedstatus.length == 0) {
                DisplayMandatoryMessage();
                return false;
            }
        }
    }

    //if printer is selected then provide the printer name
    if ($("#dynamic_32").is(":checked")) {
        if ($("#printerName").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
    }
    var city = $("#city").val();
    var state = $("#state").val();
    var zipcode = $("#zipcode").val();
    var country = $("#country").val();

    //If Off-Site Employee is selected then Shipping Address is mandatory
    var isOffSiteEmp = $("#offsite-yes").is(":checked");
    if (isOffSiteEmp) {
        if ($("#streetAddress").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
        if ($("#city").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
        if ($("#state").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
        if ($("#zipcode").val().trim() == '') {
            DisplayMandatoryMessage();
            return false;
        }
        if ($("#country").val().trim() == '' || $("#country").val().trim() == '----') {
            DisplayMandatoryMessage();
            return false;
        }
    }

    return true;
}

function bindManagerAutocomplete() {
    $.ajax({
        url: '/newuseritsetup/bindmanagertextbox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.managersToBind;
            $("#manager").autocomplete({
                source: users
            });
        },
        error: function (response) {
        }
    });
}

function bindRequesterAndModelE360AfterAutocomplete() {
    $.ajax({
        url: '/newuseritsetup/BindRequesterTextBox',
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            var users = response.requestersToBind;
            $("#requester").autocomplete({
                source: users
            });

            $("#modelE360after").autocomplete({
                source: users
            });
        },
        error: function (response) {
        }
    });
}

//This must be at the last of file-- Add any code before it
$(function () {
    $(".shipping-address").slideUp();
    $(".fgmc-training-required").slideUp();
});

