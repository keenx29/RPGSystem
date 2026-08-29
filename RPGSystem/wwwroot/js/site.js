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