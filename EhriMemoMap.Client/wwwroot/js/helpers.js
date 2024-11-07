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
        title: [1, function(image, imageData) {
            return image.alt.replace("image ", "");
        }]
        
    });
}
