/* Configuración del DataTable */
$(document).ready(function () {
    $('#documentsTable').DataTable({
        responsive: {
            details: {
                type: 'column',
                target: 'tr'
            }
        },
        language: {
            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún dato disponible en esta tabla",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",
            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            },
            "oAria": {
                "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
                "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            },
            responsive: {
                details: {
                    display: $.fn.dataTable.Responsive.display.childRowImmediate,
                    type: 'none',
                    target: ''
                }
            }
        },
        order: [[1, 'desc']],
        pageLength: 10,
        autoWidth: false,
        columnDefs: [
            {
                responsivePriority: 1,
                targets: 0,  // Nombre del documento
                width: 40%
            },
            {
                responsivePriority: 4,
                targets: 1,  // Fecha de subida
                width: 20%
            },
            {
                responsivePriority: 2,
                targets: 2,  // Estado
                className: 'status-column',
                width: 10%
            },
            {
                responsivePriority: 1,
                targets: 3, // Acciones
                orderable: false,
                width: 30%

            }
        ]
    });
});

// Cambiar el estado de un documento mediante el formulario oculto
function changeStatus(id, status) {
    document.getElementById('documentId').value = id;
    document.getElementById('documentStatus').value = status;
    document.getElementById('statusForm').submit();
}

// Inicializar todos los tooltips
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});