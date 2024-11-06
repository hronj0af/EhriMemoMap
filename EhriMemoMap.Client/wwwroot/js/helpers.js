var gallery;

function initGallery() {
    if (gallery !== undefined) {
        gallery.destroy();
    }
    gallery = new Viewer(document.getElementById('documents'), {
        transition: false,
        title: [1, function(image, imageData) {
            return image.alt.replace("image ", "");
        }]
        
    });
}
