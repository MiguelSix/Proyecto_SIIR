$(document).ready(function () {
    // Configuración global de toastr
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "timeOut": "5000",
        "extendedTimeOut": "2000"
    };
    // Verificar si hay un mensaje pendiente al cargar la página
    const pendingMessage = localStorage.getItem('documentMessage');
    const messageType = localStorage.getItem('documentMessageType');
    if (pendingMessage) {
        toastr[messageType || 'success'](pendingMessage);
        localStorage.removeItem('documentMessage');
        localStorage.removeItem('documentMessageType');
    }
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
        toastr.error(errorMessage);
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
            toastr.warning(MESSAGES.SELECT_FILE);
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
                    toastr.success(response.message || 'Documento guardado exitosamente');
                    location.reload();
                } else {
                    toastr.error(response.message || 'Error al guardar el documento');
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, status, error, 'Error al guardar el documento');
            }
        });
    });

    $('.btn-download-document').click(function (e) {
        e.preventDefault();
        const button = $(this);
        const documentId = button.data('document-id');
        const originalHtml = button.html();
        updateButtonState(button, true, 'Descargando...', originalHtml);

        fetch(`${window.location.origin}/Student/Document/DownloadDocument?documentCatalogId=${documentId}`, {
            method: 'GET',
        })
            .then(response => {
                // Obtener el nombre del archivo de la cabecera Content-Disposition
                const contentDisposition = response.headers.get('content-disposition');
                let filename;

                if (contentDisposition) {
                    // Intentar obtener el nombre del archivo de la cabecera
                    const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                    if (filenameMatch && filenameMatch[1]) {
                        filename = filenameMatch[1].replace(/['"]/g, '');
                        // Decodificar el nombre del archivo si es necesario
                        try {
                            filename = decodeURIComponent(filename);
                        } catch (e) {
                            // Si falla la decodificación, usar el nombre tal cual
                            console.warn('Error decodificando nombre de archivo:', e);
                        }
                    }
                }

                // Si no se pudo obtener el nombre del archivo, usar uno por defecto
                if (!filename) {
                    filename = `documento_${documentId}.pdf`;
                }

                return response.blob().then(blob => {
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.style.display = 'none';
                    a.href = url;
                    // Limpiar el nombre del archivo de cualquier carácter especial adicional
                    a.download = filename.split(';')[0].trim();
                    document.body.appendChild(a);
                    a.click();
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                    toastr.success('Documento descargado exitosamente');
                });
            })
            .catch(error => {
                console.error('Error:', error);
                toastr.error('Error al descargar el documento');
            })
            .finally(() => {
                updateButtonState(button, false, '', originalHtml);
            });
    });

    // Manejador para eliminar documentos
    $('.btn-delete-document').click(function (e) {
        e.preventDefault();
        const button = $(this);
        const documentId = button.data('document-id');
        const originalHtml = button.html();

        // Usar SweetAlert para la confirmación
        swal({
            title: "¿Está seguro de borrar?",
            text: MESSAGES.CONFIRM_DELETE,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Sí, borrar!",
            closeOnconfirm: true
        }, function () {
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
                        toastr.success(response.message || 'Documento eliminado exitosamente');
                        location.reload();
                    } else {
                        toastr.error(response.message || 'Error al eliminar el documento');
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
});