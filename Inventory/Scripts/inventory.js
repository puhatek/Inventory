
/****** ---------------------------OrderItem page  ---------------------------******/
/* *********************************************************************************/
//---------------------------- FUNCTIONS FIRED ON EVENTS ---------------------------
/* *********************************************************************************/




//---------------------------- autocomplete -------------------------
function createAutocomplete() {
    var $input = $(this);

    if ($input.attr("Name") === "AssetName") {
        var options = {
            source: $input.attr("data-asset-autocomplete")
        };
    } else if ($input.attr("Name") === "Supplier") {
        options = {
            source: $input.attr("data-supplier-autocomplete")
        };
    }
    $input.autocomplete(options);
}

function checkIfMedicine(val) {

    $.ajax({
        dataType: "json",
        type: "GET",
        url: "OrderItem/isMedicine",
        data: { AssetName: val },
        success: function (data) {

            $("#addMedicine").prop("checked", data ? true : false);
        }
    });
}

function getLastSupplier(assetName) {
    $.ajax({
        url: "OrderItem/GetLastSupplier",
        dataType: "json",
        type: "GET",
        data: { AssetName: assetName },
        success: function (data) {

            $("input[name=Supplier]").prop("value", data);
        }
    });
}

function getUnitMeasure(assetName) {
    $.ajax({
        url: "OrderItem/GetUnitMeasure",
        dataType: "json",
        type: "GET",
        data: { AssetName: assetName },
        success: function (data) {
            $("input[name=UM]").prop("value", data);
        }
    }).fail(function () {
        $("#UM").val("");
    });
}

//---------------------------- order table events  -------------------------

function setOrderActionsVisibility(isVisible, theRow) {
    theRow.find("td div.orderActions").css("visibility", isVisible);
}



function updateSeledtedCheckBoxes(elem) {
    var isCheck = elem.is(":checked");
    $(".isSelected").prop("checked", isCheck ? isCheck : false);
}

function ShowDeleteMessage(elem) {
    //clear errors if any
    if ($("#ValidationMessage li").is(":visible")) {
        $("#Asset_errorMsg").empty();
        $("#Qty_errorMsg").empty();
        $("#ValidationMessage li").hide();
    }
    //store datas in cache
    $("#ModalMultiFuntionalBox").data("rowIdentity", {
        assetId: $.trim(elem.parents("tr").find("td.AssetIdHiddenRow").text()),
        orderId: $.trim(elem.parents("tr").find("td.OrderIdHiddenRow").text())
    });
    //pass parameteres to modal box
    var asset = elem.parents("tr").find("td div#assetRow").text();
    var um = elem.parents("tr").find("td div#umRow").text();
    var qty = elem.parents("tr").find("td div#qtyRow").text();

    showModalBox(asset, um, qty, 600, 'delete');
}

function setOrderMessage() {
    var token = $("#__myAjaxAntiForgeryForm input[name=__RequestVerificationToken]").val();
    var itemIdsForMailing = JSON.stringify(setOrderIds());
    $.ajax({
        url: "OrderMessage/Create",
        dataType: "json",
        type: "POST",
        data: {
            mailingItemIds: itemIdsForMailing,
            __RequestVerificationToken: token
        },
        success: function () {
            $.get("OrderItem/UpdateOrderList", { partialToReturn: "body" }, function (data) {
                $("#OrderListBodyContainer").html(data);
            });
            $("#SelectAll").prop("checked", false);
            $("#isSelected").prop("checked", false);
            $("#Options").val("BulkActions");
        }
    });
}


function setOrderIds() {
    var arrayIds = [];
    $("#OrderListBodyContainer tr").each(function () {
        var isChecked = $(this).find("input#isSelected").is(":checked");
        if (isChecked) {
            arrayIds.push({
                orderId: Number($(this).find("td.OrderIdHiddenRow").text().trim()),
                assetId: Number($(this).find("td.AssetIdHiddenRow").text().trim())
            });
        }
    });
    return arrayIds;
}

function AddNewAsset() {
    var assetName = $("#AssetName").val();
    alert(assetName);
    if (isAssetNew(assetName)) {
        //show modal box

        //add to stock with 'needed to be accepted by admin' status

        //send info to admin about attempt  to add new asset

        //update view 
    }
}


function isAssetNew(val) {
    $.get("OrderItem/IsAssetNew", { AssetName: val })
        .done(function (data) {
            return data;
        });
}

//---------------------------- modal box : new asset, delete order item  -------------------------

//inside the modal box there might be such tables: delete, add new asset ...
function showModalBox(asset, qty, um, modalBoxWidth, tableType) {

    switch (tableType) {
        case 'delete':
            createDeleteTable();
            break;
        case 'newAsset':
            createAddNewAsset();
            break;
    }

    setModalBoxCss(modalBoxWidth);

    setModalBoxValues(asset, qty, um);

    $("body").css("overflow", "hidden");
}

function setModalBoxCss(baseWidthValue) {
    $("#ModalMultiFuntionalBox").css({ "width": baseWidthValue + "px", "margin-left": Number(-baseWidthValue) / 2 + "px" });
    $("#ModalMultiFuntionalBox table").css({ "width": Number((baseWidthValue) * 0.8) + "px", "margin-left": Number(-(baseWidthValue) * 0.8) / 2 + "px" });
    $("#ModalMultiFuntionalBox b").css("margin-left", Number((baseWidthValue - (baseWidthValue) * 0.8) / 2) + "px");

    showHideElement($("#ModalMultiFuntionalBox"), 'visible');
    showHideElement($("#ModalBackground"), 'visible');

    setModalBoxControls("visible");
}

function setModalBoxValues(AssetName, Quantity, UnitMeasure) {
    $("#AssetToDelete").text(AssetName);
    $("#QtyToDelete").text(Quantity);
    $("#UMAssetToDelete").text(UnitMeasure);
}

function AcceptDelete() {
    $.ajax({
        url: "OrderItem/Delete",
        dataType: "json",
        type: "GET",
        data: {
            AssetId: $("#ModalMultiFuntionalBox").data("rowIdentity").assetId,
            OrderId: $("#ModalMultiFuntionalBox").data("rowIdentity").orderId
        },
        success: function () {

            setModalBoxControls("hidden");
            setTitle("Pozycja została usunięta.");
            $("#ModalMultiFuntionalBox").removeData("rowIdentity");

            $.get("OrderItem/UpdateOrderList", { partialToReturn: "header" }, function (data) {
                $("#OrderListHeaderContainer").html(data);

                data.preventDefault ? data.preventDefault() : data.returnValue = false;

            }).done(function () {
                $.get("OrderItem/UpdateOrderList", { partialToReturn: "body" }, function (data) {
                    $("#OrderListBodyContainer").html(data);

                    data.preventDefault ? data.preventDefault() : data.returnValue = false;

                    $(".DeleteRowLink").on("click", function () {
                        ShowDeleteMessage($(this));
                    });
                });
            });

            setTimeout(function () {
                closeModalBox();
            }, 1000);

        }
    });
}

function AcceptAdd() {
    var _assetName = $("#NewAssetName").val();
    var _isMedicine = $("#isNewAssetMedicine").is(":checked");
    var _stockName = $("#stocks").val();
    var _UM = $("#NewAssetUM").val();

    $.get("OrderItem/GetStockId", { stockName: _stockName })
       .done(function (data) {

           var _stockId = data;


           $.get("OrderItem/AddAsset", { AssetName: _assetName, isMedicine: _isMedicine, StockId: _stockId, UM: _UM })
              .done(function (data) {
                  if (data == 'done') {
                      alert("Item added");
                  } else{
                      alert(data);
                  }
                  setTimeout(function () {
                      closeModalBox();
                  }, 1000);
              });
       });

}

function ConfirmDelete() {
    showHideElement($("#ModalMultiFuntionalBox table"), "hidden");
    showHideElement($("#DeleteModalBoxButtons"), "hidden");
}

function showHideElement(elem, value) {
    elem.css("visibility", value);
}

function closeModalBox() {
    showHideElement($("#ModalMultiFuntionalBox"), "hidden");
    showHideElement($("#ModalBackground"), 'hidden');
    showHideElement($("#ModalMultiFuntionalBox table"), "hidden");
    showHideElement($("#DeleteModalBoxButtons"), "hidden");
    $("body").css("overflow", "auto");
}

function setModalBoxControls(visibility) {
    showHideElement($("#ModalMultiFuntionalBox table"), visibility);
    showHideElement($("#DeleteModalBoxButtons"), visibility);
}


function createDeleteTable() {
    var thValues = ["Towar", "J.M", "Ilość"];
    var tdIds = ["AssetToDelete", "UMAssetToDelete", "QtyToDelete"];
    var btnIds = ["DeleteNo", "DeleteYes"], btnValues = ["Anuluj", "Usuń"], btnClasses = ["btn btn-group-sm btn-success", "btn btn-group-sm btn-danger"];

    setTitle("Czy napewno usunąć pozycję zapotrzebowania?");

    setTable(thValues, "", "", "", tdIds, "", thValues.length);

    setButtons(btnValues, btnIds, btnClasses);
}

function createAddNewAsset() {
    var isAdmin;
    //sprawdzenie czy admin czy nie
    $.get("OrderItem/isAdmin", function (data) {
        isAdmin = data;


        //jeśli admin to trzeba dodać listę rozwijaną z magazynami
        //w przeciwnym razie magazyn wskazuje tabela UserStocks
        var thValues = ["Towar", "Indeks", "J.M", "Apteka"];
        var thClasses = ["", "my_1_5 centerMe", "my_1_5 centerMe", "my_1_5 centerMe"];
        if (isAdmin) {
            var tdControls = ["<input type='text' class='form-control' id='NewAssetName'/>",
                             "<select id='stocks'><option value='zabiegowy'>Zabiegowy</option><option value='stomatologia'>Stomatologia</option></select>",
                             "<input type='text' class='form-control' id='NewAssetUM'/>",
                             "<label><input type='checkbox' class='checkbox' id='isNewAssetMedicine'/></label>"];
        }else {
            var tdControls = ["<input type='text' class='form-control'  id='NewAssetName'/>",
                             "<input type='text' class='form-control' id='stocks'/>",
                             "<input type='text' class='form-control' id='NewAssetUM'/>",
                             "<label><input type='checkbox' class='checkbox' id='isNewAssetMedicine'/></label>"];
        }
        var tdClasses = ["NewAssetParametersRow", "my_1_5 centerMe NewAssetParametersRow", "my_1_5 centerMe NewAssetParametersRow", "my_1_5 centerMe NewAssetParametersRow"];
        var btnIds = ["AddNo", "AddYes"], btnValues = ["Anuluj", "Dodaj"], btnClasses = ["btn btn-group-sm btn-danger", "btn btn-group-sm btn-success"];

        setTitle("Dodaj nowy towar:");

        setTable(thValues, "", thClasses, tdControls, "", tdClasses, thValues.length);

        setButtons(btnValues, btnIds, btnClasses);
    });
}

function setTitle(text) {
    $("#modalTitle").html('');
    var $title = $("<b>" + text + "</b>");
    $("#modalTitle").append($title);
}

function setTable(thValues, thIds, thClasses, tdControls, tdIds, tdClasses, elementCount) {
    $("#ModalBoxTablePlaceHolder").html("");
    var $table = $("<table/>");
    $table.append(addRow("<th/>", thValues, thClasses, thIds, elementCount));
    $table.append(addRow("<td/>", tdControls, tdClasses, tdIds, elementCount));

    $("#ModalBoxTablePlaceHolder").append($table);
}

function setButtons(btnValues, btnIds, btnClasses) {

    $("#DeleteModalBoxButtons").html('');
    for (var i = 0; i < 2; i++) {
        var $input = $("<input/>");
        $input.attr({
            "type": "button",
            "id": btnIds[i],
            "value": btnValues[i],
            "class": btnClasses[i]
        });
        $("#DeleteModalBoxButtons").append($input);
    }
    //set delegate for closing modal box
    $("#" + btnIds[0]).on("click", function () {
        closeModalBox();
    });

    //set delegate for deleting a selected item
    if (btnIds[1] == "DeleteYes") {
        $("#" + btnIds[1]).on("click", function () {
            AcceptDelete();
        });
    }else {
        $("#" + btnIds[1]).on("click", function () {
            AcceptAdd();
        });
    }

}

function addRow(rowType, datas, classNames, ids, dataLength) {
    var $row = $("<tr/>");
    for (var i = 0; i < dataLength; i++) {
        var $cell = $(rowType);
        if (classNames != "") {
            $cell.addClass(classNames[i]);
        }
        if (ids != "") {
            $cell.attr("id", ids[i]);
        }
        if (datas != "") {
            $cell.append(datas[i]);
        }
        $row.append($cell);
    }
    return $row;
}

/* *********************************************************************************/
//---------------------------------------- ON EVENTS -------------------------------
/* *********************************************************************************/

//---------------------------- autocomplete -------------------------


$("input[data-asset-autocomplete]").each(createAutocomplete);
$("input[data-supplier-autocomplete]").each(createAutocomplete);

$("input[data-supplier-autocomplete]").autocomplete({
    change: function (event, ui) {
        var supplierName = $(this).val();
    }
});

$("input[data-asset-autocomplete]").autocomplete({
    change: function (event, ui) {
        var assetName = $(this).val();

        checkIfMedicine(assetName);

        getLastSupplier(assetName);

        getUnitMeasure(assetName);

        $("input[id=Qty]").focus();
    }
});

//---------------------------- order table events -------------------------

$("#OrderList").on("mouseenter", "tr", function () {
    setOrderActionsVisibility("visible", $(this));
});

$("#OrdersToSend").on("mouseenter", "tr", function () {
    setOrderActionsVisibility("visible", $(this));
});

$("#OrderList").on("mouseleave", "tr", function () {
    setOrderActionsVisibility("hidden", $(this));
});

$("#OrdersToSend").on("mouseleave", "tr", function () {
    setOrderActionsVisibility("hidden", $(this));
});

$(".DeleteRowLink").on("click", function () {
    ShowDeleteMessage($(this));
});

//$("#DeleteNo").on("click", function () {
//    closeModalBox();
//});

//$("#DeleteYes").on("click", function () {
//    AcceptDelete();
//});

$("#closePendingOrderList").on("click", function () {
    closeModalBox();
});

//-------------------------- checkboxes and multiple operations ------------

$("#SelectAll").on("change", function () {
    updateSeledtedCheckBoxes($(this));
});

$("#submitBulkOperation").click(function (event) {
    event.preventDefault();
    var selected = $("Select#Options").val();
    if (selected === 'Complete') {
        setOrderMessage();
    }
});


//-------------------------- add new asset operations ------------

$("Input[Value=Dodaj]").on("click", function () {
    AddNewAsset();
});

$("#AddNewAsset").on("click", function () {

    showModalBox("", "", "", 600, "newAsset");
});




/****** -----------------------OrderMessage page  ---------------------------******/

/* *********************************************************************************/
//---------------------------- FUNCTIONS  ---------------------------
/* *********************************************************************************/

function setEmail(orderId) {

    if (event.target.className != "notActive") {
        $("#MessageContainer").toggle();

        $.get("OrderMessage/GetMessageContent", { OrderId: orderId }, function (data) {
            $("#myTextArea").html(data);
        });

        $.get("OrderMessage/GetMessageRecipient", { OrderId: orderId }, function (data) {
            $("#raz").html(data);
        });
    }
}

function setMessageTemplate() {

    var li2change = $(".ul-msg-submenu").find("#messageTemplate");

    if (li2change.val() == "Zapisz szablon") {
        $("#myTextArea").attr("contenteditable", "false");
        var messageText = $("#myTextArea").html().trim();
        var newLinkName = li2change.val().replace("Zapisz szablon", "Edytuj szablon");
        li2change.val(newLinkName);
        var orderId = $(".highlight").find(".hiddenOrderId").text().trim();
        $.get("OrderMessage/SetMessageTemplate", { NewTextMessage: messageText, OrderId: orderId });
        return;
    }
    if (li2change.val() == "Edytuj szablon") {
        $("#myTextArea").attr("contenteditable", "true");
        var newLinkName = li2change.val().replace("Edytuj szablon", "Zapisz szablon");
        li2change.val(newLinkName);
    }
}


//functions, below one and showAddOptions, set behavior of message box's menu items
$(document.body).click(function (event) {

    if (event.target.className != "attachButton newTemplate zaznaczone") {
        if ($(".ul-msg-submenu").is(":visible")) {
            $(".ul-msg-submenu").toggle();
            $("#addNewMessageData").removeClass().addClass("attachButton newTemplate");
        }
    }

});

function showAddOptions() {
    $(".ul-msg-submenu").toggle();

    if ($("#addNewMessageData").hasClass("attachButton newTemplate zaznaczone")) {
        $("#addNewMessageData").removeClass().addClass("attachButton newTemplate");
    }else {
        $("#addNewMessageData").removeClass().addClass("attachButton newTemplate zaznaczone");
    }
}




function setClassesForSelectedRow(recepient, emailLink, row, pdfLink, isPdf) {

    $("input[value=Email]").removeClass().addClass("notActive");
    emailLink.removeClass("notActive").addClass("myLink SetEmailLink");


    $("input[value=Pdf]").removeClass().addClass("notActive");
    if (isPdf) {
        pdfLink.removeClass("notActive").addClass("myLink SetEmailLink");
    }

    $(".Receiver").removeClass("selected");
    recepient.addClass("Receiver selected");

    $("#OrdersToSend tr").removeClass();
    row.addClass("highlight");
    $("#MessageContainer").hide();
}



//-------------------------- the events of table of supplier's order assets - for OrderMessage page  ------------

$(".Supplier").click(function () {
    var row = $(this).parents("tr");
    var orderId = $(this).parents("tr").find(".hiddenOrderId").text().trim();
    var emailLink = $(this).parents("tr").find("input[value=Email]");
    var recepient = $(this).find("span.Receiver");
    var pdfLink = $(this).parents("tr").find("input[value=Pdf]");
    var isPdf = false;

    //Check if any of the assets is a medicine
    $.get("OrderMessage/ContainsMedicine", { OrderID: orderId }, function (data) {
        if (data) {
            isPdf = true;
        }
    });

    //Get details for the selected supplier and set the csses. 
    $.get("OrderMessage/Index", { OrderId: orderId }, function (data) {
        $("#OrderToSendDetailsContainer").html(data);
        setClassesForSelectedRow(recepient, emailLink, row, pdfLink, isPdf);
    });


});