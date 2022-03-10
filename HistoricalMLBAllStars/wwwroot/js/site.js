const images = document.querySelector('.carousel').children; //selects all images in slider
const totalImages = images.length;
let index = 0;
nextImage() //immediately begins transitioning images

function nextImage() {
    
    index++;
    if (index == totalImages) {
        index = 0; // after last image, resets to first
    }
    

    for (let i = 0; i < images.length; i++) {
        images[i].classList.remove('main');
    }
    images[index].classList.add('main');
    setTimeout(() => { nextImage() }, 1500); // calls next image every 1.5 seconds
}

function ValidateYears() { // triggered every time a dropdown list value changes
    startYear = document.getElementById("startYear").value;
    endYear = document.getElementById("endYear").value;
    if (endYear >= startYear) {
        document.getElementById("submitYears").style.visibility = "visible"; // shows the submit button if the date range is valid
    }
    else {
        document.getElementById("submitYears").style.visibility = "hidden"; // hides the submit button if the range is invalid
    }
}