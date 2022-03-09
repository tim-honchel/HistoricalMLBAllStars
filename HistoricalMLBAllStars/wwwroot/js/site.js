const images = document.querySelector('.carousel').children;
const totalImages = images.length;
let index = 0;
nextImage()

function nextImage() {
    
    index++;
    if (index == totalImages) {
        index = 0;
    }
    

    for (let i = 0; i < images.length; i++) {
        images[i].classList.remove('main');
    }
    images[index].classList.add('main');
    setTimeout(() => { nextImage() }, 1500);
}

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