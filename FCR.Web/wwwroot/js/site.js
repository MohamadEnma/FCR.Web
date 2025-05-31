$(document).ready(function () {
    // Add new URL input field
    $('#addImageUrlBtn').click(function () {
        var newInput = `
                    <div class="input-group mb-2">
                        <input type="url" name="ImageUrls" class="form-control" placeholder="https://example.com/image.jpg" />
                        <button type="button" class="btn btn-outline-danger remove-image-btn">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>`;
        $('#imageUrlsContainer').append(newInput);
    });

    // Remove URL input field
    $(document).on('click', '.remove-image-btn', function () {
        if (!$(this).is(':disabled')) {
            $(this).closest('.input-group').remove();
        }
    });
});