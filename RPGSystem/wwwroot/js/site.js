function updateInventoryAddFields() {
    const kindSelect = document.getElementById("inventoryItemKind");

    if (!kindSelect) {
        return;
    }

    const selectedKind = kindSelect.value.toLowerCase();

    document.querySelectorAll("[data-inventory-fields]").forEach(function (section) {
        const sectionKind = section.getAttribute("data-inventory-fields");
        section.style.display = sectionKind === selectedKind ? "" : "none";
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const kindSelect = document.getElementById("inventoryItemKind");

    if (!kindSelect) {
        return;
    }

    kindSelect.addEventListener("change", updateInventoryAddFields);
    updateInventoryAddFields();
});

const sheetScrollKey = "rpgsystem.sheetScrollY";

function rememberSheetScrollPosition() {
    if (!document.querySelector(".sheet-page")) {
        return;
    }

    document.querySelectorAll(".sheet-page form").forEach(function (form) {
        form.addEventListener("submit", function () {
            sessionStorage.setItem(sheetScrollKey, window.scrollY.toString());
        });
    });
}

function restoreSheetScrollPosition() {
    if (!document.querySelector(".sheet-page")) {
        sessionStorage.removeItem(sheetScrollKey);
        return;
    }

    const savedScrollY = sessionStorage.getItem(sheetScrollKey);

    if (!savedScrollY) {
        return;
    }

    sessionStorage.removeItem(sheetScrollKey);

    requestAnimationFrame(function () {
        window.scrollTo({
            top: Number(savedScrollY),
            behavior: "instant"
        });
    });
}

document.addEventListener("DOMContentLoaded", function () {
    rememberSheetScrollPosition();
    restoreSheetScrollPosition();
});