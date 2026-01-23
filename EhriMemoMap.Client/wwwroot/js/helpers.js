var bp;

function initGallery(id, index = 0) {
    try {
        var container = document.getElementById('documents_' + id);
        if (!container) return;

        if (typeof BiggerPicture === 'undefined') {
            console.error('BiggerPicture script is not loaded. Verify script path in index.html.');
            return;
        }

        // Close existing instance if it exists
        if (bp && bp.close) {
            bp.close();
        }

        // Always create a fresh instance
        bp = BiggerPicture({ target: document.body });

        // Use anchor elements as items, per BiggerPicture examples
        var links = container.querySelectorAll('a');
        if (!links || links.length === 0) return;

        // Open on first item; you can change this to a specific index if needed
        bp.open({
            items: links,
            el: links[index],
            intro: "fadeup"
        });
    } catch (err) {
        console.error('initGallery error', err);
    }
}

function closeGallery() {
    if (bp === null || bp === undefined)
        return;
    bp.close();
}

function resetPageElementHeight() {
    const pageElement = document.getElementsByClassName("page");
    pageElement[0].style.height = "";
}