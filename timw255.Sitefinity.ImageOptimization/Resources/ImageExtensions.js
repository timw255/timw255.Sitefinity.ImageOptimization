function optimizeCommandHandler(sender, args) {
    if (args._commandName == "optimize") {

        var dataItem = clickedDataArgs.get_dataItem();

        jQuery.ajax({
            type: "POST",
            url: sender._baseUrl + "ImageOptimization/Optimization/?parentId=" + args._dataItem.ParentId + "&imageId=" + args._dataItem.Id,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            processdata: false,
            success: function (taskId) {
                sender.get_itemsGrid().dataBind();
            },
            error: function (jqXHR) {
                alert(Telerik.Sitefinity.JSON.parse(jqXHR.responseText).Detail);
            }
        });
    }
}

function imagesListLoaded(sender, args) {
    sender.add_itemCommand(optimizeCommandHandler);
}