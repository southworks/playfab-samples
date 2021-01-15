elementExistsById = function (id) {
    var element = $("#" + id);
    return element.length > 0;
}

createCustomModal = function (id) {
    // doesn't exist an element with specified id
    if (!elementExistsById(id)) {
        return;
    }

    var element = $("#" + id);

    element.addClass("modal fade");
    element.attr("tabindex", -1);
    element.attr("role", "dialog");
    element.attr("aria-hidden", "true");

    var modalDialogDiv = document.createElement('div');
    modalDialogDiv.classList.add("modal-dialog");
    modalDialogDiv.setAttribute("role", "document");

    var modalContentDiv = document.createElement('div');
    modalContentDiv.classList.add("modal-content");

    var modalHeaderDiv = document.createElement('div');
    modalHeaderDiv.classList.add("modal-header");

    var modalTitle = document.createElement('h5');
    modalTitle.classList.add("modal-title");
    modalTitle.id = id + "-modal-title";

    var closeButton = document.createElement("button");
    closeButton.classList.add("close");
    closeButton.setAttribute("type", "button");
    closeButton.setAttribute("data-dismiss", "modal");
    closeButton.setAttribute("aria-label", "Close");

    var closeSpan = document.createElement('span');
    closeSpan.setAttribute("aria-hidden", "true");
    closeSpan.innerText = "x";

    closeButton.appendChild(closeSpan);

    var modalBodyDiv = document.createElement('div');
    modalBodyDiv.classList.add("modal-body");

    var bodyText = document.createElement("p");
    bodyText.id = id + "-modal-body";

    modalBodyDiv.appendChild(bodyText);

    modalHeaderDiv.appendChild(modalTitle);
    modalHeaderDiv.appendChild(closeButton);

    modalContentDiv.appendChild(modalHeaderDiv);
    modalContentDiv.appendChild(modalBodyDiv);

    modalDialogDiv.appendChild(modalContentDiv);

    element.append(modalDialogDiv);
}

showCustomModal = function (id) {
    // doesn't exist an element with specified id
    if (!elementExistsById(id)) {
        return;
    }

    $("#" + id).modal('show');
}

hideCustomModal = function (id) {
    // doesn't exist an element with specified id
    if (!elementExistsById(id)) {
        return;
    }

    $("#" + id).modal('hide');
}

disposeCustomModal = function (id) {
    // doesn't exist an element with specified id
    if (!elementExistsById(id)) {
        return;
    }

    $("#" + id).modal('dispose');
}

setCustomModalData = function (id, title, body) {
    if (!elementExistsById(id)) {
        return;
    }

    var titleId = id + "-modal-title";
    var bodyId = id + "-modal-body";

    var titleDiv = document.getElementById(titleId);
    var bodyDiv = document.getElementById(bodyId);

    titleDiv.innerText = title;
    bodyDiv.innerText = body;
}
