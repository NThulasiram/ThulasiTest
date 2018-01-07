$(document).ready(function() {
    $(document).on('click', '#remove', function(e) {
        var id = $(this).data('id');
        if (confirm('Are you sure you want to remove this Configuration from the DB')) {

            $.ajax({
                type: "POST",
                url: "/ApplicationConfiguration/RemoveConfiguration",
                datatype: "text",
                data: { 'command': id },
                success: function(data) {
                    // showInfoMsg("Sucessinfo", data.message);
                    alert(data.message);
                    window.location.reload();
                },
                error: function() {
                    //showInfoMsg("ErrorInfo", data.message);
                    alert('Something went Wrong !!!. Please try again');
                    window.location.reload(true);
                }
            });
        }
    });
    $("#partialView").dialog({
        autoOpen: false,
        draggable: true,
        resizable: false,
        width: ($(window).width() * .4),
        height: ($(window).height() * .5),
        modal: true,
        title: 'Add configurations',
        dialogClass: 'no-close'
    });
    $(document).on('click', '#addPopUp', function(e) {
        if (jQuery('#updatePartialView').dialog('isOpen')) {
            jQuery('#updatePartialView').hide();
        }
        $("#partialView").dialog("open");
        $(".ui-dialog").show();
        $(".ui-widget-overlay").show();
        $("#partialView").show();
        jQuery('#updatePartialView').hide(); // added to get rid of duplicate pop ups first click *** Important
    });

    $("#updatePartialView").dialog({
        autoOpen: false,
        draggable: true,
        resizable: false,
        width: ($(window).width() * .4),
        height: ($(window).height() * .5),
        modal: true,
        title: 'Modify configurations',
        dialogClass: 'no-close'
    });

    $(document).on('click', '#editConfig', function(e) {
        var id = $(this).data('id');
        $.ajax({
            type: "POST",
            url: "/ApplicationConfiguration/EditConfigurator",
            datatype: "text",
            data: { id: id },
            success: function(response) {
                $('#updatePartialView').html(response);
                // $("#updatePartialView").dialog("open");
                // $("#updatePartialView").show();
            },
            error: function() {
                alert('Something went Wrong !!!. Please try again');
                window.location.reload(true);
            }
        });
        if (jQuery('#partialView').dialog('isOpen')) {
            jQuery('#partialView').hide();
        }
        $("#updatePartialView").dialog("open");
        $(".ui-dialog").show();
        $(".ui-widget-overlay").show();
        $("#updatePartialView").show();
        jQuery('#partialView').hide(); // added to get rid of duplicate pop ups on first click *** Important
    });
    $("#Config").dataTable(
        {
            "aoColumnDefs": [
                {
                    "bSortable": false,
                    "aTargets": [0, 1, 2, 3]
                }
            ]
        }
    );
});
