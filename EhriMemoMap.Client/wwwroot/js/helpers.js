//var gallery;

//function initGallery(id) {
//    if (gallery !== undefined) {
//        gallery.destroy();
//    }

//    var galleryElement = document.getElementById('documents_' + id);
//    if (galleryElement == null)
//        return;

//    var imageCount = galleryElement.getElementsByTagName('img').length;

//    gallery = new Viewer(galleryElement, {
//        transition: false,
//        toolbar: false,
//        navbar: imageCount > 1,
//        title: [1, function(image, imageData) {
//            return image.alt.replace("image ", "");
//        }]

//    });

//    gallery.view();
//}

var bp;

function initGallery(id) {
    try {
        var container = document.getElementById('documents_' + id);
        if (!container) return;

        if (typeof BiggerPicture === 'undefined') {
            console.error('BiggerPicture script is not loaded. Verify script path in index.html.');
            return;
        }

        if (!bp) {
            bp = BiggerPicture({ target: document.body });
        }

        // Use anchor elements as items, per BiggerPicture examples
        var links = container.querySelectorAll('a');
        if (!links || links.length === 0) return;

        // Open on first item; you can change this to a specific index if needed
        bp.open({
            items: links,
            el: links[0]
        });
    } catch (err) {
        console.error('initGallery error', err);
    }
}