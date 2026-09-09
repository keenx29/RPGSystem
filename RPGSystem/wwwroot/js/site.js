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
    applyInventoryDefaults(selectedKind);
}
function setInventoryFieldValue(id, value) {
    const field = document.getElementById(id);

    if (field) {
        field.value = value;
    }
}

function applyInventoryDefaults(itemKind) {
    switch (itemKind) {
        case "general":
            setInventoryFieldValue("inventoryItemName", "");
            setInventoryFieldValue("inventoryItemWeight", "0.5");
            setInventoryFieldValue("inventoryItemDescription", "");
            break;

        case "healingpotion":
            setInventoryFieldValue("inventoryItemName", "Healing Potion");
            setInventoryFieldValue("inventoryItemWeight", "0.5");
            setInventoryFieldValue(
                "inventoryItemDescription",
                "Restores 2d4 + 2 hit points when used.");
            setInventoryFieldValue("inventoryHealingDice", "2d4+2");
            break;

        case "weapon":
            setInventoryFieldValue("inventoryItemName", "");
            setInventoryFieldValue("inventoryItemWeight", "3");
            setInventoryFieldValue("inventoryItemDescription", "");
            break;

        case "armor":
            setInventoryFieldValue("inventoryItemName", "");
            setInventoryFieldValue("inventoryItemDescription", "");
            updateArmorWeightDefault();
            break;
    }
}

function updateArmorWeightDefault() {
    const armorType = document.getElementById("inventoryArmorType");
    const weight = document.getElementById("inventoryItemWeight");

    if (!armorType || !weight) {
        return;
    }

    const defaultWeights = {
        Light: "10",
        Medium: "40",
        Heavy: "65",
        Shield: "6"
    };

    weight.value = defaultWeights[armorType.value] ?? "10";
}

document.addEventListener("DOMContentLoaded", function () {
    const kindSelect = document.getElementById("inventoryItemKind");

    if (!kindSelect) {
        return;
    }

    kindSelect.addEventListener("change", updateInventoryAddFields);
    updateInventoryAddFields();
    const armorType = document.getElementById("inventoryArmorType");

    if (armorType) {
        armorType.addEventListener("change", updateArmorWeightDefault);
    }
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
function setupCurrencyAutoSave() {
    const currencyForm = document.querySelector("[data-currency-form]");

    if (!currencyForm) {
        return;
    }

    currencyForm
        .querySelectorAll("[data-currency-input]")
        .forEach(function (input) {
            input.addEventListener("change", function () {
                currencyForm.requestSubmit();
            });
        });
}

document.addEventListener("DOMContentLoaded", function () {
    setupCurrencyAutoSave();
});