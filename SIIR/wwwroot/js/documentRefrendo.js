$(document).ready(function () {
    // Configuración de toastr
    function toastrConfiguration() {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    }

    // Configurar toastr al inicio
    toastrConfiguration();

    // Obtener el token antiforgery
    var token = $('input[name="__RequestVerificationToken"]').val();

    // Manejar el checkbox "Seleccionar todos"
    $('#selectAll').change(function () {
        $('.document-checkbox').prop('checked', $(this).prop('checked'));
    });

    // Cargar documentos en el modal
    function cargarDocumentosRefrendo() {
        $.ajax({
            url: '/Admin/DocumentCatalog/GetAll',
            type: 'GET',
            success: function (response) {
                console.log('Respuesta GetAll:', response); // Debug
                var tbody = $('#documentosRefrendo tbody');
                tbody.empty();
                response.data.forEach(function (doc) {
                    console.log('Procesando documento:', doc); // Debug
                    tbody.append(`
                        <tr>
                            <td>
                                <input type="checkbox" class="form-check-input document-checkbox" 
                                       value="${doc.id}" data-name="${doc.name}"
                                       id="doc-${doc.id}">
                            </td>
                            <td>${doc.name}</td>
                            <td>${doc.description || ''}</td>
                        </tr>
                    `);
                });
            },
            error: function (xhr, status, error) {
                console.error('Error al cargar documentos:', error);
                toastr.error('Error al cargar los documentos');
            }
        });
    }

    // Abrir modal y cargar documentos
    $(document).on('click', '#btnAbrirRefrendo', function () {
        cargarDocumentosRefrendo();
        $('#refrendoModal').modal('show');
    });

    // Manejar el refrendo de documentos
    $('#btnRefrendarDocumentos').click(function () {
        var documentosSeleccionados = [];
        $('.document-checkbox:checked').each(function () {
            var id = parseInt($(this).val());
            console.log('Checkbox seleccionado - valor:', $(this).val(), 'convertido a:', id); // Debug
            documentosSeleccionados.push(id);
        });

        console.log('Documentos seleccionados:', documentosSeleccionados);

        if (documentosSeleccionados.length === 0) {
            swal({
                title: "Atención",
                text: "Debe seleccionar al menos un documento para refrendar",
                type: "warning"
            });
            return;
        }

        swal({
            title: "¿Está seguro?",
            text: "Se notificará a todos los estudiantes que deben volver a subir los documentos seleccionados",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Sí, refrendar!",
            cancelButtonText: "Cancelar",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                $.ajax({
                    url: '/Admin/DocumentCatalog/RefrendoDocumentos',
                    type: 'POST',
                    data: JSON.stringify(documentosSeleccionados),
                    contentType: 'application/json',
                    headers: {
                        'RequestVerificationToken': token
                    },
                    success: function (response) {
                        console.log('Respuesta del servidor:', response); // Debug
                        if (response.success) {
                            toastr.success('Refrendo exitoso. Se enviará una notificación a los estudiantes.');
                            $('#refrendoModal').modal('hide');
                            if (typeof documentosTable !== 'undefined') {
                                documentosTable.ajax.reload();
                            }
                        } else {
                            toastr.error(response.message || 'Ha ocurrido un error al procesar la solicitud');
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error en la petición:', {
                            status: status,
                            error: error,
                            response: xhr.responseText
                        });
                        toastr.error('Ha ocurrido un error al procesar la solicitud');
                    }
                });
            }
        });
    });
});