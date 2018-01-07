function ParseNewUserITSetupDescription(requestSummary) {
    var table = document.createElement('table');
    table.setAttribute("class", "table table-condensed");
    table.setAttribute("id", "newUserITSetupDetailsOuterTable");
    var tbody = document.createElement("tbody");
    var columnCount = 3;
    var curentColumnIndex = 0;
    var thPointer = true;
    var tr = document.createElement("tr");
    for (var parentKey in requestSummary) {
        if (requestSummary.hasOwnProperty(parentKey)) {
            var currentTopChild = requestSummary[parentKey];
            var td = document.createElement("td");
            td.style.verticalAlign = "top";
            td.style.width = (Math.floor(100 / columnCount)) + "%";
            var innerTable = document.createElement('table');
            innerTable.setAttribute("class", "table table-striped table-condensed table-hover");
            var thead = document.createElement("thead");
            var innerHeaderTr = document.createElement("tr");
            var innerHeaderTd = document.createElement(thPointer ? "th" : "td");
            thPointer = false;
            innerHeaderTd.innerHTML = "<strong><u>" + parentKey + "<u><strong/>";
            innerHeaderTd.colSpan = 2;
            innerHeaderTr.appendChild(innerHeaderTd);
            innerTable.appendChild(innerHeaderTr);
            innerTable.appendChild(thead);
            var innerTbody = document.createElement("tbody");

            for (var childKey in currentTopChild) {
                if (currentTopChild.hasOwnProperty(childKey)) {
                    var innerTr = document.createElement("tr");
                    innerTr.style.borderTop = "1px solid #dddbdb";
                    innerTr.style.borderBottom = "1px solid #dddbdb";
                    var innerTd1 = document.createElement("td");
                    var innerTd2 = document.createElement("td");
                    innerTd1.style.verticalAlign = "top";
                    innerTd2.style.verticalAlign = "top";
                    innerTd1.innerHTML = childKey + ":";
                    var contentValue = currentTopChild[childKey];
                    if (childKey.toLowerCase().indexOf("date") > 0 && contentValue && contentValue.indexOf("T") > 0) {
                        contentValue = contentValue.substr(0, contentValue.indexOf('T'));
                    }
                    if (typeof (contentValue) === "boolean") {
                        contentValue = contentValue ? "Yes" : "No";
                    }
                    if ($.isArray(contentValue)) {
                        var concatinatedValue = contentValue.join(", ");
                        contentValue = concatinatedValue;
                    }

                    innerTd2.innerHTML = contentValue;
                    innerTr.appendChild(innerTd1);
                    innerTr.appendChild(innerTd2);
                    innerTbody.appendChild(innerTr);
                }
            }
            innerTable.appendChild(innerTbody);

            td.appendChild(innerTable);
            tr.appendChild(td);
            curentColumnIndex++;
            if (curentColumnIndex === columnCount) {

                tbody.appendChild(tr);
                tr = document.createElement("tr");
                curentColumnIndex = 0;
            }
        }
    }
    table.appendChild(tbody);
    var newUserItsetupDataHolder = document.getElementById("newUserItSetupSpan");
    if (newUserItsetupDataHolder) {
        newUserItsetupDataHolder.appendChild(table);
        $("#modeldialog").css("width", (($(window).width() * 7) / 10) + "px").css("height", (($(window).width() * 8) / 10) + "px");
    }
}
$(window).resize(function () {
    $("#modeldialog").css("width", (($(window).width() * 7) / 10) + "px");
});