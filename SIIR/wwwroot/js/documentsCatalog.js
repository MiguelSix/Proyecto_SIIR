﻿$(document).ready(function () {
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
                var tbody = $('#documentosRefrendo tbody');
                tbody.empty();
                response.data.forEach(function (doc) {
                    tbody.append(`
                        <tr>
                            <td>
                                <input type="checkbox" class="form-check-input document-checkbox" 
                                       value="${doc.id}" data-name="${doc.name}">
                            </td>
                            <td>${doc.name}</td>
                            <td>${doc.description || ''}</td>
                        </tr>
                    `);
                });
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
            documentosSeleccionados.push({
                id: $(this).val(),
                name: $(this).data('name')
            });
        });

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
            closeOnConfirm: true
        }, function () {
            $.ajax({
                url: '/Admin/DocumentCatalog/RefrendoDocumentos',
                type: 'POST',
                data: { documentIds: documentosSeleccionados.map(d => d.id) },
                success: function (response) {
                    if (response.success) {
                        toastr.success('Refrendo exitoso. Se enviará una notificación a los estudiantes.');
                        $('#refrendoModal').modal('hide');
                    } else {
                        toastr.error(response.message || 'Ha ocurrido un error al procesar la solicitud');
                    }
                },
                error: function () {
                    toastr.error('Ha ocurrido un error al procesar la solicitud');
                }
            });
        });
    });
});