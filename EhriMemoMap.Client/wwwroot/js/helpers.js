var gallery;

function initGallery() {
    if (gallery !== undefined) {
        gallery.destroy();
    }

    var galleryElement = document.getElementById('documents');
    if (galleryElement == null)
        return;

    gallery = new Viewer(galleryElement, {
        transition: false,
        toolbar: false,
        title: [1, function(image, imageData) {
            return image.alt.replace("image ", "");
        }]
        
    });
}

function showImage(imageNumber) {
    if (gallery == undefined)
        return;
    console.log(imageNumber);
    gallery.view(imageNumber);
}
