$(document).ready(function () {
    // Constantes para mensajes y configuraciones comunes
    const MESSAGES = {
        SELECT_FILE: 'Por favor seleccione un archivo',
        CONFIRM_DELETE: '¿Está seguro de que desea eliminar este documento? Esta acción no se puede deshacer.',
        GENERIC_ERROR: 'Error al procesar la solicitud'
    };

    // Función para manejar errores AJAX
    function handleAjaxError(xhr, status, error, defaultMessage) {
        console.error('Error:', error);
        console.error('Status:', status);
        console.error('Response:', xhr.responseText);

        let errorMessage = defaultMessage;
        try {
            const response = JSON.parse(xhr.responseText);
            errorMessage = response.message || defaultMessage;
        } catch (e) {
            console.error('Error al parsear respuesta:', e);
        }
        alert(errorMessage);
    }

    $('.btn-save-document').click(function (e) {
        e.preventDefault();
        const documentId = $(this).data('document-id');
        const fileInput = $(`#file_${documentId}`);
        const documentCard = $(this).closest('.document-card');
        const statusElement = documentCard.find('.document-status');

        if (fileInput[0].files.length === 0) {
            toastr.warning(MESSAGES.SELECT_FILE);
            return;
        }

        const formData = new FormData();
        formData.append('file', fileInput[0].files[0]);
        formData.append('documentCatalogId', documentId);
        formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

        const button = $(this);
        const originalHtml = button.html();
        updateButtonState(button, true, 'Guardando...', originalHtml);

        $.ajax({
            url: '@Url.Action("SaveDocument", "Document")',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    updateDocumentStatus(documentId, documentCard);
                    toastr.success(response.message || 'Documento guardado exitosamente');

                    // Habilitar los botones de descargar y eliminar
                    documentCard.find('.btn-download-document, .btn-delete-document').prop('disabled', false);
                } else {
                    toastr.error(response.message || 'Error al guardar el documento');
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, status, error, 'Error al guardar el documento');
            },
            complete: function () {
                updateButtonState(button, false, '', originalHtml);
            }
        });
    });

    // Agregar esta nueva función para actualizar el estado
    function updateDocumentStatus(documentId, documentCard) {
        $.ajax({
            url: '@Url.Action("UpdateStatus", "Document")',
            type: 'POST',
            data: {
                documentCatalogId: documentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    const statusElement = documentCard.find('.document-status');

                    // Remover todas las clases de estado existentes
                    statusElement.removeClass('status-pending status-approved status-rejected');

                    // Agregar la nueva clase de estado
                    statusElement.addClass(response.statusClass);

                    // Actualizar el texto del estado
                    statusElement.html(`
                    <i class="fa-solid fa-circle-info me-1"></i>
                    ${response.statusText}
                `);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error al actualizar estado:', error);
            }
        });
    }

    // Manejador para descargar documentos
    $('.btn-download-document').click(function (e) {
        e.preventDefault();
        const button = $(this);
        const documentId = button.data('document-id');
        const originalHtml = button.html();

        updateButtonState(button, true, 'Descargando...', originalHtml);

        $.ajax({
            url: '@Url.Action("DownloadDocument", "Document")',
            type: 'GET',
            data: { documentCatalogId: documentId },
            xhrFields: { responseType: 'blob' },
            success: function (response, status, xhr) {
                const filename = xhr.getResponseHeader('content-disposition')
                    ? xhr.getResponseHeader('content-disposition').split('filename=')[1]
                    : `documento_${documentId}.pdf`;

                const url = window.URL.createObjectURL(new Blob([response]));
                const link = document.createElement('a');
                link.href = url;
                link.download = filename;
                document.body.appendChild(link);
                link.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(link);
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, status, error, 'Error al descargar el documento');
            },
            complete: function () {
                updateButtonState(button, false, '', originalHtml);
            }
        });
    });

    // Manejador para eliminar documentos
    $('.btn-delete-document').click(function (e) {
        e.preventDefault();
        if (!confirm(MESSAGES.CONFIRM_DELETE)) return;

        const button = $(this);
        const documentId = button.data('document-id');
        const originalHtml = button.html();

        updateButtonState(button, true, 'Eliminando...', originalHtml);

        $.ajax({
            url: '@Url.Action("DeleteDocument", "Document")',
            type: 'POST',
            data: {
                documentCatalogId: documentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    location.reload();
                } else {
                    alert(response.message || 'Error al eliminar el documento');
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, status, error, 'Error al eliminar el documento');
            },
            complete: function () {
                updateButtonState(button, false, '', originalHtml);
            }
        });
    });
});