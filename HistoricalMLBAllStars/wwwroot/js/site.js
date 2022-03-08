function ValidateYears() {
    startYear = document.getElementById("startYear").value;
    endYear = document.getElementById("endYear").value;
    if (endYear >= startYear) {
        document.getElementById("submitYears").style.visibility = "visible";
    }
    else {
        document.getElementById("submitYears").style.visibility = "hidden";
    }
}