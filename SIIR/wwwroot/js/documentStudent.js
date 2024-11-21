$(document).ready(function () {
    // Configuración global de toastr
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "timeOut": "5000",
        "extendedTimeOut": "2000"
    };

    // Verificar mensajes pendientes
    const pendingMessage = localStorage.getItem('documentMessage');
    const messageType = localStorage.getItem('documentMessageType');
    if (pendingMessage) {
        toastr[messageType || 'success'](pendingMessage);
        localStorage.removeItem('documentMessage');
        localStorage.removeItem('documentMessageType');
    }

    // Constantes para mensajes
    const MESSAGES = {
        SELECT_FILE: 'Por favor seleccione un archivo',
        CONFIRM_DELETE: '¿Está seguro de que desea eliminar este documento? Esta acción no se puede deshacer.',
        GENERIC_ERROR: 'Error al procesar la solicitud'
    };

    // Inicializar modal
    const uploadModal = new bootstrap.Modal(document.getElementById('uploadModal'));

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

    // Función para abrir el modal de subida
    window.openUploadModal = function (documentId, documentName) {
        document.getElementById('documentCatalogId').value = documentId;
        document.getElementById('uploadModalLabel').textContent = `Subir documento: ${documentName}`;
        uploadModal.show();
    }

    // Función para guardar documento
    window.saveDocument = async function () {
        const fileInput = document.getElementById('documentFile');
        const documentCatalogId = document.getElementById('documentCatalogId').value;

        if (!fileInput.files[0]) {
            toastr.warning(MESSAGES.SELECT_FILE);
            return;
        }

        const formData = new FormData();
        formData.append('file', fileInput.files[0]);
        formData.append('documentCatalogId', documentCatalogId);
        formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

        try {
            const response = await fetch('/Student/Document/SaveDocument', {
                method: 'POST',
                body: formData
            });
            const result = await response.json();

            if (result.success) {
                uploadModal.hide();
                toastr.success(result.message || 'Documento guardado exitosamente');
                location.reload();
            } else {
                toastr.error(result.message || 'Error al guardar el documento');
            }
        } catch (error) {
            console.error('Error:', error);
            toastr.error('Error al guardar el documento');
        }
    }

    // Función para descargar documento
    window.downloadDocument = async function (documentId) {
        const button = $(`.btn-download-document[data-document-id="${documentId}"]`);
        const originalHtml = button.html();
        updateButtonState(button, true, 'Descargando...', originalHtml);

        try {
            const response = await fetch(`/Student/Document/DownloadDocument?documentCatalogId=${documentId}`);
            const contentDisposition = response.headers.get('content-disposition');
            let filename;

            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                    try {
                        filename = decodeURIComponent(filename);
                    } catch (e) {
                        console.warn('Error decodificando nombre de archivo:', e);
                    }
                }
            }

            if (!filename) {
                filename = `documento_${documentId}.pdf`;
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = filename.split(';')[0].trim();
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
            toastr.success('Documento descargado exitosamente');
        } catch (error) {
            console.error('Error:', error);
            toastr.error('Error al descargar el documento');
        } finally {
            updateButtonState(button, false, '', originalHtml);
        }
    }

});