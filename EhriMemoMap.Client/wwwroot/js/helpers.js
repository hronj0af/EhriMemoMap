var gallery;

function initGallery(id) {
    if (gallery !== undefined) {
        gallery.destroy();
    }

    var galleryElement = document.getElementById('documents_' + id);
    if (galleryElement == null)
        return;

    var imageCount = galleryElement.getElementsByTagName('img').length;

    gallery = new Viewer(galleryElement, {
        transition: false,
        toolbar: false,
        navbar: imageCount > 1,
        title: [1, function(image, imageData) {
            return image.alt.replace("image ", "");
        }]
        
    });

    gallery.view();
}