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

    // Función para actualizar estado del botón
    function updateButtonState(button, isLoading, loadingText, originalHtml) {
        button.prop('disabled', isLoading);
        button.html(isLoading ?
            `<i class="fa-solid fa-spinner fa-spin"></i> ${loadingText}` :
            originalHtml
        );
    }

    // Manejador para guardar documentos
    $('.btn-save-document').click(function (e) {
        e.preventDefault();
        const documentId = $(this).data('document-id');
        const fileInput = $(`#file_${documentId}`);

        if (fileInput[0].files.length === 0) {
            alert(MESSAGES.SELECT_FILE);
            return;
        }

        const formData = new FormData();
        formData.append('file', fileInput[0].files[0]);
        formData.append('documentCatalogId', documentId);
        formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

        $.ajax({
            url: '@Url.Action("SaveDocument", "Document")',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    console.log('Documento guardado exitosamente');
                    alert(response.message);
                    location.reload();
                } else {
                    handleAjaxError(null, null, response.message, 'Error al guardar el documento');
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, status, error, 'Error al guardar el documento');
            }
        });
    });

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